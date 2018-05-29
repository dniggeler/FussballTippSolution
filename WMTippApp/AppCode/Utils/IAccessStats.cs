using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FussballTipp.Utils
{
    public interface IAccessStats
    {
        int GetRemoteHits();
        int GetCacheHits();
    }
}
