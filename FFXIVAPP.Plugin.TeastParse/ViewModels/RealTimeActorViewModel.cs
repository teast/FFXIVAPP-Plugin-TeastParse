using System;
using System.Collections.Generic;
using System.ComponentModel;
using FFXIVAPP.Plugin.TeastParse.Actors;
using static Sharlayan.Core.Enums.Actor;

namespace FFXIVAPP.Plugin.TeastParse
{
    /// <summary>
    /// The idea with this view model is to unify the view for dps, dtps and hps
    /// </summary>
    public class RealTimeActorViewModel : ViewModelBase
    {
        private readonly RealTimeType _type;

        public int DPS { get; private set; }
        public int TotalDPS { get; private set; }
        public ulong Timeline { get; private set; }
        public ulong Total { get; private set; }
        public double PercentOfTimeline { get; private set; }
        public string Name { get; private set; }
        public Job Job { get; private set; }

        public IEnumerable<ActorActionModel> AllActions => _allActions ?? (_allActions = _allActionsFunc());

        private IEnumerable<ActorActionModel> _allActions;
        private Func<IEnumerable<ActorActionModel>> _allActionsFunc;

        public RealTimeActorViewModel(ActorModel actor, RealTimeType type, Func<IEnumerable<ActorActionModel>> allActions = null)
        {
            _type = type;
            _allActionsFunc = allActions ?? (() => new List<ActorActionModel>());

            Name = actor.Name;
            Job = actor.Job;

            switch (type)
            {
                case RealTimeType.DPS:
                    DPS = actor.DPS;
                    TotalDPS = actor.TotalDPS;
                    Timeline = actor.TimelineDamage;
                    Total = actor.TotalDamage;
                    PercentOfTimeline = actor.PercentOfTimelineDamage;
                    actor.PropertyChanged += HandleDPS;
                    break;
                case RealTimeType.DTPS:
                    DPS = actor.DTPS;
                    TotalDPS = actor.TotalDTPS;
                    Timeline = actor.TimelineDamageTaken;
                    Total = actor.TotalDamageTaken;
                    PercentOfTimeline = actor.PercentOfTimelineDamageTaken;
                    actor.PropertyChanged += HandleDTPS;
                    break;
                case RealTimeType.HPS:
                    DPS = actor.HPS;
                    TotalDPS = actor.TotalHPS;
                    Timeline = actor.TimelineHeal;
                    Total = actor.TotalHeal;
                    PercentOfTimeline = actor.PercentOfTimelineHeal;
                    actor.PropertyChanged += HandleHPS;
                    break;
            }
        }

        private void HandleDPS(object sender, PropertyChangedEventArgs args)
        {
            var actor = (ActorModel)sender;
            switch (args.PropertyName)
            {
                case nameof(ActorModel.DPS):
                    Set(() => DPS = actor.DPS, nameof(DPS));
                    break;
                case nameof(ActorModel.TotalDPS):
                    Set(() => TotalDPS = actor.TotalDPS, nameof(TotalDPS));
                    break;
                case nameof(ActorModel.TimelineDamage):
                    Set(() => Timeline = actor.GrandTimelineDamage, nameof(Timeline));
                    break;
                case nameof(ActorModel.TotalDamage):
                    Set(() => Total = actor.GrandTotalDamage, nameof(Total));
                    break;
                case nameof(ActorModel.PercentOfTimelineDamage):
                    Set(() => PercentOfTimeline = actor.PercentOfTimelineDamage, nameof(PercentOfTimeline));
                    break;
            }
        }

        private void HandleDTPS(object sender, PropertyChangedEventArgs args)
        {
            var actor = (ActorModel)sender;
            switch (args.PropertyName)
            {
                case nameof(ActorModel.DTPS):
                    Set(() => DPS = actor.DTPS, nameof(DPS));
                    break;
                case nameof(ActorModel.TotalDTPS):
                    Set(() => TotalDPS = actor.TotalDTPS, nameof(TotalDPS));
                    break;
                case nameof(ActorModel.TimelineDamageTaken):
                    Set(() => Timeline = actor.TimelineDamageTaken, nameof(Timeline));
                    break;
                case nameof(ActorModel.TotalDamageTaken):
                    Set(() => Total = actor.TotalDamageTaken, nameof(Total));
                    break;
                case nameof(ActorModel.PercentOfTimelineDamageTaken):
                    Set(() => PercentOfTimeline = actor.PercentOfTimelineDamageTaken, nameof(PercentOfTimeline));
                    break;
            }
        }

        private void HandleHPS(object sender, PropertyChangedEventArgs args)
        {
            var actor = (ActorModel)sender;
            switch (args.PropertyName)
            {
                case nameof(ActorModel.HPS):
                    Set(() => DPS = actor.HPS, nameof(DPS));
                    break;
                case nameof(ActorModel.TotalHPS):
                    Set(() => TotalDPS = actor.TotalHPS, nameof(TotalDPS));
                    break;
                case nameof(ActorModel.TimelineHeal):
                    Set(() => Timeline = actor.TimelineHeal, nameof(Timeline));
                    break;
                case nameof(ActorModel.TotalHeal):
                    Set(() => Total = actor.TotalHeal, nameof(Total));
                    break;
                case nameof(ActorModel.PercentOfTimelineHeal):
                    Set(() => PercentOfTimeline = actor.PercentOfTimelineHeal, nameof(PercentOfTimeline));
                    break;
            }
        }
    }

    public enum RealTimeType
    {
        DPS,
        DTPS,
        HPS
    }
}