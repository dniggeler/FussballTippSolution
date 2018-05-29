using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using HtmlAgilityPack;
using OddsScraper.Contract;
using OddsScraper.Contract.Model;

namespace WebScraperOdds
{
    public class WettfreundeOddsBuLiScraper : IOddsScraper
    {
        public List<OddsInfoModel> GetOdds(string oddsAsHtmlStr, string roundTag)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(oddsAsHtmlStr);

            return GetOdds(doc,roundTag);
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

            var oddsList = new List<OddsInfoModel>();
            {
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

                HtmlNode spieltagNode = GetSpieltagHtml(doc, spieltag);
                if (spieltagNode == null) throw new ApplicationException("Spieltag nicht gefunden.");

                HtmlNode oddsTableNode = spieltagNode
                                        .ParentNode
                                        .ParentNode
                                        .ParentNode
                                        .NextSibling;

                // Odds
                {

                    HtmlNode trNode = oddsTableNode.NextSibling.NextSibling.NextSibling;

                    for (int ii = 3; ii < 12; ii++)
                    {
                        Debug.WriteLine(trNode.InnerHtml);
                        var model = new OddsInfoModel();

                        int kk = 0;
                        foreach (var tdNode in trNode.SelectNodes("./td"))
                        {
                            switch (kk)
                            {
                                case 1:
                                    model.HomeTeam = tdNode.InnerText;
                                    model.HomeTeamSearch = model.HomeTeam.ToUpper();
                                    break;
                                case 2:
                                    model.AwayTeam = tdNode.InnerText;
                                    model.AwayTeamSearch = model.AwayTeam.ToUpper();
                                    break;
                                case 4:
                                {
                                    var oddsNode = tdNode.SelectSingleNode(".//strong");
                                    model.WinOdds = ScrubHelper.ConvertToDouble(oddsNode);
                                    
                                }
                                    break;
                                case 5:
                                {
                                    var oddsNode = tdNode.SelectSingleNode(".//strong");
                                    model.DrawOdds = ScrubHelper.ConvertToDouble(oddsNode);
                                    
                                }
                                    break;
                                case 6:
                                {
                                    var oddsNode = tdNode.SelectSingleNode(".//strong");
                                    model.LossOdds = ScrubHelper.ConvertToDouble(oddsNode);
                                    
                                }
                                    break;
                            }

                            kk++;
                        }

                        trNode = trNode.NextSibling.NextSibling;

                        oddsList.Add(model);
                    }
                }

            }

            return oddsList;
        }

        private static HtmlNode GetSpieltagHtml(HtmlDocument doc, int spieltag)
        {
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//table/tbody/tr/td/span/strong"))
            {
                string cleanStr = node.InnerText.Trim(new char[] { '\n', ' ' });

                if (cleanStr.ToUpper().StartsWith(string.Format("{0}. SPIELTAG ", spieltag)))
                {
                    return node;
                }
            }

            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//table/tbody/tr/td/span/strong"))
            {
                string cleanStr = node.InnerText.Trim(new char[] { '\n', ' ' });

                if (cleanStr.ToUpper().Equals("SPIELTAG " + spieltag))
                {
                    return node;
                }
            }

            // fall back, none found so far
            foreach (HtmlNode node in doc.DocumentNode.SelectNodes("//table[3]/tbody/tr/td/span/strong"))
            {
                string cleanStr = node.InnerText.Trim(new char[] { '\n', ' ' });

                if (cleanStr.ToUpper().Equals("SPIELTAG " + spieltag))
                {
                    return node;
                }
            }

            return null;
        }
    }
}
