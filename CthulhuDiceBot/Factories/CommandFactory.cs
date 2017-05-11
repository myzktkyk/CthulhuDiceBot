using Microsoft.Bot.Connector;
using Microsoft.Cognitive.LUIS;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;

namespace CthulhuDiceBot.Factories
{

    public class CommandFactory
    {

        #region static readonly fields

        private static readonly Dictionary<string, Func<Activity, LuisResult, string>> dictionary = new Dictionary<string, Func<Activity, LuisResult, string>>();

        private static readonly Regex MentionRegex = new Regex(@"^@\w+\s+");

        private static readonly Regex DimensionRegex = new Regex(@"^(\d+)d(\d+)$");

        #endregion

        #region properties

        public static CommandFactory Instance { get; } = new CommandFactory();

        public static LuisClient LuisClient { get; } = new LuisClient(ConfigurationManager.AppSettings["MicrosoftLuisAppId"], ConfigurationManager.AppSettings["MicrosoftLuisAppKey"]);

        #endregion

        #region constractor

        private CommandFactory()
        {
            dictionary.Add("DiceRoll", (a, r) => CalculateDice(a, r));
            dictionary.Add("Regist", (a, r) => CalculateRegist(a, r));
            dictionary.Add("Judge", (a, r) => CalculateJudge(a, r));
        }

        #endregion

        #region public method(s)

        public Func<Activity, LuisResult, string> GetCommand(Activity activity)
        {
            LuisResult response = LuisClient.Predict(activity.Text).Result;
            Func<Activity, LuisResult, string> command = dictionary[response.TopScoringIntent.Name];
            if (command == null)
            {
                command = (a, r) => { return $"Sorry I don't know command [{a.Text}]."; };
            }
            return command;
        }

        public string ExecuteCommand(Activity activity)
        {
            LuisResult response = LuisClient.Predict(activity.Text).Result;
            Func<Activity, LuisResult, string> command = dictionary[response.TopScoringIntent.Name];
            return command.Invoke(activity, response);
        }

        public bool IsCommand(Activity activity)
        {
            bool result = false;
            bool isGroup = activity.Conversation.IsGroup ?? false;
            if (!isGroup || MentionRegex.IsMatch(activity.Text.ToLower()))
            {
                LuisResult response = LuisClient.Predict(activity.Text).Result;
                result = dictionary.ContainsKey(response.TopScoringIntent.Name);
            }
            return result;
        }

        #endregion

        #region private method(s)

        private string CalculateDice(Activity activity, LuisResult luisResult)
        {
            var dimension = luisResult.Entities["Dimension"].FirstOrDefault().Value;
            var group = DimensionRegex.Match(dimension).Groups;
            var left = int.Parse(group[1].Value);
            var right = int.Parse(group[2].Value);
            if (right <= 0)
            {
                throw new ArgumentException("１以上の数字を指定してください！");
            }
            int dice = 0;
            for (int i = 0; i < left; i++)
            {
                var generator = new Random();
                dice += generator.Next(1, right);
            }
            return $"出目は{dice.ToString()}だよ！";
        }

        private string CalculateRegist(Activity activity, LuisResult luisResult)
        {
            var active = int.Parse(luisResult.Entities["Active"].FirstOrDefault().Value);
            var passive = int.Parse(luisResult.Entities["Passive"].FirstOrDefault().Value);

            int target = (active - passive) * 5 + 50;
            int dice = new Random().Next(1, 100);
            bool isSuccess = dice <= target;

            string result = null;
            if (isSuccess)
            {
                result = $"抵抗成功！ 目標値 = {target} ダイス = {dice}";
            }
            else
            {
                result = $"抵抗失敗・・・。 目標値 = {target} ダイス = {dice}";
            }
            return result;
        }

        private string CalculateJudge(Activity activity, LuisResult luisResult)
        {
            var target = int.Parse(luisResult.Entities["TargetValue"].FirstOrDefault().Value);

            int dice = 0;
            for (int i = 0; i < 1; i++)
            {
                var generator = new Random();
                dice += generator.Next(1, 100);
            }

            string result = null;
            bool isSuccess = (dice <= target);
            if (isSuccess)
            {
                if (dice == target)
                {
                    result = $"いちたりた！ {dice}/{target}";
                }
                else
                {
                    result = $"成功！ {dice}/{target}";
                }
            }
            else
            {
                result = $"失敗・・・。 {dice}/{target}";
            }
            return result;
        }

        #endregion
    }
}