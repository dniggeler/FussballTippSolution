using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FussballTippApp.Models
{
    public class DailyWinnerInfoModel
    {
        public List<RankingInfoModel> Ranking { get; set; }
        public Dictionary<string, List<MatchInfoModel>> AllTippInfoDict { get; set; }
        public List<MatchInfoModel> MatchInfo { get; set; }

        public DailyWinnerInfoModel()
        {
            Ranking = new List<RankingInfoModel>();
            AllTippInfoDict = new Dictionary<string, List<MatchInfoModel>>();
            MatchInfo = new List<MatchInfoModel>();
        }
    }

    public class OverallDailyWinnerInfoModel
    {
        public Dictionary<string, OverallDailyWinnerRankingItemModel> AllDailyWinnerMap { get; set; }

        public OverallDailyWinnerInfoModel()
        {
            AllDailyWinnerMap = new Dictionary<string, OverallDailyWinnerRankingItemModel>();
        }
    }

    public class OverallDailyWinnerRankingItemModel
    {
        public string User { get; set; }
        public string DisplayName { get; set; }
        public int Rang { get; set; }
        public int Wins { get; set; }        
    }

    public class RankingInfoModel
    {
        public string User { get; set; }
        public string DisplayName { get; set; }
        public int TippCount { get; set; }
        public int TippCountFavorite { get; set; }
        public int TippCountLongshot { get; set; }
        public double TotalPoints { get; set; }
        public int Rang { get; set; }
        public int RangDelta { get; set; }

        public double FavoriteRatio
        {
            get
            {
                return (TippCount > 0) ? TippCountFavorite / TippCount : 0.0;
            }
        }

        public double LongshotRatio
        {
            get
            {
                return (TippCount > 0) ? TippCountLongshot / TippCount : 0.0;
            }
        }

        public double PointAvg
        {
            get
            {
                return (TippCount > 0) ? TotalPoints / TippCount : 0.0;
            }
        }
    }

    public class MatchInfoModel
    {
        public int MatchId { get; set; }
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

        public int? FavoriteTippIndex { get; set; }
        public int? LongshotTippIndex { get; set; }

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

        public double? MyPoints
        {
            get
            {
                if (this.HasStarted == true &&
                    this.MyTip.HasValue &&
                    this.ResultType.HasValue)
                {
                    return (this.ResultType.Value == this.MyTip.Value) ? this.MyOdds * this.MyAmount : 0.0;
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
        public int Spieltag { get; set; }
        public bool IsSpieltagFinished { get; set; }
        public bool IsTippSpielFinished { get; set; }
        public List<MatchInfoModel> Matchdata { get; set; }

        public SpieltagModel()
        {
            Spieltag = 1;
            IsSpieltagFinished = true;
            IsSpieltagFinished = false;
            Matchdata = new List<MatchInfoModel>();
        }
    }
}