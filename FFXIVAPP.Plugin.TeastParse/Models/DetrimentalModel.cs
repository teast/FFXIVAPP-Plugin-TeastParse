using System;

namespace FFXIVAPP.Plugin.TeastParse.Models
{
    /// <summary>
    /// Represents an detrimental
    /// </summary>
    public class DetrimentalModel : ActorStatusModel
    {
        public DetrimentalModel(string name, ActionModel action, int potency, string timestamp,
            DateTime timeUtc, DateTime? lastUtc, string source, string target,
            string chatCode, string direction, string subject)
            : base(name, action, potency, timestamp, timeUtc, lastUtc, source, target, chatCode, direction, subject)
        {

        }

        public static implicit operator DamageModel(DetrimentalModel model)
        {
            if (model == null)
                throw new InvalidCastException($"Cannot convert a null {nameof(DetrimentalModel)} to {nameof(DamageModel)}");

            return new DamageModel(
                occurredUtc: model.TimeUtc.ToString("yyyy-MM-dd HH:mm:ss"),
                timestamp: model.Timestamp,
                source: model.Source,
                target: model.Target,
                damage: 0,
                modifier: null,
                action: model.Action,
                critical: false,
                directHit: false,
                blocked: false,
                parried: false,
                initDmg: 0,
                endTimeUtc: null,
                subject: model.Subject,
                direction: model.Direction,
                chatCode: model.ChatCode,
                isDetrimental: true
            );
        }
    }
}