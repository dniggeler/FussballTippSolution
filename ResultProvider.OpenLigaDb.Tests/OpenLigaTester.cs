using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ResultProvider.OpenLigaDb.Tests.Properties;
using Xunit;
using Xunit.Abstractions;

namespace ResultProvider.OpenLigaDb.Tests
{
    public class Team1
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string TeamIconUrl { get; set; }
    }

    public class Team2
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string TeamIconUrl { get; set; }
    }

    public class RootObject
    {
        public int MatchID { get; set; }
        public string MatchDateTime { get; set; }
        public string TimeZoneID { get; set; }
        public string MatchDateTimeUTC { get; set; }
        public Team1 Team1 { get; set; }
        public Team2 Team2 { get; set; }
        public string LastUpdateDateTime { get; set; }
        public bool MatchIsFinished { get; set; }
        public List<object> MatchResults { get; set; }
        public List<object> Goals { get; set; }
        public object Location { get; set; }
        public object NumberOfViewers { get; set; }
    }
    public class OpenLigaTester
    {
        private readonly ITestOutputHelper _helper;

        public OpenLigaTester(ITestOutputHelper helper)
        {
            _helper = helper;
        }

        [Fact(DisplayName = "All Matches")]
        public void CompareAllMatches()
        {
            var realEuro16Matches = JsonConvert.DeserializeObject<RootObject[]>(Resources.realem2016);
            var validEuro16Matches = JsonConvert.DeserializeObject<RootObject[]>(Resources.em2016);

            for (int ii = 0; ii < realEuro16Matches.Length; ii++)
            {
                var realItem = realEuro16Matches[ii];

                var validMatch =
                    validEuro16Matches.FirstOrDefault(
                        v => realItem.Team1.TeamName.Contains(v.Team1.TeamName) && realItem.Team2.TeamName.Contains(v.Team2.TeamName));

                if (validMatch == null)
                {
                    _helper.WriteLine("Error with {0}/{1}/{2}",realItem.MatchID, realItem.Team1.TeamName, realItem.Team2.TeamName);
                }
                else
                {
                    _helper.WriteLine("{0}/{1}/{2} - {3}/{4}/{5}",
                          realItem.MatchID, realItem.Team1.TeamName, realItem.Team2.TeamName,
                          validMatch.MatchID, validMatch.Team1.TeamName, validMatch.Team2.TeamName);
                }
            }
        }

        [Fact(DisplayName = "Update Stmt")]
        public void ProduceUpdateStmt()
        {
            var realEuro16Matches = JsonConvert.DeserializeObject<RootObject[]>(Resources.realem2016);
            var validEuro16Matches = JsonConvert.DeserializeObject<RootObject[]>(Resources.em2016);

            for (int ii = 0; ii < realEuro16Matches.Length; ii++)
            {
                var realItem = realEuro16Matches[ii];

                var validMatch =
                    validEuro16Matches.FirstOrDefault(
                        v => realItem.Team1.TeamName.Contains(v.Team1.TeamName) && realItem.Team2.TeamName.Contains(v.Team2.TeamName));

                if (validMatch == null)
                {
                    _helper.WriteLine("Error with {0}/{1}/{2}", realItem.MatchID, realItem.Team1.TeamName, realItem.Team2.TeamName);
                }
                else
                {
                    _helper.WriteLine("UPDATE [wmdatadb].[dbo].[TippMatch] SET [MatchId] = {0} WHERE MatchId= {1} --and [user]='dieter'", validMatch.MatchID, realItem.MatchID);
                }
            }
        }
    }
}
