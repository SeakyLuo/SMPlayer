﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Models.DAO
{
    public class PreferenceItemDAO
    {
        // 偏好项目ID
        public int ItemId { get; set; }
        // 为了防止数据失效后无法通过实时查询展示项目名称，故此记录
        public string ItemName { get; set; }
        public bool IsEnabled { get; set; }
        public PreferLevel Level { get; set; } = PreferLevel.Normal;
    }
}
