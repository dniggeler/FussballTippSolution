using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BhFS.Tippspiel.Utils;
using WMTippApp.Models;

namespace BhFS.Tippspiel.Utils
{
    public class OpenDBHelper
    {
        public struct SpieltagInfo
        {
            public int CurrentSpieltag { get; set; }
            public int TippSpieltag { get; set; }
            public bool IsCompleted { get; set; }
        }

        public struct CompleteInfo
        {
            public bool IsCompleted { get; set; }
            // recently means round not completed yesterday
            public bool IsCompletedRecently { get; set; }
            public DateTime? CompletedSince { get; set; }
        }

        public static CompleteInfo IsSpieltagComplete(WMTippApp.SvcFussballDB.SportsdataSoapClient client)
        {
            var dataNext = client.GetNextMatch(SportsdataConfigInfo.Current.LeagueShortcut);
            var dataLast = client.GetLastMatch(SportsdataConfigInfo.Current.LeagueShortcut);

            if (dataNext == null)
            {

                return new CompleteInfo()
                {
                    IsCompleted = true,
                    CompletedSince = dataLast.matchDateTime.AddHours(3)
                };
            }
            else if (dataLast == null)
            {
                return new CompleteInfo()
                {
                    IsCompleted = false,
                    CompletedSince = null,
                };
            }
            else
            {
                var result = new CompleteInfo();

                if (dataLast.groupOrderID < dataNext.groupOrderID)
                {
                    result.IsCompleted = true;
                    // check if emails already sent the day before
                    {
                        var lastMatchDate = dataLast.matchDateTime;
                        var yesterday = DateTime.Now.AddDays(-1);
                        if (lastMatchDate > yesterday)
                        {
                            result.IsCompletedRecently = true;
                        }
                    }
                    result.CompletedSince = dataLast.matchDateTime.AddHours(3);
                }

                return result;
            }
        }

        public static SpieltagInfo GetSpieltagInfo(WMTippApp.SvcFussballDB.SportsdataSoapClient client)
        {
            var spieltagInfo = new SpieltagInfo();
            spieltagInfo.CurrentSpieltag = spieltagInfo.TippSpieltag = 1;

            var dataNext = client.GetNextMatch(SportsdataConfigInfo.Current.LeagueShortcut);
            var dataLast = client.GetLastMatch(SportsdataConfigInfo.Current.LeagueShortcut);

            if (dataNext == null && dataLast == null)
            {
                return spieltagInfo;
            }
            else if (dataLast == null)
            {
                spieltagInfo.CurrentSpieltag = spieltagInfo.TippSpieltag = dataNext.groupOrderID;

                return spieltagInfo;
            }
            else if (dataNext == null)
            {
                spieltagInfo.CurrentSpieltag = spieltagInfo.TippSpieltag = dataLast.groupOrderID;

                return spieltagInfo;
            }
            else
            {
                if(dataNext.groupOrderID > dataLast.groupOrderID)
                {
                    spieltagInfo.CurrentSpieltag = dataLast.groupOrderID;
                    spieltagInfo.TippSpieltag = dataNext.groupOrderID;
                }
                else{
                    spieltagInfo.CurrentSpieltag = spieltagInfo.TippSpieltag = dataLast.groupOrderID;
                }

                return  spieltagInfo;
            }
        }

        public static MatchInfoModel Create(WMTippApp.SvcFussballDB.Matchdata match)
        {
            var matchModelObj = new MatchInfoModel();
            matchModelObj.MatchId = match.matchID;
            matchModelObj.KickoffTime = match.matchDateTime;
            matchModelObj.HomeTeamScore = match.pointsTeam1;
            matchModelObj.AwayTeamScore = match.pointsTeam2;
            matchModelObj.HomeTeamIcon = match.iconUrlTeam1;
            matchModelObj.AwayTeamIcon = match.iconUrlTeam2;
            matchModelObj.HomeTeam = match.nameTeam1;
            matchModelObj.AwayTeam = match.nameTeam2;
            matchModelObj.IsFinished = match.matchIsFinished;

            return matchModelObj;
        }
    }
}