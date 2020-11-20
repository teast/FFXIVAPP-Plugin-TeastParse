using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.Events;
using FFXIVAPP.Plugin.TeastParse.Factories;
using FFXIVAPP.Plugin.TeastParse.Models;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using NLog;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse
{
    /// <summary>
    /// Keep trakcs on all Actors that have been notice in any of the monitoring events.
    /// Also keep tracks on total damage/heal made/taken.
    /// </summary>
    public interface IActorModelCollection
    {
        event EventHandler<ActorAddedEvent> PartyActorAdded;
        event EventHandler<ActorAddedEvent> AllianceActorAdded;
        ulong PartyTotalDamage { get; }
        ulong AllianceTotalDamage { get; }

        List<ActorModel> GetParty();
        List<ActorModel> GetAlliance();
        List<ActorModel> GetAll();
        ActorModel GetModel(string name, ChatCodeDirection direction, ChatCodeSubject subject);
        ActorModel GetModel(string name, ChatCodeSubject subject);
        void AddToTotalDamage(ActorModel actor, DamageModel damage);
        void AddToTotalDamageTaken(ActorModel actor, DamageModel damage);
        void AddToTotalCure(ActorModel source, CureModel model);
    }

    /// <summary>
    /// Keep tracks on total damage/heal/etc for party/alliance
    /// </summary>
    internal interface ITotalStats
    {
        ulong PartyTotalDamage { get; }
        ulong PartyTotalDamageTaken { get; }
        ulong PartyTotalHeal { get; }
        ulong AllianceTotalDamage { get; }
        ulong AllianceTotalDamageTaken { get; }
        ulong AllianceTotalHeal { get; }
        IActionFactory ActionFactory { get; }
    }

    /// <summary>
    /// Concrete implementation of <see cref="IActorModelCollection" /> and <see cref="ITotalStats" />
    /// </summary>
    internal class ActorModelCollection : IActorModelCollection, ITotalStats
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly ITimelineCollection _timeline;

        /// <summary>
        /// This helps fetching actors that have been found in FFXIV' memory
        /// </summary>
        private readonly IActorItemHelper _actors;
        private readonly IRepository _repository;

        /// <summary>
        /// This contains actors that we have already fetched from FFXIV's memory.
        /// </summary>
        /// <remarks>
        /// This list is not automatically updated if an actor gets updated in FFXIV's memory.
        /// </remarks>
        private readonly List<ActorModel> _localActors;

        public event EventHandler<ActorAddedEvent> PartyActorAdded;
        public event EventHandler<ActorAddedEvent> AllianceActorAdded;

        public ulong PartyTotalDamage { get; private set; }
        public ulong AllianceTotalDamage { get; private set; }
        public ulong PartyTotalDamageTaken { get; private set; }
        public ulong AllianceTotalDamageTaken { get; private set; }
        public ulong AllianceTotalHeal { get; private set; }
        public ulong PartyTotalHeal { get; private set; }

        public IActionFactory ActionFactory { get; }

        public ActorModelCollection(ITimelineCollection timeline, IActorItemHelper actors, IActionFactory actionFactory, IRepository repository)
        {
            _timeline = timeline;
            _actors = actors;
            _repository = repository;
            ActionFactory = actionFactory;
            AllianceTotalHeal = 0;
            AllianceTotalDamage = 0;
            AllianceTotalDamageTaken = 0;
            PartyTotalHeal = 0;
            PartyTotalDamage = 0;
            PartyTotalDamageTaken = 0;
            _localActors = new List<ActorModel>();
            _timeline.CurrentTimelineChange += OnTimelineChange;
        }

        private void OnTimelineChange(object sender, TimelineChangeEvent args)
        {
            AllianceTotalHeal = 0;
            AllianceTotalDamage = 0;
            AllianceTotalDamageTaken = 0;
            PartyTotalHeal = 0;
            PartyTotalDamage = 0;
            PartyTotalDamageTaken = 0;
        }

        public List<ActorModel> GetParty()
        {
            return _localActors.Where(a => a.IsParty).ToList();
        }

        public List<ActorModel> GetAlliance()
        {
            return _localActors.Where(a => a.IsAlliance).ToList();
        }

        public List<ActorModel> GetAll()
        {
            return _localActors.ToList();
        }

        public ActorModel GetModel(string name, ChatCodeDirection direction, ChatCodeSubject subject)
        {
            // TODO: Now we got this on multiple places...
            if (name == "You" || name == "you")
                name = _actors.CurrentPlayer?.Name ?? name;

            var actor = _localActors.FirstOrDefault(a => a.Name == name);
            if (actor != null)
                return actor;

            var item = FindActor(name, direction, subject);

            if (item.actor == null)
                return null;

            actor = new ActorModel(name, ExtractServerName(name, item.actor.Name), item.actor.Level,
                    item.actor.Job, _timeline,
                    _partyDirection.Contains(direction), _allianceDirection.Contains(direction),
                    item.type, this, item.actor.Coordinate);

            if (actor.Name == _actors.CurrentPlayer?.Name)
            {
                actor.IsYou = true;
                actor.IsParty = true;
            }

            _localActors.Add(actor);
            _repository.AddActor(actor);

            if (actor.IsParty)
                RaisePartyAdded(actor);
            if (actor.IsAlliance)
                RaiseAllianceAdded(actor);

            return actor;
        }

        public ActorModel GetModel(string name, ChatCodeSubject subject)
        {
            // TODO: Now we got this on multiple places...
            if (name == "You" || name == "you")
                name = _actors.CurrentPlayer?.Name ?? name;

            var actor = _localActors.FirstOrDefault(a => a.Name == name);
            if (actor != null)
                return actor;

            var item = FindActor(name, subject);

            if (item.actor == null)
            {
                Logging.Log(Logger, $"Could not find \"{name}\" subject: {subject}");
                return null;
            }

            actor = new ActorModel(name, ExtractServerName(name, item.actor.Name), item.actor.Level,
                    item.actor.Job, _timeline,
                    _partySubjects.Contains(subject), _allianceSubjects.Contains(subject),
                    item.type, this, item.actor.Coordinate);

            if (actor.Name == _actors.CurrentPlayer?.Name)
            {
                actor.IsYou = true;
                actor.IsParty = true;
            }

            _localActors.Add(actor);
            _repository.AddActor(actor);

            if (actor.IsParty)
                RaisePartyAdded(actor);
            if (actor.IsAlliance)
                RaiseAllianceAdded(actor);

            return actor;
        }

        private string ExtractServerName(string chatName, string actorName)
        {
            // TODO: Let the user configure what server the user is from and use that here
            if (chatName.Length == actorName.Length)
                return "";

            return chatName.Substring(actorName.Length);
        }

        public void AddToTotalDamage(ActorModel actor, DamageModel damage)
        {
            if (actor == null || (!actor.IsParty && !actor.IsAlliance))
                return;
            if (actor.IsParty)
                PartyTotalDamage += damage.Damage;
            if (actor.IsAlliance)
                AllianceTotalDamage += damage.Damage;

            _localActors.ForEach(a => a.TotalDmgUpdated());
        }

        public void AddToTotalDamageTaken(ActorModel actor, DamageModel damage)
        {
            if (actor == null || (!actor.IsParty && !actor.IsAlliance))
                return;
            if (actor.IsParty)
                PartyTotalDamageTaken += damage.Damage;
            if (actor.IsAlliance)
                AllianceTotalDamageTaken += damage.Damage;

            _localActors.ForEach(a => a.TotalDmgTakenUpdated());
        }

        public void AddToTotalCure(ActorModel actor, CureModel model)
        {
            if (actor == null || (!actor.IsParty && !actor.IsAlliance))
                return;
            if (actor.IsParty)
                PartyTotalHeal += model.Cure;
            if (actor.IsAlliance)
                AllianceTotalHeal += model.Cure;

            _localActors.ForEach(a => a.TotalCureUpdated());
        }

        /// <summary>
        /// Helper method for selecting actor based on subject and name
        /// </summary>
        /// <param name="name">name of the actor to find</param>
        /// <param name="subject">what subject the specific chat code has</param>
        /// <returns>an actor if found, else null</returns>
        private (ActorItem actor, ActorType type) FindActor(string name, ChatCodeSubject subject)
        {
            switch (subject)
            {
                case ChatCodeSubject.Alliance:
                case ChatCodeSubject.Party:
                case ChatCodeSubject.You:
                    if (_actors[ActorType.Player, name] != null)
                        return (_actors[ActorType.Player, name], ActorType.Player);
                    return _actors[name];
                case ChatCodeSubject.NPC:
                case ChatCodeSubject.Pet:
                case ChatCodeSubject.PetAlliance:
                case ChatCodeSubject.PetOther:
                case ChatCodeSubject.PetParty:
                    if (_actors[ActorType.Monster, name] != null)
                        return (_actors[ActorType.Monster, name], ActorType.Player);
                    return _actors[name];
                case ChatCodeSubject.Engaged:
                case ChatCodeSubject.UnEngaged:
                    if (_actors[ActorType.Monster, name] != null)
                        return (_actors[ActorType.Monster, name], ActorType.Player);
                    return _actors[name];
                default:
                    return _actors[name];
            }
        }

        /// <summary>
        /// Helper method for selecting actor based on direction, name and maybe subject
        /// </summary>
        /// <param name="name">name of the actor to find</param>
        /// <param name="direction">what direction the specific chat code has</param>
        /// <param name="subject">what subject the specific chat code has</param>
        /// <returns>an actor if found, else null</returns>
        private (ActorItem actor, ActorType type) FindActor(string name, ChatCodeDirection direction, ChatCodeSubject subject)
        {
            switch (direction)
            {
                case ChatCodeDirection.Alliance:
                case ChatCodeDirection.Party:
                    if (_actors[ActorType.Player, name] != null)
                        return (_actors[ActorType.Player, name], ActorType.Player);
                    return _actors[name];
                case ChatCodeDirection.Pet:
                case ChatCodeDirection.PetAlliance:
                case ChatCodeDirection.PetOther:
                case ChatCodeDirection.PetParty:
                    if (_actors[ActorType.Monster, name] != null)
                        return (_actors[ActorType.Monster, name], ActorType.Player);
                    return _actors[name];
                case ChatCodeDirection.Self:
                    return FindActor(name, subject);
                default:
                    return _actors[name];
            }
        }

        private void RaisePartyAdded(ActorModel actor)
        {
            var args = new ActorAddedEvent(this, actor);
            PartyActorAdded?.Invoke(this, args);
        }

        private void RaiseAllianceAdded(ActorModel actor)
        {
            var args = new ActorAddedEvent(this, actor);
            AllianceActorAdded?.Invoke(this, args);
        }

        /// <summary>
        /// All <see cref="ChatCodeSubject" /> that an party member can have
        /// </summary>
        private readonly static ChatCodeSubject[] _partySubjects = new ChatCodeSubject[]
        {
            ChatCodeSubject.Party, ChatCodeSubject.PetParty, ChatCodeSubject.You, ChatCodeSubject.Pet
        };

        /// <summary>
        /// All <see cref="ChatCodeSubject" /> that an alliance member can have
        /// </summary>
        private readonly static ChatCodeSubject[] _allianceSubjects = new ChatCodeSubject[]
        {
            ChatCodeSubject.Alliance, ChatCodeSubject.PetAlliance
        };

        /// <summary>
        /// All <see cref="ChatCodeDirection" /> that an party member can have
        /// </summary>
        private readonly static ChatCodeDirection[] _partyDirection = new ChatCodeDirection[]
        {
            ChatCodeDirection.Party, ChatCodeDirection.PetParty, ChatCodeDirection.You, ChatCodeDirection.Pet
        };

        /// <summary>
        /// All <see cref="ChatCodeDirection" /> that an alliance member can have
        /// </summary>
        private readonly static ChatCodeDirection[] _allianceDirection = new ChatCodeDirection[]
        {
            ChatCodeDirection.Alliance, ChatCodeDirection.PetAlliance
        };
    }
}