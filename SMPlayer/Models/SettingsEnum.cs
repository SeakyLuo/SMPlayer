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
        Chinese = 1,
        English = 2
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
        MusicChange = 1,
        Never = 2
    }
}
