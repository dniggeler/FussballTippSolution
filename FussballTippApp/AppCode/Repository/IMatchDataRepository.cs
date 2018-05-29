using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FussballTipp.Repository
{
    public class GroupInfoModel
    {
        public int Id { get; private set; }
        public string Text { get; private set; }

        public GroupInfoModel(int id, string text)
        {
            Id = id;
            Text = text;
        }
    }

    public class MatchDataModel
    {
        public int MatchId { get; set; }
        public int GroupId { get; set; }
        public int MatchNr { get; set; }
        public string LeagueShortcut { get; set; }
        public DateTime KickoffTime { get; set; }
        public DateTime KickoffTimeUTC { get; set; }
        public bool IsFinished { get; set; }
        public int HomeTeamId { get; set; }
        public int AwayTeamId { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public string HomeTeamIcon { get; set; }
        public string AwayTeamIcon { get; set; }
        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }
        public bool HasVerlaengerung { get; set; }

        public int? ResultType
        {
            get
            {
                if (this.HasStarted == true)
                {
                    return (this.HomeTeamScore > this.AwayTeamScore) ?
                                                    1 : (this.HomeTeamScore < this.AwayTeamScore) ?
                                                    2 : 0;
                }
                else
                {
                    return null;
                }
            }
        }

        public bool HasStarted
        {
            get
            {
                return !(KickoffTime > DateTime.Now);
            }
        }
    }

    public interface IFussballDataRepository
    {
        bool IsSpieltagComplete { get; }

        bool Exist(string leagueShortcut, string leagueSeason);

        GroupInfoModel GetCurrentGroup();
        List<GroupInfoModel> GetAllGroups();

        MatchDataModel GetNextMatch();
        MatchDataModel GetLastMatch();

        MatchDataModel GetMatchData(int matchId);

        List<MatchDataModel> GetMatchesByCurrentGroup();
        List<MatchDataModel> GetMatchesByGroup(int groupId);
        List<MatchDataModel> GetAllMatches();
    }
}