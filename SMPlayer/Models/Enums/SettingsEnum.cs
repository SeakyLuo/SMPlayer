using System;

namespace SMPlayer.Models
{
    public static class SettingsEnum
    {
        public static NotificationDisplayMode[] NotificationDisplayModes = { NotificationDisplayMode.Reminder, NotificationDisplayMode.Normal, NotificationDisplayMode.Quick };
        public static NotificationSendMode[] NotificationSendModes = { NotificationSendMode.MusicChanged, NotificationSendMode.Never };
    }

    public enum PlayMode
    {
        Once = 0,
        Repeat = 1,
        RepeatOne = 2,
        Shuffle = 3
    }

    public enum LocalView
    {
        ListView = 0,
        GridView = 1
    }

    public enum NotificationSendMode
    {
        MusicChanged = 0,
        Never = 1
    }

    public enum NotificationDisplayMode
    {
        Reminder = 0,
        Normal = 1,
        Quick = 2
    }

    public enum VoiceAssistantLanguage
    {
        English = 0,
        Chinese = 1
    }

    public enum SortBy
    {
        Default = -1,
        Title = 0,
        Artist = 1,
        Album = 2,
        Duration = 3,
        PlayCount = 4,
        DateAdded = 5,
        Name = 6,
        Reverse = 7
    }

    public enum LocalPageViewMode
    {
        Grid, List
    }

    public enum LyricsSource
    {
        Internet = 0, LrcFile = 1, Music = 2
    }

    public static class SortByConverter
    {
        public static string ToStr(this SortBy criterion)
        {
            switch (criterion)
            {
                case SortBy.Default:
                    return "Default";
                case SortBy.Title:
                    return "Title";
                case SortBy.Artist:
                    return "Artist";
                case SortBy.Album:
                    return "Album";
                case SortBy.Duration:
                    return "Duration";
                case SortBy.PlayCount:
                    return "Play Count";
                case SortBy.DateAdded:
                    return "Date Added";
                case SortBy.Name:
                    return "Name";
                default:
                    return "";
            }
        }

        public static SortBy FromStr(string criterion)
        {
            switch (criterion)
            {
                case "Default":
                    return SortBy.Default;
                case "Title":
                case "Name":
                    return SortBy.Title;
                case "Artist":
                    return SortBy.Artist;
                case "Album":
                    return SortBy.Album;
                case "Duration":
                    return SortBy.Duration;
                case "PlayCount":
                case "Play Count":
                    return SortBy.PlayCount;
                case "DateAdded":
                case "Date Added":
                    return SortBy.DateAdded;
                default:
                    return SortBy.Default;
            }
        }
    }
}
