using SMPlayer.Helpers;
using SMPlayer.Models;
using SMPlayer.Models.DAO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Services
{
    public class PreferenceService
    {
        public static void RemovePreferences(EntityType type, IEnumerable<string> itemIds)
        {
            SQLHelper.Run(c => c.Execute("update PreferenceItem set State = ? where Type = ? and ItemId in (?)", ActiveState.Inactive, type, itemIds.JoinIntoString()));
        }
    }
}
