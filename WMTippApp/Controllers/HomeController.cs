using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Web;
using System.Web.Mvc;
using BhFS.Tippspiel.Utils;
using WMTippApp.Models;
using System.Web.Caching;
using WMTippApp.Mailers;
using FussballTipp.Repository;
using FussballTipp.Utils;
using OddsScraper.Contract.Model;
using WebScraperOdds;
using WMTippApp.Properties;
using WMTippApp.SvcFussballDB;

namespace WMTippApp.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private ITippMailer _tippMailer = new TippMailer();
        private readonly IBettingQuotesRepository _quoteRepository = 
            new WettfreundeWorldcupScraper( WettfreundeWorldCupConfigInfo.Current, new WettfreundeOddsWorldcupScraper());

        private readonly IFussballDataRepository _matchDataRepository = 
            new WMFussballDataRepository(SportsdataConfigInfo.Current);

        public ITippMailer TippMailer
        {
            get { return _tippMailer; }
            set { _tippMailer = value; }
        }

        public IAccessStats MatchDBStats
        {
            get { return _matchDataRepository as IAccessStats; }
        }

        [AcceptVerbs(HttpVerbs.Get|HttpVerbs.Post)]
        public ActionResult Index(int? Spieltag)
        {
            log.Debug("Index begin");

            var allGroups = _matchDataRepository.GetAllGroups();

            int currentSpieltag = Spieltag ?? SportsdataConfigInfo.Current.CurrentSpieltag;

            // build dropdown list data
            {
                var count = SportsdataConfigInfo.Current.EndSpieltag - SportsdataConfigInfo.Current.StartSpieltag +1;
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
            }

            var model = new SpieltagModel();
            model.GroupId = currentSpieltag;
            model.GroupName = allGroups.Find(g => g.Id==currentSpieltag).Text;

            var oddsList = _quoteRepository.GetOdds();

            var matches = _matchDataRepository.GetMatchesByGroup(currentSpieltag);

            matches = (from m in matches
                       orderby m.KickoffTime
                       select m).ToList();

            foreach (var m in matches)
            {
                var modelAllInfo = new MatchInfoModel()
                {
                    MatchId = m.MatchId,
                    GroupId = m.GroupId,
                    MatchNr = m.MatchNr,
                    AwayTeam = m.AwayTeam,
                    AwayTeamIcon = m.AwayTeamIcon,
                    AwayTeamScore = m.AwayTeamScore,
                    HomeTeam = m.HomeTeam,
                    HomeTeamIcon = m.HomeTeamIcon,
                    HomeTeamScore = m.HomeTeamScore,
                    IsFinished = m.IsFinished,
                    KickoffTime = m.KickoffTime
                };

                // mixin odds quotes into match data
                MixinOddsQuotes(oddsList, modelAllInfo);

                using (var ctxt = new TippSpielContext())
                {
                    var myTippObject = (from t in ctxt.TippMatchList
                            where t.MatchId == modelAllInfo.MatchId &&
                                  t.User == User.Identity.Name
                            select t)
                        .FirstOrDefault();

                    if (myTippObject != null)
                    {
                        modelAllInfo.MyOdds = myTippObject.MyOdds;
                        modelAllInfo.MyTip = myTippObject.MyTip;
                        modelAllInfo.IsJoker = myTippObject.IsJoker;
                        modelAllInfo.MyAmount = myTippObject.MyAmount;
                    }
                }


                model.Matchdata.Add(modelAllInfo);
            }

            model.MaxJokers = TippspielConfigInfo.Current.MaxJokerTips;
            model.NumJokersUsed = model.Matchdata.Count(m => m.IsJoker);

            {

                log.DebugFormat("Tipp data stats: remote hits={0}, cache hits={1}", MatchDBStats.GetRemoteHits(), MatchDBStats.GetCacheHits());
            }

            log.Debug("Index end");

            return View(model);
        }

        [AllowAnonymous]
        public ActionResult OverallStanding()
        {
            _matchDataRepository.GetCurrentGroup();
            var groupModel = _matchDataRepository.GetCurrentGroup();

            var maxSpieltag = groupModel.Id;

            using (var ctxt = new TippSpielContext())
            {
                var resultDict = new Dictionary<string, RankingInfoModel>();
                using (var userCtxt = new UsersContext())
                {
                    // init dict
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
                        }
                    }
                }

                foreach (var tip in ctxt.TippMatchList.Where(t=>t.MyTip.HasValue))
                {
                    var rankingObj = resultDict[tip.User];

                    var matchInfo = _matchDataRepository.GetMatchData(tip.MatchId);

                    if (matchInfo.LeagueShortcut == SportsdataConfigInfo.Current.LeagueShortcut && 
                        tip.MyOdds.HasValue && 
                        tip.MyAmount.HasValue)
                    {
                        var matchModelObj = new MatchInfoModel()
                        {
                            MatchId = matchInfo.MatchId,
                            MatchNr = matchInfo.MatchNr,
                            AwayTeam = matchInfo.AwayTeam,
                            AwayTeamIcon = matchInfo.AwayTeamIcon,
                            AwayTeamScore = matchInfo.AwayTeamScore,
                            HomeTeam = matchInfo.HomeTeam,
                            HomeTeamIcon = matchInfo.HomeTeamIcon,
                            HomeTeamScore = matchInfo.HomeTeamScore,
                            IsFinished = matchInfo.IsFinished,
                            KickoffTime = matchInfo.KickoffTime,
                            GroupId = tip.GroupId,
                            MyOdds = tip.MyOdds,
                            MyAmount = tip.MyAmount,
                            MyTip = tip.MyTip,
                            IsJoker = tip.IsJoker,
                        };

                        if (tip.IsJoker == true)
                        {
                            matchModelObj.MyAmount = tip.MyAmount * TippspielConfigInfo.Current.JokerMultiplicator;
                        }
                        else
                        {
                            matchModelObj.MyAmount = tip.MyAmount;
                        }


                        if (matchModelObj.HasStarted == true)
                        {
                            rankingObj.TippCount++;
                            rankingObj.TotalPoints += matchModelObj.MyPoints ?? 0.0;
                            rankingObj.TotalPointsClean += matchModelObj.MyPointsClean ?? 0.0;

                            if(tip.IsJoker)
                            {
                                rankingObj.JokerUsed++;
                            }
                        }
                    }
                }

                var resultList = (from kp in resultDict select kp.Value).ToList();

                resultList = (from e in resultList
                                orderby e.TotalPoints descending, e.PointAvg, e.TippCount descending
                                select e)
                                .ToList();

                int counter = 1;
                resultList.ForEach(e => { e.Rang = counter++; });

                return View(resultList);
            }
         }

        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        [AllowAnonymous]
        public ActionResult DailyReport(DateTime? Spieltag)
        {
            log.Debug("DailyReport begin");

            var kickoff = Spieltag;

            // extract spieltage
            if(kickoff == null)
            {
                var match = _matchDataRepository.GetLastMatch();
                if (match.MatchId == -1)
                {
                    match = _matchDataRepository.GetNextMatch();
                }

                kickoff = match.KickoffTimeUTC;
            }

            var matchFilter = (from m in _matchDataRepository.GetAllMatches()
                               where m.KickoffTimeUTC.ToShortDateString() == kickoff.Value.ToShortDateString() 
                               && m.HasStarted == true
                                select m);


            // build dropdown list data
            {
                var distinctByKickoff = (from m in _matchDataRepository.GetAllMatches()
                                         orderby m.KickoffTime
                                         select new
                                         {
                                             KickoffDate = m.KickoffTime.ToShortDateString(),
                                             KickoffDateUTC = m.KickoffTimeUTC.ToShortDateString()
                                         })
                                         .DistinctBy(a => a.KickoffDateUTC);

                var ddlSpieltageRange = (from m in distinctByKickoff
                                         select new SelectListItem()
                                         {
                                             Value = m.KickoffDateUTC,
                                             Text = m.KickoffDate,
                                             Selected = (m.KickoffDateUTC == kickoff.Value.ToShortDateString())
                                         })
                                         .Distinct();

                ViewBag.Spieltag = ddlSpieltageRange;
            }

            var viewModel = new DailyWinnerInfoModel();
            {
                viewModel.MatchInfo = matchFilter.ToList();
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
                foreach (var m in matchFilter)
                {
                    var tippSet = (from t in ctxt.TippMatchList
                                   where t.MatchId == m.MatchId &&
                                         t.MyTip.HasValue &&
                                         t.MyAmount.HasValue &&
                                         t.MyOdds.HasValue
                                   select t);
                    foreach (var tip in tippSet)
                    {
                        var matchModelObj = new MatchInfoModel()
                        {
                            MatchId = m.MatchId,
                            GroupId = m.GroupId,
                            MatchNr = m.MatchNr,
                            AwayTeam = m.AwayTeam,
                            AwayTeamIcon = m.AwayTeamIcon,
                            AwayTeamScore = m.AwayTeamScore,
                            HomeTeam = m.HomeTeam,
                            HomeTeamIcon = m.HomeTeamIcon,
                            HomeTeamScore = m.HomeTeamScore,
                            IsFinished = m.IsFinished,
                            KickoffTime = m.KickoffTime
                        };

                        matchModelObj.MyOdds = tip.MyOdds;
                        matchModelObj.IsJoker = tip.IsJoker;
                        if(tip.IsJoker == true)
                        {
                            matchModelObj.MyAmount = tip.MyAmount*TippspielConfigInfo.Current.JokerMultiplicator;
                        }
                        else
                        {
                            matchModelObj.MyAmount = tip.MyAmount;
                        }
                        matchModelObj.MyTip = tip.MyTip;

                        if (matchModelObj.HasStarted == true)
                        {
                            resultDict[tip.User].TippCount++;
                            resultDict[tip.User].TotalPoints += matchModelObj.MyPoints ?? 0.0;
                        }

                        if (matchModelObj.HasStarted == true)
                        {
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
            }

            log.Debug("DailyReport end");

            return View(viewModel);
        }

        [HttpPost]
        public JsonResult SetMatchTipp(int id, int groupId, int tip, double? odds)
        {
            try
            {
                var user = User.Identity.Name.Trim().ToLower();

                log.Debug($"match id={id}, tip={tip}, odds={odds:0.00}, user={user}");

                var matchToTip = _matchDataRepository.GetMatchData(id);

                if (matchToTip != null)
                {
                    if (matchToTip.KickoffTime < DateTime.Now)
                    {
                        log.Warn($"User {user} tried to change tip for match {id} after kickoff time utc {matchToTip.KickoffTime}");

                        return Json(new
                        {
                            Success = false,
                            Error = "Tipp kann nicht mehr entgegengenommen werden. Das Spiel hat bereits begonnnen.",
                            MatchId = id
                        });
                    }
                }

                using (var ctxt = new TippSpielContext())
                {
                    var matchObj = (from m in ctxt.TippMatchList
                            where m.MatchId == id &&
                                  m.User == user
                            select m)
                        .FirstOrDefault();

                    if (matchObj == null)
                    {
                        var newMatchObj = new TippMatchModel()
                        {
                            MatchId = id,
                            GroupId = groupId,
                            MyOdds = odds,
                            MyTip = tip,
                            MyAmount = MapGroup2Amount(groupId),
                            User = user,
                            LastUpdated = DateTime.Now
                        };

                        ctxt.TippMatchList.Add(newMatchObj);
                    }
                    else
                    {
                        matchObj.LastUpdated = DateTime.Now;
                        matchObj.GroupId = groupId;
                        matchObj.User = user;
                        matchObj.MyOdds = odds;
                        matchObj.MyTip = tip;
                        matchObj.MyAmount = MapGroup2Amount(groupId);
                        matchObj.MyTip = tip;
                    }

                    ctxt.SaveChanges();

                    var result = new
                    {
                        Success = true,
                        MatchId = id,
                        MyOdds = $"{odds:0.00}"
                    };

                    log.DebugFormat("Tipp data stats: remote hits={0}, cache hits={1}", MatchDBStats.GetRemoteHits(),
                        MatchDBStats.GetCacheHits());

                    return Json(result);
                }
            }
            catch (FormatException ex)
            {
                log.ErrorFormat("Match id cannot be converted, id={0}." + id.ToString());
                log.ErrorFormat("Exception message={0}." + ex.Message);

                return Json(new {
                    Success = false,
                    Error = ex.Message,
                    MatchId = id
                });
            }
        }

        [AllowAnonymous]
        public ActionResult About()
        {
            ViewBag.Message = Resources.Title_Long;

            return View();
        }

        private SvcFussballDB.Matchdata AddOrReplaceMatchToCache(SvcFussballDB.Matchdata m)
        {
            HttpContext.Cache.Insert(m.matchID.ToString(), m, null, Cache.NoAbsoluteExpiration, new TimeSpan(1, 0, 0), CacheItemPriority.Normal, null);

            return m;
        }

        private SvcFussballDB.Matchdata GetMatchFromCache(int matchId)
        {
            var obj = HttpContext.Cache.Get(matchId.ToString());

            return obj as Matchdata;
        }

        private static double MapGroup2Amount(int groupId)
        {
            switch (groupId)
            {
                case 1: 
                case 2: 
                case 3: 
                    return 1.0;
                case 4: return 2.0;
                case 5: return 4.0;
                case 6: return 6.0;
                case 7: return 8.0;
                default: return 1.0;
            }
        }

        private static void MixinOddsQuotes(List<OddsInfoModel> oddsList, MatchInfoModel m)
        {
            var homeTeamUpper = m.HomeTeam.ToUpper();
            var awayTeamUpper = m.AwayTeam.ToUpper();

            var oddsMatch = (from o in oddsList
                             where (homeTeamUpper.Contains(o.HomeTeamSearch) && awayTeamUpper.Contains(o.AwayTeamSearch))
                             select o)
                            .FirstOrDefault();

            if (oddsMatch == null)
            {
                oddsMatch = (from o in oddsList
                             where (homeTeamUpper.Contains(o.HomeTeamSearch2) && awayTeamUpper.Contains(o.AwayTeamSearch2))
                             select o)
                            .FirstOrDefault();
            }

            if (oddsMatch != null)
            {
                m.HomeTeamOdds = oddsMatch.WinOdds;
                m.AwayTeamOdds = oddsMatch.LossOdds;
                m.DrawOdds = oddsMatch.DrawOdds;
            }

            // find favorite and longshot tipp
            if (m.DrawOdds.HasValue &&
                m.HomeTeamOdds.HasValue &&
                m.AwayTeamOdds.HasValue)
            {
                double[] odds = new double[]{
                                    m.DrawOdds.Value,
                                    m.HomeTeamOdds.Value,
                                    m.AwayTeamOdds.Value
                                };
            }
        }
    }
}
