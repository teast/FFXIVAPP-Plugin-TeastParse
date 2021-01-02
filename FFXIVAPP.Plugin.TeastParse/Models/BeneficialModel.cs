using System;

namespace FFXIVAPP.Plugin.TeastParse.Models
{
    /// <summary>
    /// Represents an detrimental
    /// </summary>
    public class BeneficialModel : ActorStatusModel
    {
        public BeneficialModel(string name, ActionModel action, int potency, string timestamp,
            DateTime timeUtc, DateTime? lastUtc, string source, string target,
            string chatCode, string direction, string subject)
            : base(name, action, potency, timestamp, timeUtc, lastUtc, source, target, chatCode, direction, subject)
        {

        }

        public static implicit operator DetrimentalModel(BeneficialModel model)
        {
            if (model == null)
                return null;
            return new DetrimentalModel(
                model.Name,
                model.Action,
                model.Potency,
                model.Timestamp,
                model.TimeUtc,
                model.LastUtc,
                model.Source,
                model.Target,
                model.ChatCode,
                model.Direction,
                model.Subject
            );
        }
    }
}