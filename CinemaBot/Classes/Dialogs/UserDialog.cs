using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CinemaBot.Enums;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace CinemaBot.Classes.Dialogs
{
    [Serializable]
    public class UserDialog : IDialog<object>
    {
        protected string CurrentCommand;
        protected Dictionary<string, GenresFromTop> Genres;
        protected HtmlParser HtmlParser = new HtmlParser();
        protected List<string> Ids;
        protected ReplyFormatter ReplyFormatter = new ReplyFormatter();

        public UserDialog()
        {
            Genres = GetGenresDictionary();
        }

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = (Activity) await argument;

            switch (activity.Text)
            {
                case "/find":
                    await context.PostAsync("Введите название фильма:");
                    context.Wait(ContinueExecuteFindCommand);
                    break;

                case "/findsimilar":
                    await context.PostAsync("Введите название фильма:");
                    context.Wait(ContinueExecuteFindSimilarCommand);
                    break;

                case "/findtopbygenre":
                    PromptDialog.Choice(context, ChooseGenreDialog, Genres.Keys, "Выберите жанр:",
                        "Нажмите на одну из кнопок", promptStyle: PromptStyle.Keyboard);
                    break;

                case "/start":
                    await context.PostAsync(ReplyFormatter.GetHelloReply());
                    context.Wait(MessageReceivedAsync);
                    break;

                case "/help":
                    await context.PostAsync(ReplyFormatter.GetHelpReply());
                    context.Wait(MessageReceivedAsync);
                    break;

                default:
                    await context.PostAsync("Неверная команда!");
                    context.Wait(MessageReceivedAsync);
                    break;
            }
        }

        private async Task ContinueExecuteFindCommand(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            var activity = (Activity) await message;

            if (Ids == null)
            {
                Ids = await HtmlParser.GetFilmsIdsFromSearchPage(activity.Text);

                if (Ids.Count > 0)
                {
                    await
                        context.PostAsync(ReplyFormatter.GetFilmInfoReply(
                            await HtmlParser.GetFilmInfo(Ids[0])));
                }
            }

            if (Ids.Count > 0)
            {
                PromptDialog.Confirm(context, ChooseNextFilmDialog, "Хотите другой фильм?", "Нажмите \"Да\" или \"Нет\"",
                    promptStyle: PromptStyle.Keyboard);
            }
            else
            {
                await context.PostAsync("Не могу ничего найти :(");
                Ids = null;
                context.Wait(MessageReceivedAsync);
            }
        }

        private async Task ContinueExecuteFindSimilarCommand(IDialogContext context,
            IAwaitable<IMessageActivity> message)
        {
            var activity = (Activity) await message;

            bool isRootFilmFound = true;
            if (Ids == null)
            {
                var rootFilmIds = await HtmlParser.GetFilmsIdsFromSearchPage(activity.Text);
                //check root film required
                if (rootFilmIds.Count > 0)
                {
                    Ids = await HtmlParser.GetFilmsIdsFromSimilarsPage(rootFilmIds[0]);

                    if (Ids.Count > 0)
                    {
                        await
                            context.PostAsync(ReplyFormatter.GetFilmInfoReply(
                                await HtmlParser.GetFilmInfo(Ids[0])));
                    }
                }
                else
                {
                    isRootFilmFound = false;
                }
            }

            if (isRootFilmFound && Ids.Count > 0)
            {
                PromptDialog.Confirm(context, ChooseNextFilmDialog, "Хотите другой фильм?", "Нажмите \"Да\" или \"Нет\"", 
                    promptStyle: PromptStyle.Keyboard);
            }
            else
            {
                await context.PostAsync("Не могу ничего найти :(");
                Ids = null;
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task ChooseNextFilmDialog(IDialogContext context, IAwaitable<bool> argument)
        {
            var isConfirmed = await argument;

            if (isConfirmed)
            {
                if (Ids.Count > 1)
                {
                    Ids.RemoveAt(0);

                    await
                        context.PostAsync(ReplyFormatter.GetFilmInfoReply(
                            await HtmlParser.GetFilmInfo(Ids[0])));

                    PromptDialog.Confirm(context, ChooseNextFilmDialog, "Хотите другой фильм?",
                        "Нажмите \"Да\" или \"Нет\"",
                        promptStyle: PromptStyle.Keyboard);
                }
                else
                {
                    await context.PostAsync("К сожалению я больше не нашел :(");
                    CurrentCommand = string.Empty;
                    Ids.Clear();
                    Ids = null;
                    context.Wait(MessageReceivedAsync);
                }
            }
            else
            {
                await context.PostAsync("Приятного просмотра :)");
                CurrentCommand = string.Empty;
                Ids.Clear();
                Ids = null;
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task ChooseGenreDialog(IDialogContext context,
            IAwaitable<string> argument)
        {
            var genreString = await argument;

            if (Ids == null)
            {
                GenresFromTop genreId;
                if (Genres.TryGetValue(genreString, out genreId))
                {
                    Ids = await HtmlParser.GetFilmsIdsFromTopByGenrePage(genreId);

                    await context.PostAsync(ReplyFormatter.GetFilmInfoReply(await HtmlParser.GetFilmInfo(Ids[0])));

                    PromptDialog.Confirm(context, ChooseNextFilmDialog, "Хотите другой фильм?",
                        "Нажмите \"Да\" или \"Нет\"",
                        promptStyle: PromptStyle.Keyboard);
                }
            }
            else
            {
                await context.PostAsync("(((");
                context.Wait(MessageReceivedAsync);
            }
        }

        public Dictionary<string, GenresFromTop> GetGenresDictionary()
        {
            var genres = new Dictionary<string, GenresFromTop>
            {
                {"Ужасы", GenresFromTop.Horror},
                {"Фантастика", GenresFromTop.Fiction},
                {"Боевики", GenresFromTop.Action},
                {"Триллеры", GenresFromTop.Thriller},
                {"Фэнтэзи", GenresFromTop.Fantasy},
                {"Комедии", GenresFromTop.Comedy},
                {"Мелодрамы", GenresFromTop.Melodrama},
                {"Драмы", GenresFromTop.Drama},
                {"Мюзиклы", GenresFromTop.Musical},
                {"Приключенческие", GenresFromTop.Adventure},
                {"Семейные", GenresFromTop.Family},
                {"Документальные", GenresFromTop.Documentary},
                {"Вестерны", GenresFromTop.Western},
                {"Мультфильмы", GenresFromTop.Cartoon}
            };

            return genres;
        }
    }
}