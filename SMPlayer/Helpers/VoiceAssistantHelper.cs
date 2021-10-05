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
        private static SpeechRecognizer recognizer = new SpeechRecognizer(Helper.CurrentLanguage);
        private static SpeechSynthesizer synthesizer = new SpeechSynthesizer();
        private static bool IsRecognizing = false;

        public static async void Init()
        {
            recognizer.UIOptions.AudiblePrompt = Helper.LocalizeMessage("VoiceAssistantAudiblePrompt");
            recognizer.UIOptions.ExampleText = Helper.LocalizeMessage("VoiceAssistantExampleText");
            recognizer.UIOptions.IsReadBackEnabled = false;
            AddConstraints(recognizer);
            await recognizer.CompileConstraintsAsync();
        }

        private static async void AddConstraints(SpeechRecognizer recognizer)
        {
            // 获取文件。
            //StorageFile commandSet = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceAssistantCommandSet.xml"));
            //await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(commandSet);
            var test3 = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/folder.png"));
            var test2 = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/zh.grxml"));
            var grammarFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Grammars/zh.grxml"));

            // 创建 SRGS 文件约束。
            var srgsConstraint = new SpeechRecognitionGrammarFileConstraint(grammarFile);

            // 添加约束。
            recognizer.Constraints.Add(srgsConstraint);
        }

        public static async void AwakeVoiceAssistant()
        {
            if (IsRecognizing)
            {
                await recognizer.StopRecognitionAsync();
                return;
            }
            IsRecognizing = true;
            Init();
            SpeechRecognitionResult speechRecognitionResult;
            try
            {
                speechRecognitionResult = await recognizer.RecognizeWithUIAsync();
            }
            catch (Exception)
            {
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

        public static void HandleCommand(SpeechRecognitionResult speechRecognitionResult)
        {
            IReadOnlyList<string> rules = speechRecognitionResult.RulePath;
            if (rules == null || rules.Count == 0)
            {
                Speak("VoiceAssistantCannotUnderstand");
                return;
            }
            rules.Print();
            if (rules[0].Equals("play"))
            {
                if (rules.Count == 1)
                {
                    MediaHelper.QuickPlay();
                    return;
                }
                if (rules.Count == 2)
                {
                    Play(rules[1]);
                }
            }
        }

        public static void HandleCommand(string text)
        {
            string play = Helper.Localize("Play");
            if (text.StartsWith(play))
            {
                string keyword = text.Substring(play.Length);
                Play(keyword);
            }
        }

        private static async void Play(string keyword)
        {
            var result = await SearchHelper.Search(keyword);
            if (result.Result == null)
            {
                Speak("VoiceAssistantNoSearchResults", keyword);
                return;
            }
            switch (result.SearchType)
            {
                case SearchType.Artists:
                    Playlist artist = result.Result as Playlist;
                    MediaHelper.ShuffleAndPlay(artist.Songs);
                    //Speak("SearchResultArtist", artist.Name);
                    break;
                case SearchType.Albums:
                    AlbumView album = result.Result as AlbumView;
                    MediaHelper.ShuffleAndPlay(album.Songs);
                    //Speak("SearchResultAlbum", album.Name);
                    break;
                case SearchType.Playlists:
                    AlbumView playlist = result.Result as AlbumView;
                    MediaHelper.ShuffleAndPlay(playlist.Songs);
                    //Speak("SearchResultPlaylist", playlist.Name);
                    break;
                case SearchType.Folders:
                    GridFolderView folder = result.Result as GridFolderView;
                    MediaHelper.ShuffleAndPlay(folder.Songs);
                    //Speak("SearchResultFolder", folder.Name);
                    break;
                case SearchType.Songs:
                    Music music = result.Result as Music;
                    MediaHelper.SetMusicAndPlay(music);
                    //Speak("SearchResultMusic", music.Name);
                    break;
            }
        }
    }
}
