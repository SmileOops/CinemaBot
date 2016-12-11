using System;
using Microsoft.Bot.Connector;

namespace CinemaBot.Classes
{
    internal static class ReplyFormatter
    {
        private const string searchError = "Я не могу найти ничего похожего :(";

        internal static Activity GetFilmInfoReply(Activity activity, FilmInfo filmInfo)
        {
            var reply = activity.CreateReply();
            if (filmInfo.TextInfo != string.Empty && filmInfo.PosterUrl != string.Empty)
            {
                reply.Text = filmInfo.TextInfo;
                reply.Attachments.Add(new Attachment("image/jpg", filmInfo.PosterUrl));
            }
            else
            {
                reply.Text = searchError;
            }
            return reply;
        }
    }
}