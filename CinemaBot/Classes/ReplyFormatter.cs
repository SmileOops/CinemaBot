using System;
using System.Text;

namespace CinemaBot.Classes
{//static removal
    [Serializable]
    public class ReplyFormatter
    {
        #region errorPhrases   
        private const string searchError = "Я не могу найти ничего похожего :(";
        #endregion

        #region phrases

        private const string hello = "Привет! Я буду искать для тебя хорошие фильмы.";

        private const string skills =
            "Чтобы узнать, что я умею, ты можешь ввести \"/help\" или нажать соответствующую кнопку.";
        #endregion

        public string GetFilmInfoReply( FilmInfo filmInfo)
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
                sb.Append(searchError);
            }
            return sb.ToString();
        }

        public string GetHelloReply()
        {
            var replySb = new StringBuilder();
            replySb.Append(hello);
            replySb.Append(Environment.NewLine);
            replySb.Append(Environment.NewLine);
            replySb.Append(skills);

            return replySb.ToString();
        }


    }
}