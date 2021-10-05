using SMPlayer.Helpers.VoiceAssistant;
using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace SMPlayer.Helpers
{
    public class VoiceAssistantHelper
    {
        public static List<Action<SpeechRecognizer, SpeechRecognizerStateChangedEventArgs>> StateChangedListeners = new List<Action<SpeechRecognizer, SpeechRecognizerStateChangedEventArgs>>();
        private static SpeechRecognizer Recognizer = new SpeechRecognizer(Helper.CurrentLanguage); // SpeechRecognizer.SystemSpeechLanguage
        private static SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        private static IVoiceAssistantCommandHandler CommandHandler;
        private static bool IsRecognizing = false;

        public static async void Init()
        {
            switch (Helper.CurrentLanguage.LanguageTag)
            {
                case Helper.Language_CN:
                    CommandHandler = new VoiceAssistantChineseHelper();
                    break;
                default:
                    CommandHandler = new VoiceAssistantEnglishHelper();
                    break;
            }

            Recognizer.StateChanged += (sender, args) =>
            {
                foreach (var listener in StateChangedListeners) listener.Invoke(sender, args);
            };
            Recognizer.UIOptions.AudiblePrompt = Helper.LocalizeMessage("VoiceAssistantAudiblePrompt");
            Recognizer.UIOptions.ExampleText = Helper.LocalizeMessage("VoiceAssistantExampleText");
            Recognizer.UIOptions.IsReadBackEnabled = false;
            await Recognizer.CompileConstraintsAsync();
            //https://www.cnblogs.com/Aran-Wang/p/4816313.html
            //StorageFile commandSet = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceAssistantCommandSet.xml"));
            //await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(commandSet);
        }

        public static async void StopRecognition()
        {
            try
            {
                if (Recognizer.State != SpeechRecognizerState.Idle)
                {
                    await Recognizer.StopRecognitionAsync();
                }
            }
            catch (Exception e)
            {
                Helper.LogException(e);
            }
        }

        public static async void AwakeVoiceAssistant()
        {
            if (IsRecognizing)
            {
                return;
            }
            IsRecognizing = true;
            SpeechRecognitionResult speechRecognitionResult;
            try
            {
                speechRecognitionResult = await Recognizer.RecognizeWithUIAsync();
            }
            catch (Exception e)
            {
                Helper.LogException(e);
                await ShowAcceptPrivacyDialog();
                return;
            }
            finally
            {
                IsRecognizing = false;
            }
            try
            {
                if (speechRecognitionResult != null)
                {
                    HandleCommand(speechRecognitionResult.Text);
                    //HandleCommand(speechRecognitionResult);
                }
            }
            catch (Exception e)
            {
                Helper.LogException(e);
                Speak("VoiceAssitantError");
            }
        }

        private static async Task ShowAcceptPrivacyDialog()
        {
            ContentDialog Dialog = new ContentDialog()
            {
                Title = Helper.LocalizeMessage("UnacceptedSpeechPrivacyPolicy"),
                Content = Helper.LocalizeMessage("OpenSettingsAndTurnOnSpeechInput"),
                PrimaryButtonText = Helper.LocalizeMessage("NeverMind"),
                SecondaryButtonText = Helper.LocalizeMessage("GoToSettings")
            };
            if (await Dialog.ShowAsync() == ContentDialogResult.Secondary)
            {
                // https://stackoverflow.com/questions/42391526/exception-the-speech-privacy-policy-was-not-accepted-prior-to-attempting-a-spee
                if (!await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-speechtyping")))
                {
                    await new ContentDialog()
                    {
                        Title = Helper.LocalizeMessage("ErrorOccurs"),
                        Content = Helper.LocalizeMessage("OpenSettingsFailed"),
                        PrimaryButtonText = Helper.LocalizeMessage("Close"),
                    }.ShowAsync();
                }
            }
        }

        public static void Speak(string message, params object[] args)
        {
            SpeakRaw(Helper.LocalizeMessage(message, args));
        }

        public static async void SpeakRaw(string message)
        {
            // 创建一个文本转语音的流。
            var stream = await synthesizer.SynthesizeTextToStreamAsync(message);

            // 将流发送到 MediaElement 控件并播放。
            MediaElement mediaElement = Helper.GetMainPageContainer().GetMediaElement();
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }


        public static void HandleCommand(string text)
        {
            CommandHandler.HandleCommand(text);
        }

        public static void SpeakNoResults(string text)
        {
            Speak("VoiceAssistantNoSearchResults", text);
        }

        public static void SpeakNotUnderstand()
        {
            Speak("VoiceAssistantCannotUnderstand");
        }
    }

    public interface IVoiceAssistantCommandHandler
    {
        void HandleCommand(string text);
    }

}
