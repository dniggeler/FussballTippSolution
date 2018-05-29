using System.Collections.Generic;
using OddsScraper.Contract.Model;

namespace OddsScraper.Contract
{
    public interface IOddsScraper
    {
        List<OddsInfoModel> GetOdds(string oddsAsHtml, string roundTag);
        List<OddsInfoModel> LoadOdds(string url, string roundTag);
    }
}
