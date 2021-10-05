using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace SMPlayer.Helpers.VoiceAssistant
{
    class VoiceAssistantChineseHelper : IVoiceAssistantCommandHandler
    {
        private const string Text_Play = "播放", Text_Music = "音乐", Text_Song = "歌曲", Text_Artist = "歌手", Text_Album = "专辑", Text_PlayList = "列表", Text_Folder = "文件夹",
                             Text_QuickPlay = "快速播放", Text_Pause = "暂停", Text_Prev = "前一首", Text_Prev2 = "上一首", Text_Next = "后一首", Text_Next2 = "后一首",
                             Text_Volume = "音量", Text_Sound = "声音", Text_TurnUp = "调高", Text_TurnDown = "调低", Text_VolumeTo = "至",
                             Text_Mute = "静音", Text_UnMute = "取消静音",
                             Text_Search = "搜索",
                             Text_Help = "帮助";

        public async void HandleCommand(string text)
        {
            if (text.StartsWith(Text_Play))
            {
                Play(text.Substring(Text_Play.Length));
                return;
            }
            if (text.StartsWith(Text_QuickPlay))
            {
                MediaHelper.QuickPlay();
                return;
            }
            if (text.Contains(Text_Pause))
            {
                MediaHelper.Pause();
                return;
            }
            if (text.Contains(Text_Prev) || text.Contains(Text_Prev2))
            {
                MediaHelper.MovePrev();
                return;
            }
            if (text.Contains(Text_Next) || text.Contains(Text_Next2))
            {
                MediaHelper.MoveNext();
                return;
            }
            if (text.Contains(Text_Volume) || text.Contains(Text_Sound))
            {
                ChangeVolume(text);
                return;
            }
            if (text.Contains(Text_Mute))
            {
                Helper.GetMainPageContainer().GetMediaControl().SetMuted(true);
                return;
            }
            if (text.Contains(Text_UnMute))
            {
                Helper.GetMainPageContainer().GetMediaControl().SetMuted(false);
                return;
            }
            if (text.Contains(Text_Search))
            {
                Search(text);
                return;
            }
            if (text.Contains(Text_Help))
            {
                await new Dialogs.VoiceAssistantHelpDialog().ShowAsync();
                return;
            }
        }

        private void Play(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                MediaHelper.Play();
                return;
            }
            switch (text)
            {
                case Text_Music:
                case Text_Song:
                    PlayMusic(text);
                    return;
                case Text_Artist:
                    PlayArtist(text);
                    return;
                case Text_Album:
                    PlayAlbum(text);
                    return;
                case Text_PlayList:
                    PlayPlaylist(text);
                    return;
                case Text_Folder:
                    PlayFolder(text);
                    return;
                default:
                    SearchAndPlay(text);
                    return;
            }

        }

        private void PlayMusic(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                MediaHelper.QuickPlay();
                return;
            }
            IEnumerable<Music> list = SearchHelper.SearchSongs(MusicLibraryPage.AllSongs, text, SortBy.Default);
            if (list.Count() == 0)
            {
                VoiceAssistantHelper.SpeakNoResults(text);
                return;
            }
            MediaHelper.SetMusicAndPlay(list.ElementAt(0));
        }

        private void PlayArtist(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                RandomPlayHelper.PlayArtist();
                return;
            }
            IEnumerable<Playlist> list = SearchHelper.SearchArtists(MusicLibraryPage.AllSongs, text, SortBy.Default);
            if (list.Count() == 0)
            {
                VoiceAssistantHelper.SpeakNoResults(text);
                return;
            }
            MediaHelper.ShuffleAndPlay(list.ElementAt(0).Songs);
        }

        private void PlayAlbum(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                RandomPlayHelper.PlayAlbum();
                return;
            }
            IEnumerable<AlbumView> list = SearchHelper.SearchAlbums(MusicLibraryPage.AllSongs, text, SortBy.Default);
            if (list.Count() == 0)
            {
                VoiceAssistantHelper.SpeakNoResults(text);
                return;
            }
            MediaHelper.ShuffleAndPlay(list.ElementAt(0).Songs);
        }

        private void PlayPlaylist(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                RandomPlayHelper.PlayPlaylist();
                return;
            }
            IEnumerable<AlbumView> list = SearchHelper.SearchPlaylists(Settings.settings.Playlists, text, SortBy.Default);
            if (list.Count() == 0)
            {
                VoiceAssistantHelper.SpeakNoResults(text);
                return;
            }
            MediaHelper.ShuffleAndPlay(list.ElementAt(0).Songs);
        }

        private void PlayFolder(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                RandomPlayHelper.PlayFolder();
                return;
            }
            IEnumerable<GridFolderView> list = SearchHelper.SearchFolders(Settings.settings.Tree, text, SortBy.Default);
            if (list.Count() == 0)
            {
                VoiceAssistantHelper.SpeakNoResults(text);
                return;
            }
            MediaHelper.ShuffleAndPlay(list.ElementAt(0).Songs);
        }

        private async void SearchAndPlay(string text)
        {
            var result = await SearchHelper.Search(text);
            if (result.Result == null)
            {
                VoiceAssistantHelper.SpeakNoResults(text);
                return;
            }
            switch (result.SearchType)
            {
                case SearchType.Artists:
                    Playlist artist = result.Result as Playlist;
                    MediaHelper.ShuffleAndPlay(artist.Songs);
                    if (IsBadSearch(text, artist.Name, result))
                    {
                        VoiceAssistantHelper.Speak("SearchResultArtist", artist.Name);
                    }
                    break;
                case SearchType.Albums:
                    AlbumView album = result.Result as AlbumView;
                    MediaHelper.ShuffleAndPlay(album.Songs);
                    if (IsBadSearch(text, album.Name, result))
                    {
                        VoiceAssistantHelper.Speak("SearchResultAlbum", album.Name);
                    }
                    break;
                case SearchType.Playlists:
                    AlbumView playlist = result.Result as AlbumView;
                    MediaHelper.ShuffleAndPlay(playlist.Songs);
                    if (IsBadSearch(text, playlist.Name, result))
                    {
                        VoiceAssistantHelper.Speak("SearchResultPlaylist", playlist.Name);
                    }
                    break;
                case SearchType.Folders:
                    GridFolderView folder = result.Result as GridFolderView;
                    MediaHelper.ShuffleAndPlay(folder.Songs);
                    if (IsBadSearch(text, folder.Name, result))
                    {
                        VoiceAssistantHelper.Speak("SearchResultFolder", folder.Name);
                    }
                    break;
                case SearchType.Songs:
                    Music music = result.Result as Music;
                    MediaHelper.SetMusicAndPlay(music);
                    if (IsBadSearch(text, music.Name, result))
                    {
                        VoiceAssistantHelper.Speak("SearchResultMusic", music.Name);
                    }
                    break;
            }
        }

        private bool IsBadSearch(string expected, string given, SearchResult result)
        {
            return expected != given && result.Score < 80;
        }

        public void ChangeVolume(string text)
        {
            string command = text.Substring(Text_Volume.Length);
            bool to = text.Contains(Text_VolumeTo);
            Regex regex = new Regex(@"\d+");
            MatchCollection matchCollections = regex.Matches(text);
            if (matchCollections.Count == 0)
            {
                VoiceAssistantHelper.SpeakNotUnderstand();
                return;
            }
            int value = int.Parse(matchCollections.ElementAt(0).Value);
            double newVolume;
            if (to)
            {
                newVolume = value;
            }
            else
            {
                int positive = command.StartsWith(Text_TurnUp) ? 1 : -1;
                double currentVolume = MediaHelper.Player.Volume * 100;
                double percent = text.Contains("%") ? MediaHelper.Player.Volume : 1;
                newVolume = currentVolume + positive * value * percent;
            }
            Helper.GetMainPageContainer().GetMediaControl().VolumeSlider.Value = newVolume;
            VoiceAssistantHelper.Speak("VoiceAssistantNewVolume", newVolume);
        }

        private void Search(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                VoiceAssistantHelper.SpeakNotUnderstand();
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
    }
}
