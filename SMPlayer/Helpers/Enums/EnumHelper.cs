using SMPlayer.Helpers.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Helpers
{
    public static class EnumHelper
    {
        /// <summary>
        /// 获取枚举类型的描述信息
        /// </summary>
        public static string GetDescription(this Enum value)
        {
            return GetEnumDescription(value)?.Description;
        }

        public static string GetToolTip(this Enum value)
        {
            return GetEnumDescription(value)?.ToolTip;
        }

        private static EnumDescription GetEnumDescription(Enum value)
        {
            return GetField(value) is FieldInfo field ? 
                   (EnumDescription)Attribute.GetCustomAttribute(field, typeof(EnumDescription))
                   : null;
        }

        private static FieldInfo GetField(Enum value)
        {
            var type = value.GetType();
            var name = Enum.GetName(type, value);
            if (name == null) return null;
            return name == null ? null : type.GetField(name);
        }

        public static int GetOrder(this Enum value)
        {
            return GetEnumOrder(value)?.Order ?? Convert.ToInt32(value);
        }

        public static EnumOrder GetEnumOrder(Enum value)
        {
            return GetField(value) is FieldInfo field ?
                   (EnumOrder)Attribute.GetCustomAttribute(field, typeof(EnumOrder))
                   : null;
        }

        public static List<Enum> GetOrderedValues(Type enumType)
        {
            return Enum.GetValues(enumType).OfType<Enum>().OrderBy(i => i.GetOrder()).ToList();
        }
    }
}
