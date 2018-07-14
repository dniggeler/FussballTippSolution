using Tippspiel.Base;

namespace BhFS.Tippspiel.Utils
{
    public class SportsdataConfigInfo : AppInfo<SportsdataConfigInfo>
    {
        // Addition shared info
        public string LeagueShortcut { get; set; }
        public string LeagueSaison { get; set; }
        public int StartSpieltag { get; set; }
        public int CurrentSpieltag { get; set; }
        public int EndSpieltag { get; set; }

        public SportsdataConfigInfo()
        {
            LeagueShortcut = "wmrussland";
            LeagueSaison = "2018";
            StartSpieltag = 1;
            CurrentSpieltag = 7;
            EndSpieltag = 8;
        }
    }

}