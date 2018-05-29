using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BhFS.Tippspiel.Utils
{
    public class SportsdataConfigInfo : AppInfo<SportsdataConfigInfo>
    {
        // Addition shared info
        public string LeagueShortcut { get; set; }
        public string LeagueSaison { get; set; }
        public int StartSpieltag { get; set; }
        public int EndSpieltag { get; set; }

        public SportsdataConfigInfo()
        {
            LeagueShortcut = "bl1";
            LeagueSaison = "2015";
            StartSpieltag = 18;
            EndSpieltag = 34;
        }
    }

    public class AppInfo<Subclass>
        where Subclass : AppInfo<Subclass>, new()
    {
        private static string Key
        {
            get { return typeof(AppInfo<Subclass>).FullName; }
        }

        private static Subclass Value
        {
            get { return (Subclass)HttpContext.Current.Application[Key]; }
            set { HttpContext.Current.Application[Key] = value; }
        }

        public static Subclass Current
        {
            get
            {
                var instance = Value;
                if (instance == null)
                    lock (typeof(Subclass)) // not ideal to lock on a type -- but it'll work
                    {
                        // standard lock double-check
                        instance = Value;
                        if (instance == null)
                            Value = instance = new Subclass();
                    }
                return instance;
            }
        }
    }
}