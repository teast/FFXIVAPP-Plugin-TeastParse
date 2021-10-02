using System;
using FFXIVAPP.Plugin.TeastParse.Models;

namespace FFXIVAPP.Plugin.TeastParse.Actors
{
    public class ActorActionModel
    {
        public ActionModel Action { get; }
        public string Name { get; }
        public string Icon { get; }
        public DateTime OccurredUtc { get; }
        public string Timestamp { get; }
        public int Damage { get; }

        public ActorActionModel(string occurredUtc, string timestamp, string action, int damage)
            : this(occurredUtc, timestamp, action, damage, null) {}

        public ActorActionModel(string occurredUtc, string timestamp, string action, int damage, ActionModel actionModel)
        {
            Name = actionModel != null ? $"{action}" : action;
            OccurredUtc = DateTime.SpecifyKind(DateTime.Parse(occurredUtc), DateTimeKind.Utc);
            Timestamp = timestamp;
            Action = actionModel;
            Damage = damage;
            Icon = (actionModel?.Icon ?? "").Replace('/', '_');

            if (actionModel != null && string.IsNullOrEmpty(actionModel.Icon))
            {
                Console.WriteLine($"Here! \"{actionModel.Name}\"");
            }
        }
    }
}