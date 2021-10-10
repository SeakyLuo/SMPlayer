using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SMPlayer.Helpers.VoiceAssistant
{
    class VoiceAssistantEnglishHelper : IVoiceAssistantCommandHandler
    {
        private const StringComparison Comparison = StringComparison.CurrentCultureIgnoreCase;

        public CommandResult Handle(string text)
        {
            CommandResult result = new CommandResult();
            if (Regex.IsMatch(text, @"^(?!play).*quick play", VoiceAssistantHelper.Option) || 
                Regex.IsMatch(text, @"^(?!play).*(give|get).*(me)? some music", VoiceAssistantHelper.Option))
            {
                // 前面不能有play
                result.Type = MatchType.QuickPlay;
            }
            else if (text.Equals("play", Comparison)) // 优先匹配
            {
                result.Type = MatchType.Play;
            }
            else if (IsPlayMusic(text))
            {
                return HandlePlayMusic(text);
            }
            else if (text.Contains("resume", Comparison) || text.Contains("continue", Comparison))
            {
                result.Type = MatchType.Play;
            }
            else if (text.Contains("volume", Comparison) || text.Contains("sound", Comparison) || text.Contains("turn up", Comparison) || text.Contains("turn down", Comparison))
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
            else if (text.Contains("previous", Comparison))
            {
                result.Type = MatchType.Previous;
            }
            else if (text.Contains("next", Comparison))
            {
                result.Type = MatchType.Next;
            }
            else if (text.Contains("unmute", Comparison))
            {
                result.Type = MatchType.UnMute;
            }
            else if (text.Contains("mute", Comparison))
            {
                result.Type = MatchType.Mute;
            }
            else if (text.Contains("pause", Comparison))
            {
                result.Type = MatchType.Pause;
            }
            else if (text.Contains("help", Comparison))
            {
                result.Type = MatchType.Help;
            }
            else if (text.Contains("search", Comparison))
            {
                result.Type = MatchType.Search;
                MatchCollection searchMatch = new Regex(@"(?<=search).+").Matches(text);
                if (searchMatch.IsNotEmpty())
                {
                    result.Param = searchMatch.GetValue();
                }
            }
            else
            {
                result.Type = MatchType.MatchNone;
            }
            return result;
        }


        private bool IsPlayMusic(string text)
        {
            return text.Contains("play", Comparison);
        }

        private CommandResult HandlePlayMusic(string text)
        {
            CommandResult result = new CommandResult();
            MatchCollection playMusicMatch = VoiceAssistantHelper.Matches(text, @"(?<=play .*music).*");
            if (playMusicMatch.IsNotEmpty())
            {
                MatchCollection playMusicByArtistMatch = VoiceAssistantHelper.Matches(text, @"(?<=play .*music).+ by .+");
                if (playMusicByArtistMatch.IsNotEmpty())
                {
                    result.Type = MatchType.PlayByArtistAndMusic;
                    result.Param = new ByArtistRequest(playMusicByArtistMatch, " by ");
                    return result;
                }
                if (HandlePlayMusicIn(text, "play .*music") is CommandResult playMusicInResult)
                {
                    return playMusicInResult;
                }
                result.Type = MatchType.PlayMusic;
                result.Param = playMusicMatch.GetValue();
                return result;
            }
            MatchCollection playArtistMatch = VoiceAssistantHelper.Matches(text, @"(?<=play .*(artist|musician|singer)).*");
            if (playArtistMatch.IsNotEmpty())
            {
                result.Type = MatchType.PlayArtist;
                result.Param = playArtistMatch.GetValue();
                return result;
            }
            MatchCollection playAlbumMatch = VoiceAssistantHelper.Matches(text, @"(?<=play .*album).*");
            if (playAlbumMatch.IsNotEmpty())
            {
                MatchCollection playAlbumByArtistMatch = VoiceAssistantHelper.Matches(text, @"(?<=play .*album) .+ by .+");
                if (playAlbumByArtistMatch.IsNotEmpty())
                {
                    result.Type = MatchType.PlayByArtistAndMusic;
                    result.Param = new ByArtistRequest(playAlbumByArtistMatch, " by ");
                    return result;
                }
                result.Type = MatchType.PlayAlbum;
                result.Param = playAlbumMatch.GetValue();
                return result;
            }
            MatchCollection playPlaylistMatch = VoiceAssistantHelper.Matches(text, @"(?<=play .*playlist).*");
            if (playPlaylistMatch.IsNotEmpty())
            {
                result.Type = MatchType.PlayPlaylist;
                result.Param = playPlaylistMatch.GetValue();
                return result;
            }
            MatchCollection playFolderMatch = VoiceAssistantHelper.Matches(text, @"(?<=play .*folder).*");
            if (playFolderMatch.IsNotEmpty())
            {
                result.Type = MatchType.PlayFolder;
                result.Param = playFolderMatch.GetValue();
                return result;
            }
            MatchCollection playByArtistMatch = VoiceAssistantHelper.Matches(text, @"(?<=play) .+ by .+");
            if (playByArtistMatch.IsNotEmpty())
            {
                result.Type = MatchType.PlayByArtist;
                result.Param = new ByArtistRequest(playByArtistMatch, " by ");
                return result;
            }
            if (HandlePlayMusicIn(text, "play .+") is CommandResult playInResult)
            {
                return playInResult;
            }
            MatchCollection playByArtistMatch2 = VoiceAssistantHelper.Matches(text, @"(?<=play) .+'s .+");
            if (playByArtistMatch2.IsNotEmpty())
            {
                result.Type = MatchType.PlayByArtist;
                result.Param = new ByArtistRequest(playByArtistMatch, "'s ");
                return result;
            }
            MatchCollection playMatch = VoiceAssistantHelper.Matches(text, @"(?<=play).*");
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
        private CommandResult HandlePlayMusicIn(string text, string patternPrefix)
        {
            string prepositions = "(in|from|of)";
            MatchCollection playMusicInMatch = VoiceAssistantHelper.Matches(text, $@"(?<={patternPrefix}).+ {prepositions} .+");
            if (playMusicInMatch.IsEmpty())
            {
                return null;
            }
            ByArtistRequest request = new ByArtistRequest
            {
                Original = playMusicInMatch.GetValue(),
                Item = VoiceAssistantHelper.Matches(text, $@"(?<={patternPrefix}).+ (?={prepositions})").GetValue()
            };
            MatchType matchType;
            MatchCollection playMusicInSthMatch = VoiceAssistantHelper.Matches(text, patternPrefix + $@"{patternPrefix}.+ {prepositions} .+(?<=(album|playlist|folder)) .+");
            string afterPrep = VoiceAssistantHelper.Matches(text, patternPrefix + $@"{patternPrefix}.+ (?<={prepositions}) .+").GetValue();
            if (playMusicInSthMatch.IsEmpty())
            {
                matchType = MatchType.PlayMusicIn;
                request.Artist = afterPrep;
            }
            else
            {
                if (afterPrep.Contains("album", Comparison))
                {
                    matchType = MatchType.PlayMusicInAlbum;
                }
                else if (afterPrep.Contains("playlist", Comparison))
                {
                    matchType = MatchType.PlayMusicInPlaylist;
                }
                else if (afterPrep.Contains("folder", Comparison))
                {
                    matchType = MatchType.PlayMusicInFolder;
                }
                else
                {
                    matchType = MatchType.PlayMusicIn;
                }
                request.Artist = playMusicInSthMatch.GetValue();
            }
            return new CommandResult
            {
                Param = request,
                Type = matchType
            };
        }


        private VolumeRequest HandleVolume(string text)
        {
            bool To, Percentage;
            double value;
            MatchCollection numMatch = new Regex(@"\d+").Matches(text);
            MatchCollection fractionMatch = new Regex(@"\d+/\d+").Matches(text);
            bool half = text.Contains("half", Comparison);
            bool quarter = text.Contains("quarter", Comparison);
            if (numMatch.Count == 0 && !half && !quarter)
            {
                To = false;
                value = 10;
                Percentage = false;
            }
            else
            {
                To = text.Contains("to");
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
                    else if (quarter)
                    {
                        Percentage = true;
                        value = 25;
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
                TurnUp = !text.Contains("lower", Comparison) && !text.Contains("down", Comparison),
                To = To,
                Value = value,
                Percentage = Percentage
            };
        }

    }
}
