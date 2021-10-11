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

        public CommandResult Handle(string text)
        {
            CommandResult result = new CommandResult();
            if (text.Contains("快速播放"))
            {
                result.Type = MatchType.QuickPlay;
            }
            else if (text.Contains("恢复") || text.Contains("继续") || text == "播放")
            {
                result.Type = MatchType.Play;
            }
            else if (IsPlayMusic(text))
            {
                return HandlePlayMusic(text);
            }
            else if (text.Contains("音量") || text.Contains("声音"))
            {
                VolumeRequest request = HandleVolume(text);
                if (request == null)
                {
                    result.Type = MatchType.MatchNone;
                }
                else
                {
                    result.Type = MatchType.ChangeVolume;
                    result.Param = request;
                }
            }
            else if (text.Contains("前一首") || text.Contains("上一首"))
            {
                result.Type = MatchType.Previous;
            }
            else if (text.Contains("后一首") || text.Contains("下一首"))
            {
                result.Type = MatchType.Next;
            }
            else if (text.Contains("取消静音"))
            {
                result.Type = MatchType.UnMute;
            }
            else if (text.Contains("静音"))
            {
                result.Type = MatchType.Mute;
            }
            else if (text.StartsWith("暂停"))
            {
                result.Type = MatchType.Pause;
            }
            else if (text.Contains("帮助"))
            {
                result.Type = MatchType.Help;
            }
            else if (text.Contains("搜索"))
            {
                result.Type = MatchType.Search;
                MatchCollection searchMatch = new Regex(@"(?<=搜索).+").Matches(text);
                if (searchMatch.IsNotEmpty())
                {
                    result.Param = searchMatch.GetValue();
                }
            }
            else if (text.StartsWith("没事") || text.StartsWith("算了"))
            {
                result.Type = MatchType.Nothing;
            }
            else
            {
                result.Type = MatchType.MatchNone;
            }
            return result;
        }

        private bool IsPlayMusic(string text)
        {
            return text.Contains("播放") || new Regex("(来|放)(一)?(首|个|点|下)(歌)?").IsMatch(text);
        }

        private CommandResult HandlePlayMusic(string text)
        {
            CommandResult result = new CommandResult();
            MatchCollection playMusicMatch = new Regex(@"(?<=(播放(歌曲|音乐)|(来|放)(一)?(首|下)(歌)?)).*").Matches(text);
            if (playMusicMatch.IsNotEmpty())
            {
                result.Type = MatchType.PlayMusic;
                result.Param = playMusicMatch.GetValue();
                return result;
            }
            MatchCollection playArtistMatch = new Regex(@"(?<=播放歌手).*").Matches(text);
            if (playArtistMatch.IsNotEmpty())
            {
                if (HandlePlayItemByArtist(text, "播放歌手") is CommandResult playByArtistResult)
                {
                    return playByArtistResult;
                }
                result.Type = MatchType.PlayArtist;
                result.Param = playArtistMatch.GetValue();
                return result;
            }
            MatchCollection playAlbumMatch = new Regex(@"(?<=播放专辑).*").Matches(text);
            if (playAlbumMatch.IsNotEmpty())
            {
                if (HandlePlayMusicIn(text, @"(?<=播放专辑).+中的", MatchType.PlayMusicInAlbum) is CommandResult playMusicInAlbumMatch)
                {
                    return playMusicInAlbumMatch;
                }
                result.Type = MatchType.PlayAlbum;
                result.Param = playAlbumMatch.GetValue();
                return result;
            }
            MatchCollection playPlaylistMatch = new Regex(@"(?<=播放(列表|歌单)).*").Matches(text);
            if (playPlaylistMatch.IsNotEmpty())
            {
                if (HandlePlayMusicIn(text, @"(?<=播放(列表|歌单)).+中的", MatchType.PlayMusicInPlaylist) is CommandResult playMusicInPlaylistMatch)
                {
                    return playMusicInPlaylistMatch;
                }
                result.Type = MatchType.PlayPlaylist;
                result.Param = playPlaylistMatch.GetValue();
                return result;
            }
            MatchCollection playFolderMatch = new Regex(@"(?<=文件夹).*").Matches(text);
            if (playFolderMatch.IsNotEmpty())
            {
                if (HandlePlayMusicIn(text, @"(?<=播放文件夹).+中的", MatchType.PlayMusicInFolder) is CommandResult playMusicInFolderMatch)
                {
                    return playMusicInFolderMatch;
                }
                result.Type = MatchType.PlayFolder;
                result.Param = playFolderMatch.GetValue();
                return result;
            }
            string patternPrefix = @"(?<=(播放|(来|放)(一)(个|下|点))).+";
            if (HandlePlayItemByArtist(text, patternPrefix) is CommandResult playItemByArtistResult)
            {
                return playItemByArtistResult;
            }
            MatchCollection playArtistMatch2 = new Regex(patternPrefix + "的歌.*").Matches(text);
            if (playArtistMatch2.IsNotEmpty())
            {
                result.Type = MatchType.PlayByArtistOrMusic;
                result.Param = new ByArtistRequest(playArtistMatch2, "的歌")
                {
                    Item = ""
                };
                return result;
            }
            MatchCollection playMusicInMatch = new Regex(patternPrefix + @"中的.+").Matches(text);
            if (playMusicInMatch.IsNotEmpty())
            {
                return HandlePlayMusicIn(text, patternPrefix);
            }
            MatchCollection playByArtistMatch = new Regex(patternPrefix + @"(的).+").Matches(text);
            if (playByArtistMatch.IsNotEmpty())
            {
                result.Type = MatchType.PlayByArtist;
                result.Param = new ByArtistRequest(playByArtistMatch, "的");
                return result;
            }
            MatchCollection playMatch = new Regex(patternPrefix).Matches(text);
            if (playMatch.Count() == 0)
            {
                result.Type = MatchType.MatchNone;
            }
            else
            {
                result.Type = MatchType.SearchAndPlay;
                result.Param = playMatch.GetValue();
            }
            return result;
        }

        private CommandResult HandlePlayItemByArtist(string text, string patternPrefix)
        {
            if (HandlePlayItemByArtist(text, patternPrefix, "的专辑", MatchType.PlayByArtistAndAlbum) is CommandResult album)
            {
                return album;
            }
            if (HandlePlayItemByArtist(text, patternPrefix, "的歌曲", MatchType.PlayByArtistAndMusic) is CommandResult music)
            {
                return music;
            }
            if (HandlePlayItemByArtist(text, patternPrefix, "的歌", MatchType.PlayByArtistAndMusic) is CommandResult music2)
            {
                return music2;
            }
            return null;
        }

        private CommandResult HandlePlayMusicIn(string text, string patternPrefix, MatchType matchType = MatchType.PlayMusicIn)
        {
            if (HandlePlayItemByArtist(text, patternPrefix, "歌曲", matchType) is CommandResult music0)
            {
                return music0;
            }
            if (HandlePlayItemByArtist(text, patternPrefix, "歌", matchType) is CommandResult music1)
            {
                return music1;
            }
            if (HandlePlayItemByArtist(text, patternPrefix, "", matchType) is CommandResult music2)
            {
                return music2;
            }
            return null;
        }

        private CommandResult HandlePlayItemByArtist(string text, string patternPrefix, string tag, MatchType type)
        {
            MatchCollection match = new Regex(patternPrefix + tag + ".+").Matches(text);
            if (match.IsNotEmpty())
            {
                return new CommandResult
                {
                    Type = type,
                    Param = new ByArtistRequest(match, tag)
                };
            }
            return null;
        }

        private VolumeRequest HandleVolume(string text)
        {
            bool To, Percentage;
            double value;
            MatchCollection numMatch = new Regex(@"\d+").Matches(text);
            MatchCollection fractionMatch = new Regex(@"\d+/\d+").Matches(text);
            bool half = text.Contains("一半");
            if (numMatch.Count == 0 && !half)
            {
                To = false;
                value = 10;
                Percentage = false;
            }
            else
            {
                To = text.Contains("至") || text.Contains("到") || text.Contains("成");
                if (fractionMatch.IsNotEmpty())
                {
                    value = VoiceAssistantHelper.FractionToDouble(fractionMatch.GetValue());
                    Percentage = true;
                }
                else
                {
                    if (half)
                    {
                        Percentage = true;
                        value = 50;
                    }
                    else
                    {
                        Percentage = text.Contains("%");
                        value = int.Parse(numMatch.GetValue());
                    }
                }
            }
            return new VolumeRequest
            {
                TurnUp = !text.Contains("低"),
                To = To,
                Value = value,
                Percentage = Percentage
            };
        }
    }
}
