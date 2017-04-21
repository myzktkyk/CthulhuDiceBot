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
            await context.PostAsync(CommandFactory(activity)?.Invoke(activity));

            context.Wait(MessageReceivedAsync);
        }
        private Func<Activity, string> CommandFactory(Activity activity)
        {
            if (Regex.IsMatch(activity.Text, @"[d|D]ice\s+(\d+)[d|D](\d+)$"))
            {
                return (a) =>
                {
                    var regex = new Regex(@"[d|D]ice\s+(\d+)[d|D](\d+)$");
                    var group = regex.Match(activity.Text).Groups;

                    var left = int.Parse(group[1].Value);
                    var right = int.Parse(group[2].Value);

                    int result = 0;
                    for (int i = 0; i < left; i++)
                    {
                        var generator = new Random();
                        result += generator.Next(1, right);
                    }

                    return result.ToString();
                };
            }
            else if (Regex.IsMatch(activity.Text, "[U|u]sage"))
            {
                return (a) => { return "avalable commands\nusage\ndice (n)D(m)"; };
            }
            else
            {
                return (a) => { return $"Sorry I don't know command [{a.Text}]."; };
            }
        }
    }
}
