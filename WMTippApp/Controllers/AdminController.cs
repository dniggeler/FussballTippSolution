using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BhFS.Tippspiel.Utils;
using WMTippApp.Models;
using WMTippApp.Filters;
using FussballTipp.Repository;
using WebMatrix.WebData;
using System.Data.SqlClient;
using OddsScraper.Contract.Model;
using WebScraperOdds;

namespace WMTippApp.Controllers
{
    public class MigrationResultModel
    {
        public string CurrentLeagueShortcut { get; set; }
        public string CurrentLeagueSeason { get; set; }
    }

    [InitializeSimpleMembership]
    public class AdminController : Controller
    {
        readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly IBettingQuotesRepository _quoteRepository = new WettfreundeEuroScraper(WettfreundeEuroConfigInfo.Current, new WettfreundeOddsEuroScraper());
        private readonly IFussballDataRepository _matchDataRepository = new WMFussballDataRepository(SportsdataConfigInfo.Current);

        public ActionResult Index(int? Spieltag)
        {
            var allGroups = _matchDataRepository.GetAllGroups();

            int currentSpieltag = (Spieltag.HasValue == true) ? Spieltag.Value : SportsdataConfigInfo.Current.StartSpieltag;

            // build dropdown list data
            {
                var count = SportsdataConfigInfo.Current.EndSpieltag - SportsdataConfigInfo.Current.StartSpieltag + 1;
                var ddlSpieltageRange = (from e in allGroups
                                         where e.Id <= SportsdataConfigInfo.Current.EndSpieltag &&
                                               e.Id >= SportsdataConfigInfo.Current.StartSpieltag
                                         select new SelectListItem()
                                         {
                                             Value = e.Id.ToString(),
                                             Text = e.Text,
                                             Selected = (e.Id == currentSpieltag)
                                         });

                ViewBag.Spieltag = ddlSpieltageRange;

                var quotes = _quoteRepository.GetOdds();
            }

            return View();
        }

        public ActionResult DeleteTipps()
        {
            // delete all tipper data
            using (var ctxt = new TippSpielContext())
            {
                foreach (var tipp in ctxt.TippMatchList)
                {
                    ctxt.Entry(tipp).State = System.Data.EntityState.Deleted;
                }

                ctxt.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Clear()
        {
            // delete all tipper data
            using (var ctxt = new TippSpielContext())
            {
                foreach (var tipp in ctxt.TippMatchList)
                {
                    ctxt.Entry(tipp).State = System.Data.EntityState.Deleted;
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

        public ActionResult ResetPassword(string user)
        {
            string token = WebSecurity.GeneratePasswordResetToken(user);

            if (WebSecurity.ResetPassword(token, "ibf123-") == true)
            {
                log.Debug("Password reset successful");
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult MigrateOpenliga()
        {

            var m = new MigrationResultModel()
            {
                CurrentLeagueSeason = SportsdataConfigInfo.Current.LeagueSaison,
                CurrentLeagueShortcut = SportsdataConfigInfo.Current.LeagueShortcut
            };

            return View(m);
        }

        [HttpPost]
        public ActionResult MigrateOpenliga(string newLeagueSeason, string newLeagueShortcut)
        {
            var m = new MigrationResultModel()
            {
                CurrentLeagueSeason = SportsdataConfigInfo.Current.LeagueSaison,
                CurrentLeagueShortcut = SportsdataConfigInfo.Current.LeagueShortcut
            };

            // precondition: db exists under new name
            {
                if (_matchDataRepository.Exist(newLeagueShortcut, newLeagueSeason))
                {
                    log.DebugFormat("League {0}/{1} is valid", newLeagueShortcut, newLeagueSeason);

                    var newClient = new WMFussballDataRepository(newLeagueShortcut, newLeagueSeason);

                    var newMatches = newClient.GetMatchesByCurrentGroup();

                    using (var ctxt = new TippSpielContext())
                    {
                        var migrationList = new List<TippMatchModel>();

                        foreach (var tipp in ctxt.TippMatchList)
                        {
                            //
                            // Basic idea:
                            // 1. is match tipp among the matches to be migrated, if yes it is already migrated => finish
                            // 2. if no, then migrate it but only if tipp belongs to old league. Do not touch others
                            //

                            var newMatchObj = (from t in newMatches
                                               where t.MatchId == tipp.MatchId 
                                               select t)
                                                .FirstOrDefault();

                            if (newMatchObj == null)
                            {
                                var oldMatchObj = _matchDataRepository.GetMatchData(tipp.MatchId);

                                if (oldMatchObj != null)
                                {

                                    // find corresponding match in new league
                                    var newMatchByTeams = (from t in newMatches
                                                           where t.HomeTeamId == oldMatchObj.HomeTeamId &&
                                                                 t.AwayTeamId == oldMatchObj.AwayTeamId
                                                           select t)
                                                           .FirstOrDefault();

                                    if (newMatchByTeams != null)
                                    {
                                        using (var ctxt2 = new TippSpielContext())
                                        {
                                            var tippObj = (from t in ctxt2.TippMatchList
                                                           where t.User == tipp.User &&
                                                                 t.MatchId == newMatchByTeams.MatchId
                                                           select t)
                                                           .FirstOrDefault();

                                            if (tippObj == null) // not null means tipp has already been migrated
                                            {
                                                var newTipp = new TippMatchModel()
                                                {
                                                    MatchId = newMatchByTeams.MatchId,
                                                    GroupId = tipp.GroupId,
                                                    IsJoker = tipp.IsJoker,
                                                    LastUpdated = DateTime.Now,
                                                    MyAmount = tipp.MyAmount,
                                                    MyOdds = tipp.MyOdds,
                                                    MyTip = tipp.MyTip,
                                                    User = tipp.User
                                                };

                                                migrationList.Add(newTipp);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        // save migration list
                        foreach(var t in migrationList)
                        {
                            log.DebugFormat("Migrate match={0} for user={1}", t.MatchId, t.User);

                            ctxt.TippMatchList.Add(t);
                        }

                        ctxt.SaveChanges();
                    }
                }
                else
                {
                    var errMsg = String.Format("League {0}/{1} does not exist", newLeagueShortcut, newLeagueSeason);
                    log.Debug(errMsg);

                    ModelState.AddModelError("league",errMsg);
                }
            }

            return View(m);
        }

        public ActionResult CleanupDuplicates()
        {
            var matches = _matchDataRepository.GetAllMatches();

            var list2deleted = new List<int>();

            foreach (var m in matches)
            {
                List<string> users = null;
                using (var ctxt = new TippSpielContext())
                {
                    users = (from t in ctxt.TippMatchList select t.User).Distinct().ToList();
                }

                foreach (var username in users)
                {
                    using(var ctxt = new TippSpielContext())
                    {
                        var filter = (from t in ctxt.TippMatchList
                                        where t.MatchId == m.MatchId &&
                                            t.User == username
                                        select t);

                        int count = filter.Count();

                        if (count > 1)
                        {
                            list2deleted.Add(filter.First().Id);
                        }
                    }
                }
            }

            using (var ctxt = new TippSpielContext())
            {
                foreach (var e in list2deleted)
                {
                    var match = (from m in ctxt.TippMatchList
                                    where m.Id == e
                                    select m)
                                    .First();

                    ctxt.Entry(match).State = System.Data.EntityState.Deleted;
                }

                ctxt.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult CorrectJokerTipps()
        {
            var allMatches = _matchDataRepository.GetAllMatches();

            using (var ctxt = new TippSpielContext())
            {
                var jokerTipps = (from t in ctxt.TippMatchList
                                   where t.IsJoker == true
                                   select t);

                foreach (var t in jokerTipps)
                {
                    if(allMatches.Where(m=>m.MatchId == t.MatchId).FirstOrDefault() == null)
                    {
                        t.IsJoker = false;
                    }
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

                var scraper = new WettfreundeEuroScraper(WettfreundeEuroConfigInfo.Current, new WettfreundeOddsEuroScraper());

                var oddsList = scraper.GetOdds();

                var matches = client.GetMatchdataByGroupLeagueSaison(spieltag, SportsdataConfigInfo.Current.LeagueShortcut, SportsdataConfigInfo.Current.LeagueSaison);


                foreach (var m in matches)
                {
                    var matchInfoModel = new MatchInfoModel
                    {
                        MatchId = m.matchID,
                        KickoffTime = m.matchDateTime,
                        IsFinished = m.matchIsFinished,
                        HomeTeam = m.nameTeam1,
                        AwayTeam = m.nameTeam2,
                        HomeTeamIcon = m.iconUrlTeam1,
                        AwayTeamIcon = m.iconUrlTeam2,
                        HomeTeamScore = m.pointsTeam1,
                        AwayTeamScore = m.pointsTeam2
                    };

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

    }
}
