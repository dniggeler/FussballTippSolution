using OddsScraper.Contract;
using Tippspiel.Base;

namespace WebScraperOdds
{
    public class WettfreundeEuroConfigInfo : AppInfo<WettfreundeEuroConfigInfo>, IOddsScraperConfig
    {
        public string BaseUrl { get; set; }
        public string OddsLink { get; set; }

        public WettfreundeEuroConfigInfo()
        {
            BaseUrl = @"http://www.wettfreunde.net";
            OddsLink = @"/em-2016-spielplan";
        }
    }
}