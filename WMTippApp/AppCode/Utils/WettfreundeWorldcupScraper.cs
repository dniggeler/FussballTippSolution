﻿using System;
using System.Collections.Generic;
using FussballTipp.Repository;
using FussballTipp.Utils;
using OddsScraper.Contract;
using OddsScraper.Contract.Model;
using WebScraperOdds;

namespace BhFS.Tippspiel.Utils
{
    public class WettfreundeWorldcupScraper : IBettingQuotesRepository, IAccessStats
    {
        private readonly IOddsScraper _oddsScraper;
        private readonly string _url;

        private static int _remoteHits;
        private static int _cacheHits = 0;


        public WettfreundeWorldcupScraper(IOddsScraperConfig config, IOddsScraper oddsScraper)
        {
            _oddsScraper = oddsScraper;
            string domainUrl = config.BaseUrl;
            _url = domainUrl + config.OddsLink;
        }

        public int GetRemoteHits()
        {
            return _remoteHits;
        }

        public int GetCacheHits()
        {
            return _cacheHits;
        }

        public List<OddsInfoModel> GetOdds()
        {
            return GetOdds(1);
        }

        public List<OddsInfoModel> GetOdds(int spieltag)
        {
            _remoteHits++;

            var oddsList = _oddsScraper.LoadOdds(_url, spieltag.ToString());

            return oddsList;
        }
    }
}