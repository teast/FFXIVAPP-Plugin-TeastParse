using System;

namespace FFXIVAPP.Plugin.TeastParse.Models
{
    /// <summary>
    /// Represents an detrimental
    /// </summary>
    internal class BeneficialModel : ActorStatusModel
    {
        public BeneficialModel(string name, string actionName, int potency, string timestamp,
            DateTime timeUtc, DateTime? lastUtc, string source, string target,
            string chatCode, string direction, string subject)
            : base(name, actionName, potency, timestamp, timeUtc, lastUtc, source, target, chatCode, direction, subject)
        {

        }
    }
}