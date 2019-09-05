﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models
{
    public enum AppLanguage
    {
        FollowSystem = 0,
        SimplifiedChinese = 1,
        TraditionalChinese = 2,
        English = 3,
        Japanese = 4
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

    public enum ShowNotification
    {
        Always = 0,
        MusicChanged = 1,
        Never = 2
    }
}
