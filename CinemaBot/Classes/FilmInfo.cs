namespace CinemaBot.Classes
{
    public sealed class FilmInfo
    {
        public FilmInfo(string textInfo, string posterUrl)
        {
            TextInfo = textInfo;
            PosterUrl = posterUrl;
        }

        public string TextInfo { get; }
        public string PosterUrl { get; }
    }
}