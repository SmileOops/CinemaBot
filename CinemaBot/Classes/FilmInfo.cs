namespace CinemaBot.Classes
{
    internal sealed class FilmInfo
    {
        internal FilmInfo(string textInfo, string posterUrl)
        {
            TextInfo = textInfo;
            PosterUrl = posterUrl;
        }

        public string TextInfo { get; }
        public string PosterUrl { get; }
    }
}