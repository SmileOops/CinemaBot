using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom.Html;

namespace CinemaBot.Classes
{
    public static class HtmlParser
    {
        #region SearchParsingSelectors

        private const string IdSelector = "div p a.js-serp-metrika";

        #endregion

        public static async Task<string> GetFilmInfo(string filmId)
        {
            var response = string.Empty;

            var config = Configuration.Default.WithDefaultLoader();
            var address = $"https://www.kinopoisk.ru/film/{filmId}/";

            var document = await BrowsingContext.New(config).OpenAsync(address);

            var parser = new AngleSharp.Parser.Html.HtmlParser();
            var htmlDocument = parser.Parse(document.Body.InnerHtml);

            response += GetElementTexts(htmlDocument, TitleRussianSelector)[0];
            response += Environment.NewLine + Environment.NewLine;
            response += GetElementTexts(htmlDocument, TitleEnglishSelector)[0];
            response += Environment.NewLine + Environment.NewLine;
            response += GetElementTexts(htmlDocument, YearSelector)[0];
            response += Environment.NewLine + Environment.NewLine;
            response += "![poster](" + GetElementAttributes(htmlDocument, PosterSelector, "src")[0] + ")";
            response += Environment.NewLine + Environment.NewLine;
            response += GetElementTexts(htmlDocument, SynopsysSelector)[0];
            return response;
        }

        //not all results yet
        public static async Task<List<string>> GetFilmIdsInfoBySearching(string userQuery)
        {
            var url = $"https://www.kinopoisk.ru/index.php?first=no&what=&kp_query={EncodeCyrillicString(userQuery)}";

            var config = Configuration.Default.WithDefaultLoader();

            var document = await BrowsingContext.New(config).OpenAsync(url);

            var parser = new AngleSharp.Parser.Html.HtmlParser();
            var htmlDocument = parser.Parse(document.Body.InnerHtml);

            return GetElementAttributes(htmlDocument, IdSelector, "data-id").Distinct().ToList();
        }


        private static List<string> GetElementTexts(IHtmlDocument parsedHtml, string selector)
        {
            var cells = parsedHtml.QuerySelectorAll(selector);
            var titlesList = new List<string>(cells.Select(m => m.TextContent));

            return titlesList;
        }

        private static List<string> GetElementAttributes(IHtmlDocument parsedHtml, string selector, string attribute)
        {
            var cells = parsedHtml.QuerySelectorAll(selector);
            var titlesList = new List<string>(cells.Select(m => m.GetAttribute(attribute)));

            return titlesList;
        }

        private static string EncodeCyrillicString(string cyrillicSrc)
        {
            return HttpUtility.UrlEncode(cyrillicSrc, Encoding.GetEncoding(1251));
        }

        #region FilmParsingSelectors

        private const string TitleRussianSelector = "h1.moviename-big";
        private const string TitleEnglishSelector = "[itemprop=\"alternativeHeadline\"]";

        private const string YearSelector =
            "table.info tr td div[style=\"position: relative\"] a[href^=\"/lists\"][title]";

        private const string PosterSelector = "a.popupBigImage img";
        private const string SynopsysSelector = "div.film-synopsys";

        #endregion FilmParsingSelectors
    }
}