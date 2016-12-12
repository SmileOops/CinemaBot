using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using AngleSharp;
using AngleSharp.Dom.Html;
using CinemaBot.Enums;

namespace CinemaBot.Classes
{
    [Serializable]
    public class HtmlParser
    {
        //internal static async Task<FilmInfo> GetSimilarFilmInfo(string userQuery)
        //{
        //    var ids = await GetFilmsIdsFromSearchPage(userQuery);

        //    var similarIds = await GetFilmsIdsFromSimilarsPage(ids[0]);
        //    if (similarIds.Count != 0)
        //    {
        //        return await GetFilmInfo(similarIds[0]);
        //    }
        //    return new FilmInfo(string.Empty, string.Empty);
        //}

        //internal static async Task<FilmInfo> GetSaughtforFilmInfo(string userQuery)
        //{
        //    var ids = await GetFilmsIdsFromSearchPage(userQuery);

        //    if (ids.Count != 0)
        //    {
        //        return await GetFilmInfo(ids[0]);
        //    }

        //    return new FilmInfo(string.Empty, string.Empty);
        //}

        public async Task<FilmInfo> GetFilmInfo(string filmId)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var address = $"https://www.kinopoisk.ru/film/{filmId}";
            var document = await BrowsingContext.New(config).OpenAsync(address);
            var parser = new AngleSharp.Parser.Html.HtmlParser();
            var htmlDocument = parser.Parse(document.Body.InnerHtml);

            var response = new StringBuilder();

            response.Append($"**{GetElementTexts(htmlDocument, TitleRussianSelector)[0]}** ");
            response.Append($"({GetElementTexts(htmlDocument, YearSelector)[0]})");
            response.Append(Environment.NewLine);
            response.Append(Environment.NewLine);
            response.Append(GetElementTexts(htmlDocument, TitleEnglishSelector)[0]);
            response.Append(Environment.NewLine);
            response.Append(Environment.NewLine);
            response.Append($"**Рейтинг КиноПоиска: {GetElementTexts(htmlDocument, RatingSelector)[0]}**");
            response.Append(Environment.NewLine);
            response.Append(Environment.NewLine);

            var trailerUrl = await GetTrailerUrl(htmlDocument);
            if (!string.IsNullOrEmpty(trailerUrl))
            {
                response.Append($"[Трейлер]({trailerUrl})");
                response.Append(Environment.NewLine);
                response.Append(Environment.NewLine);
            }

            response.Append($"**Режиссер**: {GetElementTexts(htmlDocument, DirectorSelector)[0]}");
            response.Append(Environment.NewLine);
            response.Append(Environment.NewLine);
            response.Append(GetActorsString(htmlDocument));
            response.Append(Environment.NewLine);
            response.Append(Environment.NewLine);
            response.Append(GetElementTexts(htmlDocument, SynopsysSelector)[0]);


            var possiblePostersUrl = GetElementAttributes(htmlDocument, PosterSelector, "src");
            var posterUrl = possiblePostersUrl.Count > 0 ? possiblePostersUrl[0] : string.Empty;
            return new FilmInfo(response.ToString(), posterUrl);
        }

        private async Task<List<string>> GetFilmsIdsFromTopByGenrePage(GenresFromTop genre)
        {
            var url = TopByGenreUrl + (int) genre;

            var htmlDocument = await GetParsedPageByUrl(url);

            var attributes = GetElementAttributes(htmlDocument, TopByGenreIdSelector, "mid").ToList();

            return attributes;
        }

        //not all results yet
        public async Task<List<string>> GetFilmsIdsFromSearchPage(string userQuery)
        {
            var url = SearchUrl + EncodeCyrillicString(userQuery);

            var htmlDocument = await GetParsedPageByUrl(url);

            var ids = GetElementAttributes(htmlDocument, IdSelector, "data-id");

            if (ids.Count != 0)
            {
                return ids.Distinct().ToList();
            }
            return new List<string>();
        }

        private async Task<List<string>> GetFilmsIdsFromSimilarsPage(string filmId)
        {
            var url = FilmUrl + $"{filmId}/like";

            var htmlDocument = await GetParsedPageByUrl(url);

            var attributes = GetElementAttributes(htmlDocument, SimilarIdSelector, "href");

            return attributes.Select(attribute => attribute.Replace("/film/", "")).ToList();
        }

        private async Task<IHtmlDocument> GetParsedPageByUrl(string url)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var document = await BrowsingContext.New(config).OpenAsync(url);
            var parser = new AngleSharp.Parser.Html.HtmlParser();

            return parser.Parse(document.Body.InnerHtml);
        }

        //refactor this shit
        private async Task<string> GetTrailerUrl(IHtmlDocument parsedHtmlDocument)
        {
            var trailerPageUrlPart = GetElementAttributes(parsedHtmlDocument, TrailerPageSelector, "href");

            if (trailerPageUrlPart.Count != 0)
            {
                var trailerPageUrl =
                    $"https://kinopoisk.ru{trailerPageUrlPart[0]}";

                var config = Configuration.Default.WithDefaultLoader();
                var trailerPageDocument = await BrowsingContext.New(config).OpenAsync(trailerPageUrl);
                var parser = new AngleSharp.Parser.Html.HtmlParser();
                var htmlDocument = parser.Parse(trailerPageDocument.Body.InnerHtml);

                var urls = GetElementAttributes(htmlDocument, TrailerVideoSelector, "href");
                var correctUrls = urls.Where(url => url.Contains(".mp4")).ToList();

                string trailerUrl;

                if (correctUrls.Count != 0)
                {
                    trailerUrl = correctUrls.Count >= 3 ? correctUrls[2] : correctUrls[correctUrls.Count - 1];
                }
                else
                {
                    return trailerPageUrl;
                }

                return trailerUrl.Substring(trailerUrl.IndexOf("https", StringComparison.Ordinal));
            }
            return string.Empty;
        }

        private List<string> GetElementTexts(IHtmlDocument parsedHtml, string selector)
        {
            var cells = parsedHtml.QuerySelectorAll(selector);
            var titlesList = new List<string>(cells.Select(m => m.TextContent));

            return titlesList;
        }

        private List<string> GetElementAttributes(IHtmlDocument parsedHtml, string selector, string attribute)
        {
            var cells = parsedHtml.QuerySelectorAll(selector);
            var titlesList = new List<string>(cells.Select(m => m.GetAttribute(attribute)));

            return titlesList;
        }

        private string EncodeCyrillicString(string cyrillicSrc)
        {
            return HttpUtility.UrlEncode(cyrillicSrc, Encoding.GetEncoding(1251));
        }

        private string GetActorsString(IHtmlDocument parsedHtml)
        {
            var actors = GetElementTexts(parsedHtml, ActorsSelector);

            var sb = new StringBuilder();

            sb.Append("**В главных ролях**: ");

            if (actors.Count >= 4)
            {
                sb.Append($"{actors[0]}, ");
                sb.Append($"{actors[1]}, ");
                sb.Append($"{actors[2]}, ");
                sb.Append($"{actors[3]}...");
            }
            else
            {
                for (var i = 0; i < actors.Count; i++)
                {
                    sb.Append(i != actors.Count - 1 ? $"{actors[i]}, " : $"{actors[i]}.");
                }
            }

            return sb.ToString();
        }

        #region FilmParsingSelectors

        private const string TitleRussianSelector = "h1.moviename-big";
        private const string TitleEnglishSelector = "[itemprop=\"alternativeHeadline\"]";

        private const string YearSelector =
            "table.info tr td div[style=\"position: relative\"] a[href^=\"/lists\"][title]";

        private const string PosterSelector = "a.popupBigImage img";
        private const string SynopsysSelector = "div.film-synopsys";
        private const string TrailerPageSelector = "a.all[href*=\"/video/\"]";
        private const string TrailerVideoSelector = "a.continue";
        private const string RatingSelector = "span.rating_ball";
        private const string DirectorSelector = "td[itemprop=\"director\"] a";
        private const string ActorsSelector = "div#actorList ul li";

        #endregion FilmParsingSelectors

        #region SearchParsingSelectors

        private const string IdSelector = "div p a.js-serp-metrika";
        private const string SimilarIdSelector = "a.i_orig";
        private const string TopByGenreIdSelector = "div.MyKP_Folder_Select";

        #endregion

        #region urls

        private const string SearchUrl = "https://www.kinopoisk.ru/index.php?first=no&what=&kp_query=";
        private const string FilmUrl = "https://www.kinopoisk.ru/film/";
        private const string TopByGenreUrl = "https://www.kinopoisk.ru/top/id_genre/";

        #endregion urls
    }
}