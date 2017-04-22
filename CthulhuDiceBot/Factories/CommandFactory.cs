using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CthulhuDiceBot.Factories
{

    public class CommandFactory
    {

        #region static readonly fields

        private static readonly Dictionary<string, Tuple<Regex, Func<Activity, string>>> dictionary
            = new Dictionary<string, Tuple<Regex, Func<Activity, string>>>();

        private static readonly Regex DiceCommandRegex = new Regex(@"dice\s+(\d+)d(\d+)$");

        private static readonly Regex UsageCommandRegex = new Regex("usage$");

        private static readonly Regex MentionRegex = new Regex(@"^@\w+\s+");

        private static readonly Regex RegistCommandRegex = new Regex(@"regist\s+(\d+)/(\d+)$");

        private static readonly Regex JudgeCommandRegex = new Regex(@"judge\s+(\d+)d(\d+)/(\d+)$");


        #endregion

        #region properties

        public static CommandFactory Instance { get; private set; } = new CommandFactory();

        #endregion

        #region constractor

        private CommandFactory()
        {
            dictionary.Add(
                "Dice",
                Tuple.Create<Regex, Func<Activity, string>>(DiceCommandRegex, (a) => CalculateDice(a))
            );
            dictionary.Add(
                "Usage",
                Tuple.Create<Regex, Func<Activity, string>>(UsageCommandRegex, (a) => { return "avalable commands\nusage\ndice (n)D(m)"; })
            );
            dictionary.Add(
                "Regist",
                Tuple.Create<Regex, Func<Activity, string>>(RegistCommandRegex, (a) => CalculateRegist(a))
            );
            dictionary.Add(
                "Judge",
                Tuple.Create<Regex, Func<Activity, string>>(JudgeCommandRegex, (a) => CalculateJudge(a))
            );

        }

        #endregion

        #region public method(s)

        public Func<Activity, string> GetCommand(Activity activity)
        {
            Func<Activity, string> command = null;
            foreach (var entry in dictionary.Values)
            {
                if (entry.Item1.IsMatch(activity.Text.ToLower()))
                {
                    command = entry.Item2;
                    break;
                }
            }
            if (command == null)
            {
                command = (a) => { return $"Sorry I don't know command [{a.Text}]."; };
            }
            return command;
        }

        public bool IsCommand(Activity activity)
        {
            bool result = false;
            if (MentionRegex.IsMatch(activity.Text))
            {
                foreach (var entry in dictionary.Values)
                {
                    if (entry.Item1.IsMatch(activity.Text.ToLower()))
                    {
                        result = true;
                        break;
                    }
                }
            }
            return result;
        }

        #endregion

        #region private method(s)

        private string CalculateDice(Activity activity)
        {
            var group = DiceCommandRegex.Match(activity.Text.ToLower()).Groups;
            var left = int.Parse(group[1].Value);
            var right = int.Parse(group[2].Value);

            int dice = 0;
            for (int i = 0; i < left; i++)
            {
                var generator = new Random();
                dice += generator.Next(1, right);
            }
            return dice.ToString();
        }

        private string CalculateRegist(Activity activity)
        {
            var group = RegistCommandRegex.Match(activity.Text.ToLower()).Groups;
            var active = int.Parse(group[1].Value);
            var passive = int.Parse(group[2].Value);

            int target = (active - passive) * 5 + 50;
            int dice = new Random().Next(1, 100);
            bool isSuccess = dice <= target;

            string result = null;
            if (isSuccess)
            {
                result = $"Regist succeeded! Target = {target} Dice = {dice}";
            }
            else
            {
                result = $"Regist failed... Target = {target} Dice = {dice}";
            }
            return result;
        }

        private string CalculateJudge(Activity activity)
        {
            var group = JudgeCommandRegex.Match(activity.Text).Groups;
            var left = int.Parse(group[1].Value);
            var right = int.Parse(group[2].Value);
            var target = int.Parse(group[3].Value);

            int dice = 0;
            for (int i = 0; i < left; i++)
            {
                var generator = new Random();
                dice += generator.Next(1, right);
            }

            string result = null;
            bool isSuccess = (dice <= target);
            if (isSuccess)
            {
                result = $"Succeeded! {dice}/{target}";
            }
            else
            {
                result = $"Failed... {dice}/{target}";
            }
            return result;
        }

        #endregion
    }
}