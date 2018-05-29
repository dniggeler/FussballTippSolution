using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BhFS.Tippspiel.Utils
{
    public class WettfreundeConfigInfo : AppInfo<WettfreundeConfigInfo>
    {
        public string BaseURL { get; set; }
        public string OddsLink { get; set; }

        public WettfreundeConfigInfo()
        {
            BaseURL = @"http://www.wettfreunde.net";
            OddsLink = @"/bundesliga-spielplan/";
        }
    }
}