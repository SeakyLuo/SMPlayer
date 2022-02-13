using SMPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMPlayer.Interfaces
{
    public interface ILocalStorageItem
    {
        StorageItem AsStorageItem();
    }
}
