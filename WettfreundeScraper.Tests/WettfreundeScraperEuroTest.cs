using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using HtmlAgilityPack;
using OddsScraper.Contract;
using WebScraperOdds;
using WettfreundeScraper.Tests.Properties;
using Xunit;
using Xunit.Abstractions;

namespace WettfreundeScraper.Tests
{
    [Trait("Wettfreunde Scraper", "Euro Test")]
    public class WettfreundeScraperEuroTest
    {
        private readonly ITestOutputHelper _output;
        private readonly IOddsScraper _oddsScraper = new WettfreundeOddsEuroScraper();

        public WettfreundeScraperEuroTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact(DisplayName = "Vorrundenspiele")]
        public void GetSpieltagTest()
        {
            // given
            string stage = "1";

            // when
            var result = _oddsScraper.GetOdds(Resources.EMTippspiel, stage);
            _output.WriteLine("{0}", result.Count);

            // then
            result.Should().NotBeNull().And.Subject.Should().HaveCount(36);

        }

        [Fact(DisplayName = "Achtelfinals")]
        public void GetAchtelfinalsTest()
        {
            // given
            string stage = "7";

            // when
            var result = _oddsScraper.GetOdds(Resources.EMTippspiel_Achtelfinals, stage);
            _output.WriteLine("{0}", result.Count);

            // then
            result.Should().NotBeNull().And.Subject.Should().HaveCount(8);

        }

        [Fact(DisplayName = "Viertelfinals")]
        public void GetViertelfinalsTest()
        {
            // given
            string stage = "8";

            // when
            var result = _oddsScraper.GetOdds(Resources.EMTippspiel_Viertelfinals, stage);
            _output.WriteLine("{0}", result.Count);

            // then
            result.Should().NotBeNull().And.Subject.Should().HaveCount(4);

        }

        [Fact(DisplayName = "Final")]
        public void GetFinalTest()
        {
            // given
            string stage = "10";

            // when
            var result = _oddsScraper.GetOdds(Resources.EMTippspiel_Final, stage);
            _output.WriteLine("{0}", result.Count);

            // then
            result.Should().NotBeNull().And.Subject.Should().HaveCount(1);

        }
    }
}
