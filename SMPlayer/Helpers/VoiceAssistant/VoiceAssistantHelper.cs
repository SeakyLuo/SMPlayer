using SMPlayer.Helpers.VoiceAssistant;
using SMPlayer.Models;
using SMPlayer.Models.VO;
using SMPlayer.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SMPlayer.Helpers
{
    public class VoiceAssistantHelper
    {
        private const string Hint_PlaySomeonesMusic = "VoiceAssistantHints1", Hint_PlayMusicInAlbum = "VoiceAssistantHints2", Hint_QuickPlay = "VoiceAssistantHints3";
        private static readonly List<string> VoiceAssistantHints = new List<string>() { Hint_PlaySomeonesMusic, Hint_PlayMusicInAlbum, Hint_QuickPlay };
        public static List<Action<SpeechRecognizer, VoiceAssistantEventArgs>> StateChangedListeners = new List<Action<SpeechRecognizer, VoiceAssistantEventArgs>>();
        private static SpeechRecognizer Recognizer;
        private static SpeechSynthesizer Synthesizer = new SpeechSynthesizer();
        private static IVoiceAssistantCommandHandler CommandHandler;
        private static bool IsRecognizing = false;

        public static string GetRandomHint()
        {
            string hint = VoiceAssistantHints.RandItem();
            IEnumerable<Music> allSongs;
            switch (hint)
            {
                case Hint_PlaySomeonesMusic:

                    allSongs = MusicService.AllSongs;
                    if (allSongs.IsEmpty())
                    {
                        break;
                    }
                    string artist = allSongs.RandItem().Artist;
                    if (string.IsNullOrEmpty(artist) || artist.Length > 30)
                    {
                        artist = allSongs.RandItem().Artist;
                    }
                    if (string.IsNullOrEmpty(artist) || artist.Length > 30)
                    {
                        break;
                    }
                    else
                    {
                        return Helper.LocalizeText(Hint_PlaySomeonesMusic, artist);
                    }
                case Hint_PlayMusicInAlbum:
                    allSongs = MusicService.AllSongs;
                    if (allSongs.IsEmpty())
                    {
                        break;
                    }
                    string album = allSongs.RandItem().Album;
                    if (string.IsNullOrEmpty(album) || album.Length > 30)
                    {
                        album = allSongs.RandItem().Album;
                    }
                    if (string.IsNullOrEmpty(album) || album.Length > 30)
                    {
                        break;
                    }
                    else
                    {
                        return Helper.LocalizeText(Hint_PlayMusicInAlbum, album);
                    }
            }
            return Helper.LocalizeText(Hint_QuickPlay);
        }

        public static async void Init()
        {
            if (Settings.settings == null)
            {
                return;
            }
            await SetLanguage(Settings.settings.VoiceAssistantPreferredLanguage);
            //https://www.cnblogs.com/Aran-Wang/p/4816313.html
            //StorageFile commandSet = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///VoiceAssistantCommandSet.xml"));
            //await VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(commandSet);
        }

        public static async Task<bool> SetLanguage(VoiceAssistantLanguage language)
        {
            try
            {
                Recognizer = new SpeechRecognizer(ConvertLanguage(language));
                Recognizer.UIOptions.IsReadBackEnabled = false;
                Recognizer.StateChanged += (sender, args) =>
                {
                    Log.Info("state: " + args.State);
                    VoiceAssistantEventArgs a = new VoiceAssistantEventArgs { State = args.State };
                    foreach (var listener in StateChangedListeners)
                    {
                        try
                        {
                            listener.Invoke(sender, a);
                        }
                        catch (Exception e)
                        {
                            Log.Warn($"SetLanguage invoide StateChangedListeners failed {e}");
                        }
                    }
                };
                await Recognizer.CompileConstraintsAsync();
            }
            catch (Exception e)
            {
                Log.Warn($"set languange failed {e}");
                return false;
            }
            switch (language)
            {
                case VoiceAssistantLanguage.Chinese:
                    CommandHandler = new VoiceAssistantChineseHelper();
                    break;
                default:
                    CommandHandler = new VoiceAssistantEnglishHelper();
                    break;
            }
            return true;
        }

        private static Language ConvertLanguage(VoiceAssistantLanguage language)
        {
            switch (language)
            {
                case VoiceAssistantLanguage.Chinese:
                    return new Language(Helper.Language_CN);
                default:
                    return new Language(Helper.Language_EN);
            }
        }

        public static VoiceAssistantLanguage ConvertLanguage(Language language)
        {
            switch (language.LanguageTag)
            {
                case Helper.Language_CN:
                    return VoiceAssistantLanguage.Chinese;
                default:
                    return VoiceAssistantLanguage.English;
            }
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
                Log.Warn("StopRecognition.Exception", e);
            }
        }

        public static async Task<SpeechRecognitionResult> Recognize()
        {
            if (IsRecognizing)
            {
                return null;
            }
            IsRecognizing = true;
            try
            {
                return await Recognizer.RecognizeWithUIAsync();
            }
            catch (Exception e)
            {
                Log.Warn("Recognize.RecognizeException {0}", e);
                await ShowAcceptPrivacyDialog();
                return null;
            }
            finally
            {
                IsRecognizing = false;
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
            var stream = await Synthesizer.SynthesizeTextToStreamAsync(message);
            
            // 将流发送到 MediaElement 控件并播放。
            MediaElement mediaElement = Helper.GetMainPageContainer().GetMediaElement();
            mediaElement.SetSource(stream, stream.ContentType);
            mediaElement.Play();
        }

        public static async Task HandleCommand(SpeechRecognitionResult result)
        {
            try
            {
                if (result != null)
                {
                    await HandleCommand(result.Text);
                }
            }
            catch (Exception e)
            {
                Log.Warn("Recognize.Exception {0}", e);
                Speak("VoiceAssitantError");
            }
        }

        private static async Task HandleCommand(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }
            CommandResult result = CommandHandler.Handle(text);
            switch (result.Type)
            {
                case MatchType.Play:
                    MusicPlayer.Play();
                    break;
                case MatchType.PlayMusic:
                    PlayMusic(result.Param as string);
                    break;
                case MatchType.PlayByArtist:
                    PlayByArtist(result.Param as ByArtistRequest);
                    break;
                case MatchType.PlayByArtistAndAlbum:
                    PlayByArtistAndAlbum(result.Param as ByArtistRequest);
                    break;
                case MatchType.PlayByArtistAndMusic:
                    PlayByArtistAndMusic(result.Param as ByArtistRequest);
                    break;
                case MatchType.PlayMusicInAlbum:
                    PlayMusicInAlbum(result.Param as ByArtistRequest);
                    break;
                case MatchType.PlayMusicInPlaylist:
                    PlayMusicInPlaylist(result.Param as ByArtistRequest);
                    break;
                case MatchType.PlayMusicInFolder:
                    PlayMusicInFolder(result.Param as ByArtistRequest);
                    break;
                case MatchType.PlayArtist:
                    PlayArtist(result.Param as string);
                    break;
                case MatchType.PlayAlbum:
                    PlayAlbum(result.Param as string);
                    break;
                case MatchType.PlayPlaylist:
                    PlayPlaylist(result.Param as string);
                    break;
                case MatchType.PlayFolder:
                    PlayFolder(result.Param as string);
                    break;
                case MatchType.QuickPlay:
                    MusicPlayer.QuickPlay();
                    break;
                case MatchType.PlayByArtistOrMusic:
                    PlayByArtistOrMusic(result.Param as ByArtistRequest);
                    break;
                case MatchType.SearchAndPlay:
                    SearchAndPlay(result.Param as string);
                    break;
                case MatchType.Search:
                    Search(result.Param as string);
                    break;
                case MatchType.Pause:
                    MusicPlayer.Pause();
                    break;
                case MatchType.Previous:
                    MusicPlayer.MovePrev();
                    MusicPlayer.Play();
                    break;
                case MatchType.Next:
                    MusicPlayer.MoveNext();
                    MusicPlayer.Play();
                    break;
                case MatchType.Mute:
                    Helper.GetMainPageContainer().GetMediaControl().SetMuted(true);
                    break;
                case MatchType.UnMute:
                    Helper.GetMainPageContainer().GetMediaControl().SetMuted(true);
                    break;
                case MatchType.Help:
                    await new Dialogs.VoiceAssistantHelpDialog().ShowAsync();
                    break;
                case MatchType.ChangeVolume:
                    ChangeVolume(result.Param as VolumeRequest);
                    break;
                case MatchType.Nothing:
                    break;
                case MatchType.MatchNone:
                    SpeakNotUnderstand();
                    break;
            }
        }

        public static void SpeakNoResults(string text)
        {
            Speak("VoiceAssistantNoSearchResults", text);
        }

        public static void SpeakNotUnderstand()
        {
            Speak("VoiceAssistantCannotUnderstand");
        }

        private static void PlayMusic(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                MusicPlayer.QuickPlay();
                return;
            }
            var list = SearchHelper.SearchSongs(MusicService.AllSongs, text);
            if (list.IsEmpty())
            {
                SpeakNoResults(text);
                return;
            }
            MusicPlayer.AddNextAndPlay(list.First().Entity);
        }

        private static void PlayArtist(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                RandomPlayHelper.PlayArtist();
                return;
            }
            var list = SearchHelper.SearchArtists(MusicService.AllSongs, text);
            if (list.IsEmpty())
            {
                SpeakNoResults(text);
                return;
            }
            MusicPlayer.ShuffleAndPlay(list.First().Entity.Songs);
        }

        private static void PlayAlbum(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                RandomPlayHelper.PlayAlbum();
                return;
            }
            var list = SearchHelper.SearchAlbums(MusicService.AllSongs, text);
            if (list.IsEmpty())
            {
                SpeakNoResults(text);
                return;
            }
            MusicPlayer.ShuffleAndPlay(list.First().Entity.Songs);
        }

        private static void PlayPlaylist(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                RandomPlayHelper.PlayPlaylist();
                return;
            }
            var list = SearchHelper.SearchPlaylists(PlaylistService.AllPlaylists, text);
            if (list.IsEmpty())
            {
                SpeakNoResults(text);
                return;
            }
            MusicPlayer.ShuffleAndPlay(list.First().Entity.Songs);
        }

        private static void PlayFolder(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                RandomPlayHelper.PlayFolder();
                return;
            }
            var list = SearchHelper.SearchFolders(StorageService.AllFolders, text);
            if (list.IsEmpty())
            {
                SpeakNoResults(text);
                return;
            }
            MusicPlayer.ShuffleAndPlay(StorageService.FindFolder(list.First().Entity.Id).Songs);
        }

        private static async void PlayByArtistOrMusic(ByArtistRequest request)
        {
            var byArtistResult = SearchHelper.SearchArtists(MusicService.AllSongs, request.Artist);
            var originalResult = await SearchHelper.Search(request.Original);
            if (byArtistResult.IsEmpty() && originalResult == null)
            {
                SpeakNoResults(request.Original);
                return;
            }
            if (byArtistResult.IsNotEmpty())
            {
                MusicPlayer.ShuffleAndPlay(byArtistResult.First().Entity.Songs);
            }
            if (originalResult != null)
            {
                PlayBySearch(originalResult, request.Original);
            }
        }

        private static async void PlayByArtist(ByArtistRequest request, EvaluateResult byArtistResult)
        {
            var originalResult = await SearchHelper.Search(request.Original);
            if (byArtistResult == null && originalResult == null)
            {
                SpeakNoResults(request.Original);
                return;
            }
            double byArtistScore = byArtistResult == null ? 0 : byArtistResult.Score;
            double originalScore = originalResult == null ? 0 : originalResult.Score;
            if (byArtistScore >= originalScore)
            {
                PlayBySearch(byArtistResult, request.Item);
            }
            else
            {
                PlayBySearch(originalResult, request.Original);
            }
        }

        private static async void PlayByArtist(ByArtistRequest request)
        {
            var byArtistResult = await SearchHelper.SearchByArtist(request.Artist, request.Item);
            PlayByArtist(request, byArtistResult);
        }

        private static async void PlayByArtistAndAlbum(ByArtistRequest request)
        {
            var byArtistResult = await SearchHelper.SearchByArtistAlbum(request.Artist, request.Item);
            PlayByArtist(request, byArtistResult);
        }

        private static async void PlayByArtistAndMusic(ByArtistRequest request)
        {
            var byArtistResult = await SearchHelper.SearchByArtistMusic(request.Artist, request.Item);
            PlayByArtist(request, byArtistResult);
        }

        private static async void PlayMusicInAlbum(ByArtistRequest request)
        {
            var byArtistResult = await SearchHelper.SearchAlbumMusic(request.Artist, request.Item);
            PlayByArtist(request, byArtistResult);
        }

        private static async void PlayMusicInPlaylist(ByArtistRequest request)
        {
            var byArtistResult = await SearchHelper.SearchPlaylistMusic(request.Artist, request.Item);
            PlayByArtist(request, byArtistResult);
        }

        private static async void PlayMusicInFolder(ByArtistRequest request)
        {
            var byArtistResult = await SearchHelper.SearchFolderMusic(request.Artist, request.Item);
            PlayByArtist(request, byArtistResult);
        }

        private static async void SearchAndPlay(string text)
        {
            var result = await SearchHelper.Search(text);
            if (result == null)
            {
                SpeakNoResults(text);
                return;
            }
            PlayBySearch(result, text);
        }

        private static bool IsBadSearch(string expected, string given, EvaluateResult result)
        {
            return expected != given && result.Score < 80;
        }

        private static void PlayBySearch(EvaluateResult result, string text)
        {
            switch (result.SearchType)
            {
                case EntityType.Artist:
                    Artist artist = result.Entity as Artist;
                    MusicPlayer.ShuffleAndPlay(artist.Songs);
                    if (IsBadSearch(text, artist.Name, result))
                    {
                        Speak("SearchResultArtist", artist.Name);
                    }
                    break;
                case EntityType.Album:
                    Album album = result.Entity as Album;
                    MusicPlayer.ShuffleAndPlay(album.Songs);
                    if (IsBadSearch(text, album.Name, result))
                    {
                        Speak("SearchResultAlbum", album.Name);
                    }
                    break;
                case EntityType.Playlist:
                    AlbumView playlist = result.Entity as AlbumView;
                    MusicPlayer.ShuffleAndPlay(playlist.Songs);
                    if (IsBadSearch(text, playlist.Name, result))
                    {
                        Speak("SearchResultPlaylist", playlist.Name);
                    }
                    break;
                case EntityType.Folder:
                    FolderTree folder = result.Entity as FolderTree;
                    MusicPlayer.ShuffleAndPlay(folder.Songs);
                    if (IsBadSearch(text, folder.Name, result))
                    {
                        Speak("SearchResultFolder", folder.Name);
                    }
                    break;
                case EntityType.Song:
                    Music music = result.Entity as Music;
                    MusicPlayer.AddNextAndPlay(music);
                    if (IsBadSearch(text, music.Name, result))
                    {
                        Speak("SearchResultMusic", music.Name);
                    }
                    break;
            }
        }

        public static void ChangeVolume(VolumeRequest request)
        {
            int newVolume;
            if (request.To)
            {
                newVolume = (int) request.Value;
            }
            else
            {
                double currentVolume = MusicPlayer.Player.Volume * 100;
                int positive = request.TurnUp ? 1 : -1;
                double percent = request.Percentage ? MusicPlayer.Player.Volume : 1;
                newVolume = (int) (currentVolume + positive * request.Value * percent);
            }
            Helper.GetMainPageContainer().GetMediaControl().SetVolume(newVolume);
            Speak("VoiceAssistantNewVolume", newVolume);
        }

        private static void Search(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                SpeakNotUnderstand();
                return;
            }
            object page = (Window.Current.Content as Frame)?.Content;
            if (page is NowPlayingFullPage fullPage)
            {
                fullPage.GoBack();
            }
            if (page is MiniModePage miniPage)
            {
                MiniModePage.ExitMiniMode();
            }
            MainPage.Instance.Search(text);
        }

        public static double FractionToDouble(string fraction)
        {
            string[] nums = fraction.Split("/");
            return double.Parse(nums[0]) / double.Parse(nums[1]);
        }

        public const RegexOptions Option = RegexOptions.IgnoreCase;

        public static MatchCollection Matches(string text, string pattern)
        {
            return Regex.Matches(text, pattern, Option);
        }
    }

    public interface IVoiceAssistantCommandHandler
    {
        CommandResult Handle(string text);
    }

    public class VoiceAssistantEventArgs
    {
        public SpeechRecognizerState State { get; set; }
        public string Text { get; set; }
    }

    public enum MatchType
    {
        Play, PlayMusic, PlayArtist, PlayAlbum, PlayPlaylist, PlayFolder, SearchAndPlay, QuickPlay, PlayByArtistOrMusic,
        PlayByArtist, PlayByArtistAndMusic, PlayByArtistAndAlbum, PlayMusicIn, PlayMusicInAlbum, PlayMusicInFolder, PlayMusicInPlaylist,
        Pause, Previous, Next, ChangeVolume, Search, Mute, UnMute, Help,
        MatchNone, Nothing
    }

    public class CommandResult
    {
        public MatchType Type { get; set; }
        public object Param { get; set; }
    }

    public class VolumeRequest
    {
        public bool To { get; set; }
        public bool TurnUp { get; set; }
        public bool Percentage { get; set; }
        public double Value { get; set; }
    }

    public class ByArtistRequest
    {
        public string Artist { get; set; }
        public string Item { get; set; }
        public string Original { get; set; }

        public ByArtistRequest() { }

        public ByArtistRequest(MatchCollection collection, string splitter)
        {
            string original = collection.GetValue();
            int index = original.IndexOf(splitter);

            Artist = original.Substring(0, index);
            Item = original.Substring(index + splitter.Length);
            Original = original;
        }
    }
}
