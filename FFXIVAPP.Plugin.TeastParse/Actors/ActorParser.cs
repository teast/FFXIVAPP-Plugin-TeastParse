using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using NLog;
using Sharlayan.Core;
using static Sharlayan.Core.Enums.Actor;

namespace FFXIVAPP.Plugin.TeastParse.Actors
{
    internal class ActorParser
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public ActorType Type { get; }

        public ConcurrentDictionary<uint, ActorItem> Actors { get; private set; }

        public ActorParser(ActorType type)
        {
            Actors = new ConcurrentDictionary<uint, ActorItem>();
            Type = type;
        }

        public void HandelUpdate(ConcurrentDictionary<uint, ActorItem> items)
        {
            Actors = items;
        }

        private string PrintActorItems(ConcurrentDictionary<uint, ActorItem> items)
        {
            var sb = new System.Text.StringBuilder("{");
            var first = true;
            foreach (var item in items)
            {
                if (!first)
                    sb.Append(",");
                sb.Append($"\"{item.Key}\": {{");
                sb.Append(PrintActorItem(item.Value));
                sb.Append("}");
                first = false;
            }

            sb.Append("}");
            return sb.ToString();
        }

        private string PrintActorItem(ActorItem item)
        {
            return $"\"Name\": \"{item.Name}\", \"Job\": \"{item.Job}\", \"Level\": \"{item.Level}\", \"Coordinates\": \"{item.Coordinate}\", \"mapID\": \"{item.MapID}\", \"MapIndex\": \"{item.MapIndex}\", \"MapTerritory\": \"{item.MapTerritory}\"";
        }
    }
}