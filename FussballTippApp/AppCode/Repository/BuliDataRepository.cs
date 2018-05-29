using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BhFS.Tippspiel.Utils;
using FussballTipp.Utils;

namespace FussballTipp.Repository
{
    public class BuLiDataRepository : IFussballDataRepository, IAccessStats, IDisposable
    {
        public int StartGroup { get; set; }
        public int EndGroup { get; set; }

        public BuLiDataRepository(SportsdataConfigInfo info)
            :  this(info.LeagueShortcut, info.LeagueSaison)
        {}

        public BuLiDataRepository(string leagueShortcut, string leagueSeason)
        {
            _leagueTag = leagueShortcut;
            _saisonTag = leagueSeason;
            _client = new FussballTippApp.SvcFussballDB.SportsdataSoapClient();
        }

        public int GetRemoteHits()
        {
            return _remoteHits;
        }

        public int GetCacheHits()
        {
            return _cacheHits;
        }

        public GroupInfoModel GetCurrentGroup()
        {
            const string CACHE_TAG = "cacheCurrGrp";

            FussballTippApp.SvcFussballDB.Group g = null;
            if (_cache.IsSet(CACHE_TAG))
            {
                g = (FussballTippApp.SvcFussballDB.Group)_cache.Get(CACHE_TAG);
                _cacheHits++;
            }
            else
            {
                g = _client.GetCurrentGroup(_leagueTag);
                _cache.Set(CACHE_TAG, g, CACHE_DURATION);

                _remoteHits++;
            }

            return new GroupInfoModel(g.groupOrderID, g.groupName);
        }

        List<GroupInfoModel> IFussballDataRepository.GetAllGroups()
        {
            const string CACHE_TAG = "cacheAllGrps";

            FussballTippApp.SvcFussballDB.Group[] groups = null;
            if (_cache.IsSet(CACHE_TAG))
            {
                groups = (FussballTippApp.SvcFussballDB.Group[])_cache.Get(CACHE_TAG);
                _cacheHits++;
            }
            else
            {
                groups = _client.GetAvailGroups(_leagueTag, _saisonTag);
                _cache.Set(CACHE_TAG, groups, CACHE_DURATION);
                _remoteHits++;
            }

            var groupList = new List<GroupInfoModel>();
            foreach (var g in groups)
            {
                groupList.Add(new GroupInfoModel(g.groupOrderID, g.groupName));
            }

            return groupList;
        }

        public List<MatchDataModel> GetAllMatches()
        {
            string CACHE_ALL_MATCH_TAG = "cacheAllMatches" + _leagueTag + _saisonTag;
            string CACHE_MATCH_TAG = CACHE_MATCH_PREFIX + _leagueTag;

            FussballTippApp.SvcFussballDB.Matchdata[] matches = null;
            if (_cache.IsSet(CACHE_ALL_MATCH_TAG))
            {
                matches = (FussballTippApp.SvcFussballDB.Matchdata[])_cache.Get(CACHE_ALL_MATCH_TAG);

                _cacheHits++;
            }
            else
            {
                matches = _client.GetMatchdataByLeagueSaison(_leagueTag, _saisonTag);
                _cache.Set(CACHE_ALL_MATCH_TAG, matches, CACHE_DURATION);

                // cache single matches
                foreach (var m in matches)
                {
                    _cache.Set(CACHE_MATCH_TAG + m.matchID.ToString(), m, CACHE_DURATION);
                }

                _remoteHits++;
            }

            var mList = new List<MatchDataModel>();

            foreach (var m in matches)
            {
                mList.Add(Create(m));
            }

            return mList;            
        }

        public MatchDataModel GetNextMatch()
        {
            string CACHE_NXT_MATCH_TAG = "cacheNxtGame"+_leagueTag;
            string CACHE_MATCH_TAG = CACHE_MATCH_PREFIX + _leagueTag;

            FussballTippApp.SvcFussballDB.Matchdata m = null;
            if (_cache.IsSet(CACHE_NXT_MATCH_TAG))
            {
                m = (FussballTippApp.SvcFussballDB.Matchdata)_cache.Get(CACHE_NXT_MATCH_TAG);
                _cacheHits++;
            }
            else
            {
                m = _client.GetNextMatch(_leagueTag);
                _cache.Set(CACHE_NXT_MATCH_TAG, m, CACHE_DURATION);
                _cache.Set(CACHE_MATCH_TAG + m.matchID.ToString(), m, 10);
                _remoteHits++;
            }

            return Create(m);
        }

        public MatchDataModel GetLastMatch()
        {
            string CACHE_LAST_MATCH_TAG = "cacheLastGame" + _leagueTag;
            string CACHE_MATCH_TAG = CACHE_MATCH_PREFIX + _leagueTag;

            FussballTippApp.SvcFussballDB.Matchdata m = null;
            if (_cache.IsSet(CACHE_LAST_MATCH_TAG))
            {
                m = (FussballTippApp.SvcFussballDB.Matchdata)_cache.Get(CACHE_LAST_MATCH_TAG);
                _cacheHits++;
            }
            else
            {
                m = _client.GetLastMatch(_leagueTag);
                _cache.Set(CACHE_LAST_MATCH_TAG, m, CACHE_DURATION);
                _cache.Set(CACHE_MATCH_TAG + m.matchID.ToString(), m, CACHE_DURATION);
                _remoteHits++;
            }

            return Create(m);
        }

        public MatchDataModel GetMatchData(int matchId)
        {
            string CACHE_MATCH_TAG = CACHE_MATCH_PREFIX + _leagueTag+matchId.ToString();

            FussballTippApp.SvcFussballDB.Matchdata m = null;
            if (_cache.IsSet(CACHE_MATCH_TAG))
            {
                m = (FussballTippApp.SvcFussballDB.Matchdata)_cache.Get(CACHE_MATCH_TAG);
                _cacheHits++;
            }
            else
            {
                m = _client.GetMatchByMatchID(matchId);
                _cache.Set(CACHE_MATCH_TAG, m, CACHE_DURATION);
                _remoteHits++;
            }

            return Create(m);
        }

        public List<MatchDataModel> GetMatchesByGroup(int groupId)
        {
            string CACHE_MATCH_GROUP_TAG = "cacheGameByGrp" + groupId.ToString() + _leagueTag + _saisonTag;
            string CACHE_MATCH_TAG = CACHE_MATCH_PREFIX + _leagueTag;

            FussballTippApp.SvcFussballDB.Matchdata[] matches = null;
            if (_cache.IsSet(CACHE_MATCH_GROUP_TAG))
            {
                matches = (FussballTippApp.SvcFussballDB.Matchdata[])_cache.Get(CACHE_MATCH_GROUP_TAG);

                _cacheHits++;
            }
            else
            {
                matches = _client.GetMatchdataByGroupLeagueSaison(groupId, _leagueTag, _saisonTag);

                // check if data has been found at all
                if(matches.Count()==1)
                {
                    if (matches.First().matchID == -1)
                    {
                        return new List<MatchDataModel>();
                    }
                }

                _cache.Set(CACHE_MATCH_GROUP_TAG, matches, CACHE_DURATION);

                // cache single matches
                foreach(var m in matches)
                {
                    _cache.Set(CACHE_MATCH_TAG + m.matchID.ToString(), m, CACHE_DURATION);
                }

                _remoteHits++;
            }

            var mList = new List<MatchDataModel>();

            foreach (var m in matches)
            {
                mList.Add(Create(m));
            }

            return mList;
        }

        public List<MatchDataModel> GetMatchesByCurrentGroup()
        {
            return GetMatchesByGroup(this.GetCurrentGroup().Id);
        }

        bool IFussballDataRepository.IsSpieltagComplete
        {
            get {
                var mNext = this.GetNextMatch();
                var dataLast = this.GetLastMatch();

                if (mNext == null)
                {
                    return true;
                }
                else if (dataLast == null)
                {
                    return false;
                }
                else
                {
                    if (dataLast.GroupId < mNext.GroupId)
                    {
                        return true;
                    }

                    return false;
                }
            }
        }

        public bool Exist(string leagueShortcut, string leagueSeason)
        {
            var leagues = _client.GetAvailLeagues();

            var count = (from e in leagues
                             where e.leagueShortcut == leagueShortcut &&
                                   e.leagueSaison == leagueSeason
                             select e)
                             .Count();

            return count==1?true:false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _client.Close();
            }

            // Free any unmanaged objects here. 
            //
            _disposed = true;
        }

        public static MatchDataModel Create(FussballTippApp.SvcFussballDB.Matchdata match)
        {
            var matchModelObj = new MatchDataModel();
            matchModelObj.MatchId = match.matchID;
            matchModelObj.GroupId = match.groupOrderID;
            matchModelObj.KickoffTime = match.matchDateTime;
            matchModelObj.KickoffTimeUTC = match.matchDateTimeUTC;
            matchModelObj.HomeTeamId = match.idTeam1;
            matchModelObj.AwayTeamId = match.idTeam2;
            matchModelObj.HomeTeamScore = match.pointsTeam1;
            matchModelObj.AwayTeamScore = match.pointsTeam2;
            matchModelObj.HomeTeamIcon = match.iconUrlTeam1;
            matchModelObj.AwayTeamIcon = match.iconUrlTeam2;
            matchModelObj.HomeTeam = match.nameTeam1;
            matchModelObj.AwayTeam = match.nameTeam2;
            matchModelObj.IsFinished = match.matchIsFinished;
            matchModelObj.LeagueShortcut = match.leagueShortcut;

            if(match.matchResults != null && match.matchResults.Count() > 0)
            {
                var result = (from r in match.matchResults orderby r.resultTypeId descending select r).FirstOrDefault();

                if (result == null) return matchModelObj;

                matchModelObj.HomeTeamScore = result.pointsTeam1;
                matchModelObj.AwayTeamScore = result.pointsTeam2;
            }

            return matchModelObj;
        }

        private string CACHE_MATCH_PREFIX = "cacheGame";

        private static int _remoteHits = 0;
        private static int _cacheHits = 0;
        private const int CACHE_DURATION = 60;
        private ICacheProvider _cache = new DefaultCacheProvider();

        private string _leagueTag;
        private string _saisonTag;
        private FussballTippApp.SvcFussballDB.SportsdataSoapClient _client = null;

        bool _disposed = false;
    }
}