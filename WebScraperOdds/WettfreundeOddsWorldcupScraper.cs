using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using OddsScraper.Contract;
using OddsScraper.Contract.Model;
using ScrapySharp.Extensions;

namespace WebScraperOdds
{
    public class WettfreundeOddsWorldcupScraper : IOddsScraper
    {
        private const int Year = 2018;

        public List<OddsInfoModel> GetOdds(string oddsAsHtmlStr, string roundTag)
        {
            int spieltag = int.Parse(roundTag);

            var tableIndex = 1;
            switch (spieltag)
            {
                case 8:
                    tableIndex = 2;
                    break;
            }

            var oddsList = new List<OddsInfoModel>();

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(oddsAsHtmlStr);

            // remove all unnecessary html
            {
                ScrubHelper.ScrubHtml(doc);

                var nodes = doc.DocumentNode.SelectNodes("//comment()");
                if (nodes != null)
                {
                    foreach (HtmlNode comment in nodes)
                    {
                        comment.ParentNode.RemoveChild(comment);
                    }
                }
            }

            var htmlRows = doc.DocumentNode
                .SelectSingleNode("//body")
                .SelectSingleNode(string.Format("(//table)[{0}]", tableIndex))
                .CssSelect("tr");
            

            var iter = htmlRows.GetEnumerator();
            if (iter.MoveNext())
            {
                    // skip two header rows
                iter.MoveNext();
                while (iter.MoveNext())
                {
                    var newModel = new OddsInfoModel();

                    var cells = iter.Current.CssSelect("td").ToArray();

                    if (cells.Length > 0)
                    {
                        var kickoff = ExtractKickoff(cells[0].InnerText);
                        if (kickoff.HasValue == false)
                        {
                            continue;
                        }

                        var team1 = (spieltag < 7) ? ExtractTeams(cells[2]) : ExtractTeams(cells[1]);
                        var team2 = (spieltag < 7) ? ExtractTeams(cells[3]) : ExtractTeams(cells[2]);

                        if (spieltag == 10)
                        {
                            team1 = "Portugal";
                            team2 = "Frankreich";
                        }

                        if (string.IsNullOrEmpty(team1) || string.IsNullOrEmpty(team2))
                        {
                            continue;    
                        }

                        newModel.Id = 0;
                        newModel.HomeTeam = team1;
                        newModel.HomeTeamSearch = team1.Substring(0, 3).ToUpper();
                        newModel.AwayTeam = team2;
                        newModel.AwayTeamSearch = team2.Substring(0, 3).ToUpper();
                        newModel.WinOdds = (spieltag < 7) ? ExtractOdds(cells[5].InnerText) : ExtractOdds(cells[4].InnerText);
                        newModel.DrawOdds = (spieltag < 7) ? ExtractOdds(cells[6].InnerText): ExtractOdds(cells[5].InnerText);
                        newModel.LossOdds = (spieltag < 7) ? ExtractOdds(cells[7].InnerText) : ExtractOdds(cells[6].InnerText);

                        // add fix search expressions
                        {
                            newModel.AwayTeamSearch2 = newModel.AwayTeamSearch;
                            newModel.HomeTeamSearch2 = newModel.HomeTeamSearch;

                            // Niederlande
                            if (newModel.AwayTeamSearch == "HOL")
                            {
                                newModel.AwayTeamSearch2 = "NIE";
                            }
                            else if (newModel.HomeTeamSearch == "HOL")
                            {
                                newModel.HomeTeamSearch2 = "NIE";
                            }
                        }


                        oddsList.Add(newModel);
                    }
                }
            }

            return oddsList;
        }

        public List<OddsInfoModel> LoadOdds(string url, string roundTag)
        {
            HtmlWeb web = new HtmlWeb();
            var doc = web.Load(url);

            return GetOdds(doc,roundTag);
        }

        private List<OddsInfoModel> GetOdds(HtmlDocument doc, string roundTag)
        {
            int spieltag = int.Parse(roundTag);
            var tableIndex = 1;
            switch (spieltag)
            {
                case 6:
                case 7:
                    tableIndex = 1;
                    break;
            }

            var oddsList = new List<OddsInfoModel>();

            // remove all unnecessary html
            {
                ScrubHelper.ScrubHtml(doc);

                var nodes = doc.DocumentNode.SelectNodes("//comment()");
                if (nodes != null)
                {
                    foreach (HtmlNode comment in nodes)
                    {
                        comment.ParentNode.RemoveChild(comment);
                    }
                }
            }

            var htmlRows = doc.DocumentNode
                .SelectSingleNode("//body")
                .SelectSingleNode(string.Format("(//table)[{0}]", tableIndex))
                .CssSelect("tr")
                .ToList();

            if (spieltag == 7)
            {
                var htmlRowsFinal = doc.DocumentNode
                    .SelectSingleNode("//body")
                    .SelectSingleNode(string.Format("(//table)[{0}]", tableIndex+1))
                    .CssSelect("tr")
                    .ToList();

                htmlRows.AddRange(htmlRowsFinal);
            }

            var iter = htmlRows.GetEnumerator();
            if (iter.MoveNext())
            {
                // skip header row
                iter.MoveNext();
                while (iter.MoveNext())
                {
                    var newModel = new OddsInfoModel();

                    var cells = iter.Current.CssSelect("td").ToArray();

                    if (cells.Length > 0)
                    {
                        var kickoff = ExtractKickoff(cells[0].InnerText);
                        if (kickoff.HasValue == false)
                        {
                            continue;
                        }

                        var team1 = (spieltag < 7) ? ExtractTeams(cells[2]) : ExtractTeams(cells[1]);
                        var team2 = (spieltag < 7) ? ExtractTeams(cells[3]) : ExtractTeams(cells[2]);

                        if (string.IsNullOrEmpty(team1) || string.IsNullOrEmpty(team2))
                        {
                            continue;
                        }

                        newModel.Id = 0;
                        newModel.HomeTeam = team1;
                        newModel.HomeTeamSearch = team1.Substring(0, Math.Min(team1.Length,3)).ToUpper();
                        newModel.AwayTeam = team2;
                        newModel.AwayTeamSearch = team2.Substring(0, Math.Min(team1.Length, 3)).ToUpper();
                        newModel.WinOdds = (spieltag < 7) ? ExtractOdds(cells[5].InnerText) : ExtractOdds(cells[4].InnerText);
                        newModel.DrawOdds = (spieltag < 7) ? ExtractOdds(cells[6].InnerText) : ExtractOdds(cells[5].InnerText);
                        newModel.LossOdds = (spieltag < 7) ? ExtractOdds(cells[7].InnerText) : ExtractOdds(cells[6].InnerText);

                        // add fix search expressions
                        {
                            newModel.AwayTeamSearch2 = newModel.AwayTeamSearch;
                            newModel.HomeTeamSearch2 = newModel.HomeTeamSearch;

                            // Niederlande
                            if (newModel.AwayTeamSearch == "HOL")
                            {
                                newModel.AwayTeamSearch2 = "NIE";
                            }
                            else if (newModel.HomeTeamSearch == "HOL")
                            {
                                newModel.HomeTeamSearch2 = "NIE";
                            }
                        }


                        oddsList.Add(newModel);
                    }
                }
            }

            return oddsList;
        }

        private DateTime? ExtractKickoff(string kickoffStr)
        {
            var splitArr = kickoffStr.Split(new [] { ' ', '\xA0' }, StringSplitOptions.RemoveEmptyEntries);

            if (splitArr.Length > 0)
            {
                var ci = new CultureInfo("de-CH");
                DateTime kickoff;
                var dateStr = splitArr[0]+Year;
                if (DateTime.TryParse(dateStr, ci, DateTimeStyles.AllowInnerWhite, out kickoff))
                {
                    TimeSpan time;
                    if (TimeSpan.TryParse(splitArr[1], ci, out time))
                    {
                        return kickoff.Add(time);
                    }

                    return kickoff;
                }
            }

            return null;
        }

        private double? ExtractOdds(string quoteStr)
        {
            double quote;

            var splitArr = quoteStr.Split(new char[] { ' ', '\xA0','\n' }, StringSplitOptions.RemoveEmptyEntries);

            if(splitArr.Length>0)
            {
                string valStr = splitArr[0].Replace(',','.').Replace('*',' ');

                if (double.TryParse(valStr, out quote))
                {
                    return quote;
                }
                return null;
            }

            return null;

        }

        private static string ExtractTeams(HtmlNode teamNode)
        {
            var content = teamNode
                .SelectSingleNode(".//span");
            if (content != null)
            {
                return content.InnerText;
            }

            var anchorContent = teamNode
                .SelectSingleNode(".//a/text()");

            if (anchorContent != null)
            {
                return anchorContent.InnerText;
            }

            var plainText = teamNode.InnerText;

            if (plainText != null)
            {
                return plainText;
            }

            return "";
        }

    }
}
