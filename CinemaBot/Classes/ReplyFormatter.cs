using Microsoft.Bot.Connector;

namespace CinemaBot.Classes
{
    internal static class ReplyFormatter
    {
        internal static Activity GetFilmInfoReply(Activity activity, FilmInfo filmInfo)
        {
            var reply = activity.CreateReply(filmInfo.TextInfo);
            reply.Attachments.Add(new Attachment("image/jpg", filmInfo.PosterUrl));

            return reply;
        }
    }
}