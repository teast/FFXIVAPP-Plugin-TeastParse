using System;
using FFXIVAPP.Plugin.TeastParse.Models;

namespace FFXIVAPP.Plugin.TeastParse.Factories
{
    internal interface IDetrimentalFactory
    {
        DetrimentalModel GetModel(string name, string timestamp, DateTime timeUtc, string source, string target, string chatCode, string direction, string subject, IActionFactory actionFactory);
    }

    internal class DetrimentalFactory : IDetrimentalFactory
    {
        public DetrimentalModel GetModel(string name, string timestamp, DateTime timeUtc, string source, string target, string chatCode, string direction, string subject, IActionFactory actionFactory)
        {
            // TODO: Use an "database" to lookup information about an detrimental here
            var actionName = name;
            var action = actionFactory.GetAction(actionName, isDetrimental: true);
            var lastUtc = GetLastUtc(action, timeUtc);
            var potency = 0;
            return new DetrimentalModel(name, action, potency, timestamp, timeUtc, lastUtc, source, target, chatCode, direction, subject);
        }

        private DateTime? GetLastUtc(ActionModel action, DateTime timeUtc)
        {
            if (action.Duration == 0)
                return null;
            return timeUtc.AddSeconds(action.Duration);
        }
    }
}