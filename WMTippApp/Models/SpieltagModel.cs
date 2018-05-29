using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FussballTipp.Repository;

namespace WMTippApp.Models
{
    public class DailyWinnerInfoModel
    {
        public List<RankingInfoModel> Ranking { get; set; }
        public Dictionary<string, List<MatchInfoModel>> AllTippInfoDict { get; set; }
        public List<MatchDataModel> MatchInfo { get; set; }

        public DailyWinnerInfoModel()
        {
            Ranking = new List<RankingInfoModel>();
            AllTippInfoDict = new Dictionary<string, List<MatchInfoModel>>();
            MatchInfo = new List<MatchDataModel>();
        }
    }

    public class RankingInfoModel
    {
        public string User { get; set; }
        public int JokerUsed { get; set; }
        public string DisplayName { get; set; }
        public int TippCount { get; set; }
        public double TotalPoints { get; set; }
        public int Rang { get; set; }
        public int RangDelta { get; set; }

        public double TotalPointsClean { get; set; }

        public double PointAvg
        {
            get
            {
                return (TippCount > 0) ? TotalPoints / TippCount : 0.0;
            }
        }

        public double PointAvgClean
        {
            get
            {
                return (TippCount > 0) ? TotalPointsClean / TippCount : 0.0;
            }
        }
    }

    public class MatchInfoModel
    {
        public int MatchId { get; set; }
        public int GroupId { get; set; }
        public int MatchNr { get; set; }
        public DateTime KickoffTime { get; set; }
        public bool IsFinished { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public string HomeTeamIcon { get; set; }
        public string AwayTeamIcon { get; set; }
        public int HomeTeamScore { get; set; }
        public int AwayTeamScore { get; set; }

        public double? HomeTeamOdds { get; set; }
        public double? AwayTeamOdds { get; set; }
        public double? DrawOdds { get; set; }

        public bool IsJoker { get; set; }

        public int? MyTip { get; set; }
        public double? MyOdds { get; set; }
        public double? MyAmount { get; set; }

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

        public bool? IsMyTipCorrect
        {
            get
            {
                if (this.HasStarted == false || this.ResultType.HasValue== false) return null;

                if (this.MyTip.HasValue == true)
                {
                    return (this.ResultType.Value == this.MyTip.Value) ? true : false;
                }

                return false;
            }
        }

        public double? MyPointsClean
        {
            get
            {
                if (this.HasStarted == true &&
                    this.MyTip.HasValue &&
                    this.ResultType.HasValue)
                {
                    return (this.ResultType == this.MyTip) ? this.MyOdds : 0.0;
                }
                else
                {
                    return null;
                }
            }
        }

        public double? MyPoints
        {
            get
            {
                if (this.HasStarted == true &&
                    this.MyTip.HasValue &&
                    this.ResultType.HasValue)
                {
                    return (this.ResultType == this.MyTip) ? this.MyOdds * this.MyAmount : 0.0;
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

        public MatchInfoModel()
        {
        }
    }

    public class SpieltagModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public bool IsSpieltagFinished { get; set; }
        public int MaxJokers { get; set; }
        public int NumJokersUsed { get; set; }
        public double JokerMultiplicator { get; set; }
        public List<MatchInfoModel> Matchdata { get; set; }

        public SpieltagModel()
        {
            GroupId = 1;
            IsSpieltagFinished = true;
            Matchdata = new List<MatchInfoModel>();
            JokerMultiplicator = 1.0;
        }
    }
}