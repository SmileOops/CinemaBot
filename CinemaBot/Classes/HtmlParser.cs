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
        public async Task<FilmInfo> GetFilmInfo(string filmId)
        {
            var config = Configuration.Default.WithDefaultLoader();
            var address = $"https://www.kinopoisk.ru/film/{filmId}";
            var document = await BrowsingContext.New(config).OpenAsync(address);
            var parser = new AngleSharp.Parser.Html.HtmlParser();
            var htmlDocument = parser.Parse(document.Body.InnerHtml);

            var response = new StringBuilder();

            List<string> possibleInfoParts;

            if (TryGetElementTexts(htmlDocument, TitleRussianSelector, out possibleInfoParts))
                response.Append($"**{possibleInfoParts[0]}** ");
            if (TryGetElementTexts(htmlDocument, YearSelector, out possibleInfoParts))
                response.Append($"({possibleInfoParts[0]})");
            response.Append(Environment.NewLine);
            response.Append(Environment.NewLine);
            if (TryGetElementTexts(htmlDocument, TitleEnglishSelector, out possibleInfoParts))
                response.Append(possibleInfoParts[0]);
            response.Append(Environment.NewLine);
            response.Append(Environment.NewLine);
            if (TryGetElementTexts(htmlDocument, RatingSelector, out possibleInfoParts))
                response.Append($"**Рейтинг КиноПоиска: {possibleInfoParts[0]}**");
            response.Append(Environment.NewLine);
            response.Append(Environment.NewLine);

            //var trailerUrl = await GetTrailerUrl(htmlDocument);
            //if (!string.IsNullOrEmpty(trailerUrl))
            //{
            //    response.Append($"[Трейлер]({trailerUrl})");
            //    response.Append(Environment.NewLine);
            //    response.Append(Environment.NewLine);
            //}
            if (TryGetElementTexts(htmlDocument, DirectorSelector, out possibleInfoParts))
                response.Append($"**Режиссер**: {possibleInfoParts[0]}");
            response.Append(Environment.NewLine);
            response.Append(Environment.NewLine);
            response.Append(GetActorsString(htmlDocument));
            response.Append(Environment.NewLine);
            response.Append(Environment.NewLine);

            if (TryGetElementTexts(htmlDocument, SynopsysSelector, out possibleInfoParts))
                response.Append(possibleInfoParts[0]);

            var posterUrl = string.Empty;

            if (TryGetElementAttributes(htmlDocument, PosterSelector, "src", out possibleInfoParts))
                posterUrl = possibleInfoParts[0];

            return new FilmInfo(response.ToString(), posterUrl);
        }

        public async Task<List<string>> GetFilmsIdsFromTopByGenrePage(GenresFromTop genre)
        {
            var url = TopByGenreUrl + (int) genre;

            var htmlDocument = await GetParsedPageByUrl(url);

            List<string> attributes;

            TryGetElementAttributes(htmlDocument, TopByGenreIdSelector, "mid", out attributes);

            return attributes;
        }

        //not all results yet
        public async Task<List<string>> GetFilmsIdsFromSearchPage(string userQuery)
        {
            var url = SearchUrl + EncodeCyrillicString(userQuery);

            var htmlDocument = await GetParsedPageByUrl(url);

            List<string> ids;

            if (TryGetElementAttributes(htmlDocument, FilmIdSelector, "data-id", out ids))
            {
                return ids.Distinct().ToList();
            }
            return new List<string>();
        }

        public async Task<List<string>> GetFilmsIdsFromSimilarsPage(string filmId)
        {
            var url = FilmUrl + $"{filmId}/like";

            var htmlDocument = await GetParsedPageByUrl(url);

            List<string> attributes;

            TryGetElementAttributes(htmlDocument, SimilarIdSelector, "href", out attributes);

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
            List<string> trailerPageUrlParts;

            if (TryGetElementAttributes(parsedHtmlDocument, TrailerPageSelector, "href", out trailerPageUrlParts))
            {
                var trailerPageUrl =
                    $"https://kinopoisk.ru{trailerPageUrlParts[0]}";

                var config = Configuration.Default.WithDefaultLoader();
                var trailerPageDocument = await BrowsingContext.New(config).OpenAsync(trailerPageUrl);
                var parser = new AngleSharp.Parser.Html.HtmlParser();
                var htmlDocument = parser.Parse(trailerPageDocument.Body.InnerHtml);


                List<string> urls;
                if (TryGetElementAttributes(htmlDocument, TrailerVideoSelector, "href", out urls))
                {
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
            }
            return string.Empty;
        }

        private bool TryGetElementTexts(IHtmlDocument parsedHtml, string selector, out List<string> result)
        {
            var cells = parsedHtml.QuerySelectorAll(selector);
            result = new List<string>(cells.Select(m => m.TextContent));

            return result.Count > 0;
        }

        private bool TryGetElementAttributes(IHtmlDocument parsedHtml, string selector, string attribute,
            out List<string> result)
        {
            var cells = parsedHtml.QuerySelectorAll(selector);
            result = new List<string>(cells.Select(m => m.GetAttribute(attribute)));

            return result.Count > 0;
        }

        private string EncodeCyrillicString(string cyrillicSrc)
        {
            return HttpUtility.UrlEncode(cyrillicSrc, Encoding.GetEncoding(1251));
        }

        private string GetActorsString(IHtmlDocument parsedHtml)
        {
            List<string> actors;

            if (TryGetElementTexts(parsedHtml, ActorsSelector, out actors))
            {
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

            return String.Empty;
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

        private const string FilmIdSelector = "div p a.js-serp-metrika[data-type=\"film\"]";
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