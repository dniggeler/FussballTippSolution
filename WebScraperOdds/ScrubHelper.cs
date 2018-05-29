using System;
using System.Globalization;
using HtmlAgilityPack;

namespace WebScraperOdds
{
    public class ScrubHelper
    {
        public static void ScrubHtml(HtmlDocument doc)
        {
            //Remove potentially harmful elements
            HtmlNodeCollection nc = doc.DocumentNode.SelectNodes("//script|//link|//iframe|//frameset|//frame|//applet|//object");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    node.ParentNode.RemoveChild(node, false);

                }
            }

            //remove hrefs to java/j/vbscript URLs
            nc = doc.DocumentNode.SelectNodes("//a[starts-with(@href, 'javascript')]|//a[starts-with(@href, 'jscript')]|//a[starts-with(@href, 'vbscript')]");
            if (nc != null)
            {

                foreach (HtmlNode node in nc)
                {
                    node.SetAttributeValue("href", "protected");
                }
            }



            //remove img with refs to java/j/vbscript URLs
            nc = doc.DocumentNode.SelectNodes("//img[starts-with(@src, 'javascript')]|//img[starts-with(@src, 'jscript')]|//img[starts-with(@src, 'vbscript')]");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    node.SetAttributeValue("src", "protected");
                }
            }

            //remove on<Event> handlers from all tags
            nc = doc.DocumentNode.SelectNodes("//*[@onclick or @onmouseover or @onfocus or @onblur or @onmouseout or @ondoubleclick or @onload or @onunload]");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    node.Attributes.Remove("onFocus");
                    node.Attributes.Remove("onBlur");
                    node.Attributes.Remove("onClick");
                    node.Attributes.Remove("onMouseOver");
                    node.Attributes.Remove("onMouseOut");
                    node.Attributes.Remove("onDoubleClick");
                    node.Attributes.Remove("onLoad");
                    node.Attributes.Remove("onUnload");
                }
            }

        }

        public static double? ConvertToDouble(HtmlNode tdNode)
        {
            var numberStr = tdNode.InnerText.Replace(',', '.').Trim();

            if (string.IsNullOrEmpty(numberStr))
            {
                return null;
            }

            try
            {
                var tmpQuote = Convert.ToDouble(numberStr, CultureInfo.InvariantCulture);
                if (tmpQuote > 200.0)
                {
                    tmpQuote = tmpQuote / 100.0;
                }
                return tmpQuote;
            }
            catch (Exception)
            {
                string[] subStrArray = numberStr.Split('.');

                if (subStrArray.Length > 2)
                {
                    string newNumberStr = subStrArray[0] + subStrArray[1] + "." + subStrArray[2];

                    return Convert.ToDouble(newNumberStr, CultureInfo.InvariantCulture);
                }

                string nbr = numberStr.Replace(' ', '.');
                return Convert.ToDouble(nbr, CultureInfo.InvariantCulture);
            }
        }
    }
}
