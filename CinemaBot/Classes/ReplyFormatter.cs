using System;
using System.Text;

namespace CinemaBot.Classes
{
    [Serializable]
    public class ReplyFormatter
    {
        #region errorPhrases   

        private const string SearchError = "Я не могу найти ничего похожего :(";

        #endregion

        public string GetFilmInfoReply(FilmInfo filmInfo)
        {
            var sb = new StringBuilder();
            if (filmInfo.TextInfo != string.Empty)
            {
                sb.Append(filmInfo.TextInfo);
                if (filmInfo.PosterUrl != string.Empty)
                {
                    sb.Append(Environment.NewLine);
                    sb.Append(Environment.NewLine);
                    sb.Append($"![poster]({filmInfo.PosterUrl})");
                }
            }
            else
            {
                sb.Append(SearchError);
            }
            return sb.ToString();
        }

        public string GetHelloReply()
        {
            var replySb = new StringBuilder();
            replySb.Append(Hello);
            replySb.Append(Environment.NewLine);
            replySb.Append(Environment.NewLine);
            replySb.Append(Skills);

            return replySb.ToString();
        }

        public string GetHelpReply()
        {
            var replySb = new StringBuilder();
            replySb.Append(FindCommand);
            replySb.Append(Environment.NewLine);
            replySb.Append(Environment.NewLine);
            replySb.Append(FindSimilarCommand);
            replySb.Append(Environment.NewLine);
            replySb.Append(Environment.NewLine);
            replySb.Append(FindTopByGenreCommand);

            return replySb.ToString();
        }

        #region phrases

        private const string Hello = "Привет! Я буду искать для тебя хорошие фильмы.";

        private const string Skills =
            "Чтобы узнать, что я умею, ты можешь ввести \"/help\" или нажать соответствующую кнопку.";

        private const string FindCommand = "/find - найдет фильмы с указанным тобой названием";

        private const string FindSimilarCommand =
            "/findSimilar - найдет фильмы, похожие на фильм с указанным названием";

        private const string FindTopByGenreCommand = "/findTopByGenre - найдет лучшие фильмы указанного жанра";

        #endregion
    }
}