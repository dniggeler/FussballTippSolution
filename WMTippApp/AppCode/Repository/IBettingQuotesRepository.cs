using System.Collections.Generic;
using OddsScraper.Contract.Model;

namespace FussballTipp.Repository
{
    public interface IBettingQuotesRepository
    {
        List<OddsInfoModel> GetOdds();
    }
}