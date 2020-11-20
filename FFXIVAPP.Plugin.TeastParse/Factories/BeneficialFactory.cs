using System;
using FFXIVAPP.Plugin.TeastParse.Models;

namespace FFXIVAPP.Plugin.TeastParse.Factories
{
    internal interface IBeneficialFactory
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
            var actionName = name == "Confession" ? "Plenary Indulgence" : name;
            var potency = 0;
            return new BeneficialModel(name, actionFactory.GetAction(actionName), potency, timestamp, timeUtc, lastUtc, source, target, chatCode, direction, subject);
        }
    }
}