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
        event EventHandler<ActorAddedEvent> MonsterActorAdded;
        ulong PartyTotalDamage { get; }
        ulong AllianceTotalDamage { get; }

        List<ActorModel> GetParty();
        List<ActorModel> GetAlliance();
        List<ActorModel> GetMonster();
        List<ActorModel> GetAll();
        ActorModel GetModel(string name, ChatCodeSubject subject, ChatCodeDirection? direction = null);
        void AddToTotalDamage(ActorModel actor, DamageModel damage);
        void AddToTotalDamageTaken(ActorModel actor, DamageModel damage);
        void AddToTotalCure(ActorModel source, CureModel model);
    }

    /// <summary>
    /// Concrete implementation of <see cref="IActorModelCollection" />
    /// </summary>
    internal class ActorModelCollection : IActorModelCollection
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
        public event EventHandler<ActorAddedEvent> MonsterActorAdded;

        public ulong PartyTotalDamage { get; private set; }
        public ulong AllianceTotalDamage { get; private set; }
        public ulong PartyTotalDamageTaken { get; private set; }
        public ulong AllianceTotalDamageTaken { get; private set; }
        public ulong AllianceTotalHeal { get; private set; }
        public ulong PartyTotalHeal { get; private set; }

        public IActionFactory ActionFactory { get; }

        public ActorModelCollection(ITimelineCollection timeline, IActorItemHelper actors, IActionFactory actionFactory, IRepository repository)
            : this(timeline, actors, actionFactory, repository, new List<ActorModel>())
        {

        }
        public ActorModelCollection(ITimelineCollection timeline, IActorItemHelper actors, IActionFactory actionFactory, IRepository repository, List<ActorModel> localActors)
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
            _localActors = localActors ?? new List<ActorModel>();
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

        public List<ActorModel> GetMonster()
        {
            return _localActors.Where(a => a.ActorType == ActorType.Monster).ToList();
        }

        public List<ActorModel> GetAll()
        {
            return _localActors.ToList();
        }

        public ActorModel GetModel(string name, ChatCodeSubject subject, ChatCodeDirection? direction = null)
        {
            ActorItem actorItem;
            ActorType actorType;

            // TODO: Now we got this on multiple places...
            if (name == "You" || name == "you")
                name = _actors.CurrentPlayer?.Name ?? name;

            var actor = _localActors.FirstOrDefault(a => a.Name == name);
            if (actor != null)
            {
                if (actor.IsFromMemory)
                    return actor;

                // The actor was previous not found in FFXIV memory, so lets try and find it again
                if (direction != null)
                    (actorItem, actorType) = FindActor(name, direction.Value, subject);
                else
                    (actorItem, actorType) = FindActor(name, subject);

                if (actorItem == null)
                    return actor;

                actor.UpdateFromMemory(actorItem, actorType);
                _repository.UpdateActor(actor);
                return actor;
            }

            if (direction != null)
                (actorItem, actorType) = FindActor(name, direction.Value, subject);
            else
                (actorItem, actorType) = FindActor(name, subject);

            if (actorItem == null)
            {
                actorType = TranslateSubject(subject);
                Logging.Log(Logger, $"Could not find \"{name}\" subject: {subject}. Creating an placeholder for it with type {actorType.ToString()}");
            }

            var isParty = direction != null ? IsPartyDirection(direction.Value, subject) : IsPartySubject(subject);
            var isAlliance = direction != null ? IsAllianceDirection(direction.Value, subject) : IsAllianceSubject(subject);

            actor = new ActorModel(name, actorItem, actorType,
                    _timeline,
                    name == _actors.CurrentPlayer?.Name,
                    isParty, isAlliance);

            Logging.Log(Logger, $"Creating1 Actor model \"{actor.Name}\" IsParty: {actor.IsParty}, IsAlliance: {actor.IsAlliance}. ChatcodeSubject: {subject.ToString()}");
            AddActorToLocal(actor);

            return actor;
        }

        public void AddToTotalDamage(ActorModel actor, DamageModel damage)
        {
            if (actor == null || (!actor.IsParty && !actor.IsAlliance))
                return;
            if (actor.IsParty)
                PartyTotalDamage += damage.Damage;
            if (actor.IsAlliance)
                AllianceTotalDamage += damage.Damage;

            _localActors.ForEach(a => a.TotalDmgUpdated(PartyTotalDamage, AllianceTotalDamage));
        }

        public void AddToTotalDamageTaken(ActorModel actor, DamageModel damage)
        {
            if (actor == null || (!actor.IsParty && !actor.IsAlliance))
                return;
            if (actor.IsParty)
                PartyTotalDamageTaken += damage.Damage;
            if (actor.IsAlliance)
                AllianceTotalDamageTaken += damage.Damage;

            _localActors.ForEach(a => a.TotalDmgTakenUpdated(PartyTotalDamageTaken, AllianceTotalDamageTaken));
        }

        public void AddToTotalCure(ActorModel actor, CureModel model)
        {
            if (actor == null || (!actor.IsParty && !actor.IsAlliance))
                return;
            if (actor.IsParty)
                PartyTotalHeal += model.Cure;
            if (actor.IsAlliance)
                AllianceTotalHeal += model.Cure;

            _localActors.ForEach(a => a.TotalCureUpdated(PartyTotalHeal, AllianceTotalHeal));
        }

        private void AddActorToLocal(ActorModel actor)
        {
            _localActors.Add(actor);
            _repository.AddActor(actor);

            if (actor.IsParty)
                RaisePartyAdded(actor);
            if (actor.IsAlliance)
                RaiseAllianceAdded(actor);
            if (actor.ActorType == ActorType.Monster)
                RaiseMonsterAdded(actor);
        }

        /// <summary>
        /// Helper method for selecting actor based on subject and name
        /// </summary>
        /// <param name="name">name of the actor to find</param>
        /// <param name="subject">what subject the specific chat code has</param>
        /// <returns>an actor if found, else null</returns>
        private (ActorItem actor, ActorType type) FindActor(string name, ChatCodeSubject subject)
        {
            var type = TranslateSubject(subject);
            var actor = _actors[type, name];
            if (actor != null)
                return (actor, type);
            return _actors[name];
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
            var type = TranslateDirection(direction, subject);
            var actor = _actors[type, name];
            if (actor != null)
                return (actor, type);
            return _actors[name];
        }

        private ActorType TranslateDirection(ChatCodeDirection direction, ChatCodeSubject subject)
        {
            switch (direction)
            {
                case ChatCodeDirection.Alliance:
                case ChatCodeDirection.Party:
                    return ActorType.Player;
                case ChatCodeDirection.Pet:
                case ChatCodeDirection.PetAlliance:
                case ChatCodeDirection.PetOther:
                case ChatCodeDirection.PetParty:
                    return ActorType.Player;
                case ChatCodeDirection.Self:
                    return TranslateSubject(subject);
                case ChatCodeDirection.Engaged:
                case ChatCodeDirection.UnEngaged:
                    return ActorType.Monster;
                default:
                    return ActorType.NPC;
            }
        }

        private ActorType TranslateSubject(ChatCodeSubject subject)
        {
            switch (subject)
            {
                case ChatCodeSubject.Alliance:
                case ChatCodeSubject.Party:
                case ChatCodeSubject.You:
                    return ActorType.Player;
                case ChatCodeSubject.NPC:
                case ChatCodeSubject.Pet:
                case ChatCodeSubject.PetAlliance:
                case ChatCodeSubject.PetOther:
                case ChatCodeSubject.PetParty:
                    return ActorType.Player;
                case ChatCodeSubject.Engaged:
                case ChatCodeSubject.UnEngaged:
                    return ActorType.Monster;
                default:
                    return ActorType.NPC;
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

        private void RaiseMonsterAdded(ActorModel actor)
        {
            var args = new ActorAddedEvent(this, actor);
            MonsterActorAdded?.Invoke(this, args);
        }

        /// <summary>
        /// All <see cref="ChatCodeSubject" /> that an party member can have
        /// </summary>
        private static bool IsPartySubject(ChatCodeSubject subject) =>
                subject.HasFlag(ChatCodeSubject.Party) ||
                subject.HasFlag(ChatCodeSubject.PetParty) ||
                subject.HasFlag(ChatCodeSubject.You) ||
                subject.HasFlag(ChatCodeSubject.Pet);

        /// <summary>
        /// All <see cref="ChatCodeSubject" /> that an alliance member can have
        /// </summary>
        private static bool IsAllianceSubject(ChatCodeSubject subject) =>
                subject.HasFlag(ChatCodeSubject.Alliance) ||
                subject.HasFlag(ChatCodeSubject.PetAlliance);

        /// <summary>
        /// All <see cref="ChatCodeDirection" /> that an party member can have
        /// </summary>
        private static bool IsPartyDirection(ChatCodeDirection direction, ChatCodeSubject subject) =>
            (direction.HasFlag(ChatCodeDirection.Party) ||
            direction.HasFlag(ChatCodeDirection.PetParty) ||
            direction.HasFlag(ChatCodeDirection.You) ||
            direction.HasFlag(ChatCodeDirection.Pet)
            )
            || (
                direction.HasFlag(ChatCodeDirection.Self) &&
                (
                    subject.HasFlag(ChatCodeSubject.Party) ||
                    subject.HasFlag(ChatCodeSubject.You) ||
                    subject.HasFlag(ChatCodeSubject.PetParty) ||
                    subject.HasFlag(ChatCodeSubject.Pet)
                )
            );

        /// <summary>
        /// All <see cref="ChatCodeDirection" /> that an alliance member can have
        /// </summary>
        private static bool IsAllianceDirection(ChatCodeDirection direction, ChatCodeSubject subject) =>
            (
            direction.HasFlag(ChatCodeDirection.Alliance) ||
            direction.HasFlag(ChatCodeDirection.PetAlliance)
            )
            || (
                direction.HasFlag(ChatCodeDirection.Self) &&
                (
                    subject.HasFlag(ChatCodeSubject.Alliance) ||
                    subject.HasFlag(ChatCodeSubject.PetAlliance)
                )
            );
    }
}