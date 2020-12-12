using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NLog;
using FFXIVAPP.Common.Utilities;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse.Actors
{
    /// <summary>
    /// Helper for finding actors directly from FFXIV's memory
    /// </summary>
    public interface IActorItemHelper
    {
        CurrentPlayer CurrentPlayer { get; set; }
        (ActorItem item, ActorType type) this[string name] { get; }
        ActorItem this[ActorType type, string name] { get; }
        void HandleUpdate(ConcurrentDictionary<uint, ActorItem> items, ActorType type);
    }

    internal class ActorItemHelper : IActorItemHelper
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<ActorType, ActorParser> _actors;

        public CurrentPlayer CurrentPlayer { get; set; }

        public ActorItem this[uint id]
        {
            get
            {
                foreach (var e in _actors)
                {
                    if (e.Value.Actors.ContainsKey(id))
                        return e.Value.Actors[id];
                }

                return null;
            }
        }

        public (ActorItem item, ActorType type) this[string name]
        {
            get
            {
                // TODO: Add an you check for all langauges...
                if (CurrentPlayer != null && name.ToLowerInvariant() == "you")
                    name = CurrentPlayer.Name;

                foreach (var e in _actors)
                {
                    var model = e.Value.Actors.FirstOrDefault(_ => _.Value.Name == name);
                    if (model.Value != null)
                        return (model.Value, e.Value.Type);
                }



                return (null, ActorType.Player);
            }
        }

        public ActorItem this[ActorType type, string name]
        {
            get
            {
                // TODO: Add an you check for all langauges...
                if (CurrentPlayer != null && type == ActorType.Player && name.ToLowerInvariant() == "you")
                    name = CurrentPlayer.Name;

                var model = _actors[type].Actors.FirstOrDefault(_ => _.Value.Name == name);

                // Chat names can have server names as suffix...
                if (model.Value == null)
                {
                    var hits = _actors[type].Actors.Where(_ => name.StartsWith(_.Value.Name)).ToList();
                    if (hits.Count == 1)
                        return hits[0].Value;

                    model = hits.FirstOrDefault(_ =>
                    {
                        var serverName = name.Substring(_.Value.Name.Length);
                        Logging.Log(Logger, $"Hard part! \"{serverName}\" from \"{name}\" ({_.Value.Name})");
                        return ServerNames.Japanese.Contains(serverName) ||
                            ServerNames.NAmericans.Contains(serverName) ||
                            ServerNames.Europeans.Contains(serverName);
                    });
                }

                return model.Value;
            }
        }

        public ActorItemHelper()
        {
            _actors = new Dictionary<ActorType, ActorParser>
            {
                {ActorType.Player, new ActorParser(ActorType.Player)},
                {ActorType.NPC, new ActorParser(ActorType.NPC)},
                {ActorType.Monster, new ActorParser(ActorType.Monster)}
            };
        }

        public void HandleUpdate(ConcurrentDictionary<uint, ActorItem> items, ActorType type)
        {
            _actors[type].HandelUpdate(items);
        }
    }
}