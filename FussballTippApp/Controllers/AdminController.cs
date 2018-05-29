using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using FussballTippApp.Models;
using System.Web.Security;
using FussballTippApp.Filters;
using BhFS.Tippspiel.Utils;
using Data.SqlClient;
using FussballTipp.Repository;
using FussballTipp.Utils;
using OddsScraper.Contract.Model;

namespace FussballTippApp.Controllers
{
    [InitializeSimpleMembership]
    public class AdminController : Controller
    {
        readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IFussballDataRepository _matchDataRepository = new BuLiDataRepository(SportsdataConfigInfo.Current);

        public ActionResult Index(int? Spieltag)
        {
            using (var client = new SvcFussballDB.SportsdataSoapClient())
            {
                int currSpieltag = (Spieltag.HasValue == true) ? 
                    Spieltag.Value :
                    OpenDBHelper.GetSpieltagInfo(_matchDataRepository).TippSpieltag;

                // build dropdown list data
                {
                    var count = SportsdataConfigInfo.Current.EndSpieltag - SportsdataConfigInfo.Current.StartSpieltag + 1;
                    var ddlSpieltageRange = (from e in Enumerable.Range(SportsdataConfigInfo.Current.StartSpieltag, count)
                                             select new SelectListItem()
                                             {
                                                 Value = e.ToString(),
                                                 Text = "Spieltag " + e.ToString(),
                                                 Selected = (e == currSpieltag)
                                             });

                    ViewBag.Spieltag = ddlSpieltageRange;
                }

                var viewModel = DailyWinnersInternal(currSpieltag,true);

                return View(viewModel);
            }
        }

        public ActionResult Clear()
        {
            // delete all tipper data
            using (var ctxt = new TippSpielContext())
            {
                foreach (var tipp in ctxt.TippMatchList)
                {
                    ctxt.Entry(tipp).State = EntityState.Deleted;
                }

                ctxt.SaveChanges();
            }

            // delete all users
            var users = GetUserList();

            foreach (var user in users)
            {
                var username = user.username;

                try
                {
                    if (WebSecurity.UserExists(username) == true)
                    {
                        Membership.DeleteUser(username);
                    }
                }
                catch (MembershipCreateUserException e)
                {
                    log.ErrorFormat("User {0} not created: {1}", user.username, e.Message);
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult ClearCache()
        {
            var _cache = new DefaultCacheProvider();
            _cache.Clear();

            return RedirectToAction("Index");
        }

        public ActionResult GetAllMatches(int spieltag)
        {
            var matches = _matchDataRepository.GetMatchesByGroup(spieltag);

            using (var ctxt = new MatchInfoContext())
            {
                foreach (var m in matches)
                {
                    ctxt.Items.Add(new MatchInfoItem()
                    {
                        GroupId = m.GroupId,
                        MatchId = m.MatchId,
                        MatchNr = m.MatchNr,
                        HomeTeamId = m.HomeTeamId,
                        AwayTeamId = m.AwayTeamId,
                        HomeTeam = m.HomeTeam,
                        AwayTeam = m.AwayTeam,
                        HomeTeamScore = m.HomeTeamScore,
                        AwayTeamScore = m.AwayTeamScore,
                        HomeTeamIcon = m.HomeTeamIcon,
                        AwayTeamIcon = m.AwayTeamIcon,
                        KickoffTime = m.KickoffTime,
                        KickoffTimeUtc = m.KickoffTimeUTC,
                        IsFinished = m.IsFinished,
                        HasProlongation = m.HasVerlaengerung
                    });
                }

                ctxt.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        private List<MatchInfoModel> GetMatchModelListBySpieltag(int spieltag)
        {
            using (var client = new SvcFussballDB.SportsdataSoapClient())
            {
                var matchList = new List<MatchInfoModel>();

                var oddsList = WettfreundeScraper.Scrap(spieltag);

                var matches = client.GetMatchdataByGroupLeagueSaison(spieltag, SportsdataConfigInfo.Current.LeagueShortcut, SportsdataConfigInfo.Current.LeagueSaison);


                foreach (var m in matches)
                {
                    var matchInfoModel = new MatchInfoModel();
                    matchInfoModel.MatchId = m.matchID;
                    matchInfoModel.KickoffTime = m.matchDateTime;
                    matchInfoModel.IsFinished = m.matchIsFinished;
                    matchInfoModel.HomeTeam = m.nameTeam1;
                    matchInfoModel.AwayTeam = m.nameTeam2;
                    matchInfoModel.HomeTeamIcon = m.iconUrlTeam1;
                    matchInfoModel.AwayTeamIcon = m.iconUrlTeam2;
                    matchInfoModel.HomeTeamScore = m.pointsTeam1;
                    matchInfoModel.AwayTeamScore = m.pointsTeam2;

                    // mixin odds quotes into match data
                    {
                        MixinOddsQuotes(oddsList, matchInfoModel);
                    }

                    matchList.Add(matchInfoModel);
                }

                return matchList;
            }
        }

        private static void MixinOddsQuotes(List<OddsInfoModel> oddsList, MatchInfoModel m)
        {
            var homeTeamUpper = m.HomeTeam.ToUpper();
            var awayTeamUpper = m.AwayTeam.ToUpper();

            var oddsMatch = (from o in oddsList
                             where (homeTeamUpper.Contains(o.HomeTeamSearch) || awayTeamUpper.Contains(o.AwayTeamSearch))
                             select o)
                            .First();

            m.HomeTeamOdds = oddsMatch.WinOdds;
            m.AwayTeamOdds = oddsMatch.LossOdds;
            m.DrawOdds = oddsMatch.DrawOdds;
        }

        private List<dynamic> GetUserList()
        {
            var users = new[] {  
                new { username = "stefan_z", email= "stefan.zeisberger@bf.uzh.ch"},
                new { username = "remo", email= "remo.stoessel@bf.uzh.ch"},
                new { username = "dieter", email= "dniggeler@bhfs.ch"},
                new { username = "thorsten", email= "thorsten.hens@bf.uzh.ch"},
                new { username = "karl", email= "karl.schmedders@business.uzh.ch"},
                new { username = "dominik", email= "Dominik.Hlinka@web.de"},
                new { username = "mo", email= "mhl@agfif.com"},
                new { username = "eckart", email= "eckart.jaeger@bf.uzh.ch"},
                new { username = "stefan_r", email= "stefan.rehder@via-value.de"},
            };

            return users.ToList<dynamic>();
        }

        private DailyWinnerInfoModel DailyWinnersInternal(int currSpieltag, bool IsAdminView)
        {
            var viewModel = new DailyWinnerInfoModel();

            using (var client = new SvcFussballDB.SportsdataSoapClient())
            {
                var matchesDB = client.GetMatchdataByGroupLeagueSaison(currSpieltag,
                                                                     SportsdataConfigInfo.Current.LeagueShortcut,
                                                                     SportsdataConfigInfo.Current.LeagueSaison);

                foreach (var m in matchesDB)
                {
                    viewModel.MatchInfo.Add(OpenDBHelper.Create(m));
                }

                using (var ctxt = new TippSpielContext())
                {
                    var resultDict = new Dictionary<string, RankingInfoModel>();
                    using (var userCtxt = new UsersContext())
                    {
                        // init result dict
                        {
                            foreach (var username in (from t in ctxt.TippMatchList select t.User).Distinct())
                            {
                                var m = new RankingInfoModel();
                                m.User = username;
                                m.DisplayName = (from u in userCtxt.UserProfiles
                                                 where u.UserName == username
                                                 select u.DisplayName)
                                                 .FirstOrDefault();

                                resultDict.Add(username, m);
                                viewModel.AllTippInfoDict.Add(username, new List<MatchInfoModel>());
                            }
                        }
                    }

                    // 1. get all tipps for match with id
                    foreach (var match in matchesDB)
                    {
                        var tippSet = (from t in ctxt.TippMatchList
                                       where t.MatchId == match.matchID &&
                                             t.MyTip.HasValue &&
                                             t.MyAmount.HasValue &&
                                             t.MyOdds.HasValue
                                       select t);
                        foreach (var tip in tippSet)
                        {
                            var matchModelObj = OpenDBHelper.Create(match);
                            matchModelObj.MyOdds = tip.MyOdds;
                            matchModelObj.MyAmount = tip.MyAmount;
                            matchModelObj.MyTip = tip.MyTip;

                            if (matchModelObj.HasStarted == true || IsAdminView == true)
                            {
                                resultDict[tip.User].TippCount++;
                                resultDict[tip.User].TotalPoints += (matchModelObj.MyPoints.HasValue) ? matchModelObj.MyPoints.Value : 0.0;
                                viewModel.AllTippInfoDict[tip.User].Add(matchModelObj);
                            }
                        }
                    }

                    var resultList = (from kp in resultDict select kp.Value).ToList();

                    viewModel.Ranking = (from e in resultList
                                         orderby e.TotalPoints descending, e.PointAvg, e.TippCount descending
                                         select e)
                                  .ToList();

                    int counter = 1;
                    viewModel.Ranking.ForEach(e => { e.Rang = counter++; });

                    return viewModel;
                }
            }
        }

    }
}
