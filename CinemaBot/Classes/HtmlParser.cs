using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom.Html;

namespace CinemaBot.Classes
{
    public static class HtmlParser
    {
        private const string TitleRussianSelector = "h1.moviename-big";
        private const string TitleEnglishSelector = "[itemprop=\"alternativeHeadline\"]";

        private const string YearSelector =
            "table.info tr td div[style=\"position: relative\"] a[href^=\"/lists\"][title]";

        private const string PosterSelector = "a.popupBigImage img";
        private const string SynopsysSelector = "div.film-synopsys";

        public static async Task<string> GetFilmInfo(string filmId)
        {
            var response = string.Empty;

            var config = Configuration.Default.WithDefaultLoader();
            var address = $"https://www.kinopoisk.ru/film/{filmId}/";

            var document = await BrowsingContext.New(config).OpenAsync(address);

            var parser = new AngleSharp.Parser.Html.HtmlParser();
            var htmlDocument = parser.Parse(document.Body.InnerHtml);

            response += GetElementText(htmlDocument, TitleRussianSelector);
            response += Environment.NewLine + Environment.NewLine;
            response += GetElementText(htmlDocument, TitleEnglishSelector);
            response += Environment.NewLine + Environment.NewLine;
            response += GetElementText(htmlDocument, YearSelector);
            response += Environment.NewLine + Environment.NewLine;
            response += GetElementAttribute(htmlDocument, PosterSelector, "src");
            response += Environment.NewLine + Environment.NewLine;
            response += GetElementText(htmlDocument, SynopsysSelector);
            return response;
        }

        //refactoring required
        private static string GetElementText(IHtmlDocument parsedHtml, string selector)
        {
            var cells = parsedHtml.QuerySelectorAll(selector);
            var titlesList = new List<string>(cells.Select(m => m.TextContent));

            return titlesList.Count != 1 ? "error" : titlesList[0];
        }

        private static string GetElementAttribute(IHtmlDocument parsedHtml, string selector, string attribute)
        {
            var cells = parsedHtml.QuerySelectorAll(selector);
            var titlesList = new List<string>(cells.Select(m => m.GetAttribute(attribute)));

            return titlesList.Count != 1 ? "error" : titlesList[0];
        }
    }
}