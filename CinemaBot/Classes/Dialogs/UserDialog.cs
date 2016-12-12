using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace CinemaBot.Classes.Dialogs
{
    [Serializable]
    public class UserDialog : IDialog<object>
    {
        protected string CurrentCommand;
        protected HtmlParser HtmlParser = new HtmlParser();
        protected List<string> Ids;
        protected ReplyFormatter ReplyFormatter = new ReplyFormatter();

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var activity = (Activity) await argument;
            if (string.IsNullOrEmpty(CurrentCommand))
            {
                switch (activity.Text)
                {
                    case "/find":
                        await context.PostAsync("Введи фильм");
                        context.Wait(ContinueExecuteFindCommand);
                        break;

                    case "/findSimilar":
                        await context.PostAsync("Не разгоняйся, тестер бля, этого еще нет");
                        context.Wait(MessageReceivedAsync);
                        break;

                    case "/start":
                        await context.PostAsync(ReplyFormatter.GetHelloReply());
                        context.Wait(MessageReceivedAsync);
                        break;

                    case "/help":
                        await context.PostAsync("Не разгоняйся, тестер бля, этого еще нет");
                        context.Wait(MessageReceivedAsync);
                        break;

                    default:
                        await context.PostAsync("Неверная команда!");
                        context.Wait(MessageReceivedAsync);
                        break;
                }
            }
            else
            {
                switch (CurrentCommand)
                {
                    case "/find":
                        break;

                    case "/findSimilar":
                        break;

                    case "/start":
                        break;

                    case "/help":
                        break;
                }
            }

            //context.Wait(MessageReceivedAsync);
        }

        private async Task ContinueExecuteFindCommand(IDialogContext context, IAwaitable<IMessageActivity> message)
        {
            var activity = (Activity) await message;

            if (Ids == null)
            {
                Ids = await HtmlParser.GetFilmsIdsFromSearchPage(activity.Text);

                await
                    context.PostAsync(ReplyFormatter.GetFilmInfoReply(
                        await HtmlParser.GetFilmInfo(Ids[0])));
            }

            PromptDialog.Confirm(context, AnswerDialog, "Хотите другой фильм?", "Нажмите \"Да\" или \"Нет\"",
                promptStyle: PromptStyle.Keyboard);
        }

        public async Task AnswerDialog(IDialogContext context, IAwaitable<bool> argument)
        {
            var isConfirmed = await argument;

            if (isConfirmed)
            {
                Ids.RemoveAt(0);

                await
                    context.PostAsync(ReplyFormatter.GetFilmInfoReply(
                        await HtmlParser.GetFilmInfo(Ids[0])));

                PromptDialog.Confirm(context, AnswerDialog, "Хотите другой фильм?", "Нажмите \"Да\" или \"Нет\"",
                    promptStyle: PromptStyle.Keyboard);
            }
            else
            {
                await context.PostAsync("Приятного просмотра :)");
                CurrentCommand = string.Empty;
                Ids = null;
                context.Wait(MessageReceivedAsync);
            }
        }
    }
}