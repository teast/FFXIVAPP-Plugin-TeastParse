using System;

namespace FFXIVAPP.Plugin.TeastParse.Models
{
    /// <summary>
    /// Represents an detrimental
    /// </summary>
    internal class DetrimentalModel : ActorStatusModel
    {
        public DetrimentalModel(string name, string actionName, int potency, string timestamp,
            DateTime timeUtc, DateTime? lastUtc, string source, string target,
            string chatCode, string direction, string subject)
            : base(name, actionName, potency, timestamp, timeUtc, lastUtc, source, target, chatCode, direction, subject)
        {

        }

        public static implicit operator DamageModel(DetrimentalModel model)
        {
            if (model == null)
                throw new InvalidCastException($"Cannot convert a null {nameof(DetrimentalModel)} to {nameof(DamageModel)}");

            return new DamageModel
            {
                Action = model.ActionName,
                Blocked = false,
                Critical = false,
                DirectHit = false,
                ChatCode = model.ChatCode,
                Damage = 0,
                Direction = model.Direction,
                InitDmg = 0,
                Modifier = null,
                Parried = false,
                Source = model.Source,
                Subject = model.Subject,
                Target = model.Target,
                Timestamp = model.Timestamp,
                OccurredUtc = model.TimeUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                IsDetrimental = true
            };
        }
    }
}