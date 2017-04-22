using CthulhuDiceBot.Factories;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CthulhuDiceBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // return our reply to the user
            await context.PostAsync(CommandFactory.Instance.GetCommand(activity).Invoke(activity));

            context.Wait(MessageReceivedAsync);
        }
    }
}
