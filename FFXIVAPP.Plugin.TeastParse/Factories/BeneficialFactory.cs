using System;
using FFXIVAPP.Plugin.TeastParse.Models;

namespace FFXIVAPP.Plugin.TeastParse.Factories
{
    public interface IBeneficialFactory
    {
        BeneficialModel GetModel(string name, string timestamp, DateTime timeUtc, string source, string target, string chatCode, string direction, string subject, IActionFactory actionFactory);
    }

    /// <summary>
    /// This class creates an <see cref="BeneficialModel" /> based on input name parameter and will populate extra fields based on
    /// current FFXIV version
    /// </summary>
    internal class BeneficialFactory : IBeneficialFactory
    {
        public BeneficialModel GetModel(string name, string timestamp, DateTime timeUtc, string source, string target, string chatCode, string direction, string subject, IActionFactory actionFactory)
        {
            // TODO: Use an "database" to lookup information about an beneficial here
            var lastUtc = (DateTime?)null;
            var actionName = GetActionName(name);
            var potency = 0;
            return new BeneficialModel(name, GetAction(actionName, actionFactory), potency, timestamp, timeUtc, lastUtc, source, target, chatCode, direction, subject);
        }

        private ActionModel GetAction(string actionName, IActionFactory actionFactory)
        {
            if (actionFactory.ActionExist(actionName))
                return actionFactory.GetAction(actionName);
            else
                return actionFactory.GetFakeAction(actionName);
            /*
            switch(actionName)
            {
                case "Weakness":
                case "Transcendent":
                case "Raiden Thrust Ready":
                case "Sharper Fang and Claw":
                case "Dance Partner":
                case "Nascent Chaos":
                case "Dualcast":
                case "Verfire Ready":
                case "Verstone Ready":
                case "Straight Shot Ready":
                case "The Wanderer's Minuet":
                case "Esprit":
                case "Left Eye":
                case "Right Eye":
                case "Arrow Drawn":
                case "Dive Ready":
                case "Flourishing Cascade":
                case "Flourishing Fountain":
                case "Flourishing Windmill":
                case "Flourishing Shower":
                case "Flourishing Fan Dance":
                case "Meditative Brotherhood":
                    return actionFactory.GetFakeAction(actionName);
                default:
                    return actionFactory.GetAction(actionName);
            }
            */
        }

        private string GetActionName(string name)
        {
            switch (name)
            {
                case "Confession":
                    return "Plenary Indulgence";
                default:
                    return name;
            }
        }
    }
}