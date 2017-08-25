using Microsoft.Bot.Connector;
using Microsoft.Cognitive.LUIS;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;

namespace CthulhuDiceBot.Factories
{

    public class CommandFactory
    {

        #region static readonly fields

        private static readonly Dictionary<string, Func<Activity, LuisResult, string>> Dictionary = new Dictionary<string, Func<Activity, LuisResult, string>>();

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
            Dictionary.Add("DiceRoll", CalculateDice);
            Dictionary.Add("Regist", CalculateRegist);
            Dictionary.Add("Judge", CalculateJudge);
        }

        #endregion

        #region public method(s)

        public Func<Activity, LuisResult, string> GetCommand(Activity activity)
        {
            var response = LuisClient.Predict(activity.Text).Result;
            var command = Dictionary[response.TopScoringIntent.Name] ?? ((a, r) => $"Sorry I don't know command [{a.Text}].");
            return command;
        }

        public string ExecuteCommand(Activity activity)
        {
            var response = LuisClient.Predict(activity.Text).Result;
            var command = Dictionary[response.TopScoringIntent.Name];
            return command.Invoke(activity, response);
        }

        public bool IsCommand(Activity activity)
        {
            var isGroup = activity.Conversation.IsGroup ?? false;
            if (isGroup && !MentionRegex.IsMatch(activity.Text.ToLower())) return false;
            var response = LuisClient.Predict(activity.Text).Result;
            var result = Dictionary.ContainsKey(response.TopScoringIntent.Name);
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
            var dice = 0;
            for (var i = 0; i < left; i++)
            {
                var generator = new Random();
                dice += generator.Next(1, right);
            }
            return $"出目は{dice}だよ！";
        }

        private string CalculateRegist(Activity activity, LuisResult luisResult)
        {
            var active = int.Parse(luisResult.Entities["Active"].FirstOrDefault().Value);
            var passive = int.Parse(luisResult.Entities["Passive"].FirstOrDefault().Value);

            var target = (active - passive) * 5 + 50;
            var dice = new Random().Next(1, 100);
            var isSuccess = dice <= target;

            var result = isSuccess ? $"抵抗成功！ 目標値 = {target} ダイス = {dice}" : $"抵抗失敗・・・。 目標値 = {target} ダイス = {dice}";
            return result;
        }

        private string CalculateJudge(Activity activity, LuisResult luisResult)
        {
            var target = int.Parse(luisResult.Entities["TargetValue"].FirstOrDefault()?.Value);
            var skill = luisResult.Entities["Skill"].FirstOrDefault()?.Value;

            var dice = 0;
            for (var i = 0; i < 1; i++)
            {
                var generator = new Random();
                dice += generator.Next(1, 100);
            }

            string result;
            var isSuccess = dice <= target;
            if (isSuccess)
            {
                result = dice == target ? $"{skill}いちたりた！ {dice}/{target}" : $"{skill}成功！ {dice}/{target}";
            }
            else
            {
                result = dice - target == 1 ? $"{skill}いちたりない・・・。 {dice}/{target}" : $"{skill}失敗・・・。 {dice}/{target}";
            }
            return result;
        }

        #endregion
    }
}