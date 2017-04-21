using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CthulhuDiceBot.Factories
{

    public class CommandFactory
    {
        public readonly Dictionary<string, Tuple<Regex, Func<Activity, string>>> dictionary
            = new Dictionary<string, Tuple<Regex, Func<Activity, string>>>();
        
        private CommandFactory()
        {
            {
                var regex = new Regex(@"[d|D]ice\s+(\d+)[d|D](\d+)$");
                dictionary.Add(
                    "Dice",
                    Tuple.Create<Regex, Func<Activity, string>>(
                        regex,
                        (a) =>
                            {
                                var group = regex.Match(a.Text).Groups;
                                var left = int.Parse(group[1].Value);
                                var right = int.Parse(group[2].Value);

                                int result = 0;
                                for (int i = 0; i < left; i++)
                                {
                                    var generator = new Random();
                                    result += generator.Next(1, right);
                                }
                                return result.ToString();
                            }
                    )
                );
            }
            {
                var regex = new Regex("[U|u]sage");
                dictionary.Add(
                    "Usage",
                    Tuple.Create<Regex, Func<Activity, string>>(
                        regex,
                        (a) => { return "avalable commands\nusage\ndice (n)D(m)"; }
                    )
                );
            }
        }
    }
}