using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.Plugin.TeastParse.Events;
using FFXIVAPP.Plugin.TeastParse.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NLog;
using Sharlayan.Core;
using static Sharlayan.Core.Enums.Actor;

namespace FFXIVAPP.Plugin.TeastParse.Actors
{
    /// <summary>
    /// <see cref="ActorModel" /> is an representation of an <see cref="ActorItem" />.
    /// </summary>
    /// <remarks>
    /// Main purpose of <see cref="ActorModel" /> is to keep track of an <see cref="ActorItem" />
    /// that has been notice in any of the tracking events.
    /// This class will keep track of the total damage and other parser related data.
    /// </remarks>
    public class ActorModel : ViewModelBase
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Fields
        private bool _isParty;
        private DateTime _firstStart;
        private DateTime _timelineStart;
        private string _timeline;
        private bool _isAlliance;

        /// <summary>
        /// Keep track on total dmg/heal/taken for party/alliance
        /// </summary>
        private readonly ITotalStats _totalStats;

        private ulong _totalDamage = 0;
        private ulong _timelineDamage = 0;
        private int _dps;
        private int _totalDPS;
        private double _percentOfTimelineDamage;

        private ulong _totalDamageTaken = 0;
        private ulong _timelineDamageTaken = 0;
        private int _dtps;
        private int _totalDTPS;
        private double _percentOfTimelineDamageTaken;

        private ulong _totalHeal = 0;
        private ulong _timelineHeal = 0;
        private int _hps;
        private int _totalHPS;
        private double _percentOfTimelineHeal;
        private readonly Dictionary<Job, JobModel> _jobs;
        private JobModel _currentJob
        {
            get
            {
                if (_jobs.TryGetValue(Job, out var model))
                    return model;
                model = new JobModel(Job, _actorRaw.Level);
                _jobs.Add(model.Job, model);
                return model;
            }
        }

        private readonly ActorItem _actorRaw;
        #endregion

        #region Read-only Properties
        [JsonConverter(typeof(StringEnumConverter))]
        public ActorType ActorType { get; }
        public string Name { get; }
        public string Server { get; }

        public Coordinate Coordinate { get; }

        /// <summary>
        /// Contains current weaponskill
        /// </summary>
        public string Weaponskill { get; private set; }

        public Job Job => _actorRaw.Job;
        public int Level => _actorRaw.Level;
        #endregion

        #region Properties
        public List<ActorStatusModel> Beneficials { get; set; }
        public List<ActorStatusModel> Detrimentals { get; set; }

        public bool IsParty { get; set; }

        public bool IsAlliance { get; set; }

        public bool IsYou { get; set; }
        #endregion

        #region Damage Made
        /// <summary>
        /// Damage per second during current timeline
        /// </summary>
        public int DPS { get => _dps; set => Set(() => _dps = value); }

        /// <summary>
        /// Total Damage per seconds from first seen timeline
        /// </summary>
        public int TotalDPS { get => _totalDPS; set => Set(() => _totalDPS = value); }

        /// <summary>
        /// Total damage during current timeline
        /// </summary>
        public ulong TimelineDamage { get => _timelineDamage; set => Set(() => _timelineDamage = value); }

        /// <summary>
        /// Total damage from first seen timeline
        /// </summary>
        public ulong TotalDamage { get => _totalDamage; set => Set(() => _totalDamage = value); }

        /// <summary>
        /// Percent of total damage made by this actor from current timeline for given party/alliance
        /// </summary>
        public double PercentOfTimelineDamage { get => _percentOfTimelineDamage; set => Set(() => _percentOfTimelineDamage = value); }
        #endregion

        #region Damage Taken
        /// <summary>
        /// Damage Taken per second during current timeline
        /// </summary>
        public int DTPS { get => _dtps; set => Set(() => _dtps = value); }

        /// <summary>
        /// Total Damage Taken per seconds from first seen timeline
        /// </summary>
        public int TotalDTPS { get => _totalDTPS; set => Set(() => _totalDTPS = value); }

        /// <summary>
        /// Total damage Taken during current timeline
        /// </summary>
        public ulong TimelineDamageTaken { get => _timelineDamageTaken; set => Set(() => _timelineDamageTaken = value); }

        /// <summary>
        /// Total Damage Taken from first seen timeline
        /// </summary>
        public ulong TotalDamageTaken { get => _totalDamageTaken; set => Set(() => _totalDamageTaken = value); }

        /// <summary>
        /// Percent of total Damage Taken made by this actor from current timeline for given party/alliance
        /// </summary>
        public double PercentOfTimelineDamageTaken { get => _percentOfTimelineDamageTaken; set => Set(() => _percentOfTimelineDamageTaken = value); }
        #endregion

        #region Heal made
        /// <summary>
        /// Heal per second during current timeline
        /// </summary>
        public int HPS { get => _hps; set => Set(() => _hps = value); }

        /// <summary>
        /// Total Heal per seconds from first seen timeline
        /// </summary>
        public int TotalHPS { get => _totalHPS; set => Set(() => _totalHPS = value); }

        /// <summary>
        /// Total heal during current timeline
        /// </summary>
        public ulong TimelineHeal { get => _timelineHeal; set => Set(() => _timelineHeal = value); }

        /// <summary>
        /// Total Heal from first seen timeline
        /// </summary>
        public ulong TotalHeal { get => _totalHeal; set => Set(() => _totalHeal = value); }

        /// <summary>
        /// Percent of total Heal made by this actor from current timeline for given party/alliance
        /// </summary>
        public double PercentOfTimelineHeal { get => _percentOfTimelineHeal; set => Set(() => _percentOfTimelineHeal = value); }
        #endregion

        internal ActorModel(string name, ActorItem actorRaw, ActorType actorType, ITimelineCollection timeline, ITotalStats totalStats, bool isParty, bool isAlliance)
        {
            _actorRaw = actorRaw;
            _jobs = new Dictionary<Job, JobModel>
            {
                { Job, new JobModel(Job, actorRaw.Level) }
            };

            Beneficials = new List<ActorStatusModel>();
            Detrimentals = new List<ActorStatusModel>();
            Name = name;
            Server = ExtractServerName(name, actorRaw.Name);
            ActorType = actorType;
            Coordinate = actorRaw.Coordinate;
            _timeline = timeline.Current.Name;
            _firstStart = timeline.Current.StartUtc;
            _timelineStart = timeline.Current.StartUtc;
            _isParty = isParty;
            _isAlliance = isAlliance;
            _totalStats = totalStats;
            timeline.CurrentTimelineChange += OnTimelineChange;
        }

        /// <summary>
        /// There have been more damage registered, lets add them to this actor
        /// </summary>
        /// <param name="model">Information about current damage to add</param>
        /// <remarks>
        /// This method will update either "damage made" or "damage taken" depending on what "side" of the
        /// damage this actor was on.
        /// It will also update current DPS and DTPS
        /// </remarks>
        public void UpdateStat(DamageModel model)
        {
            if (model.Source == this.Name || (this.IsYou && model.Source.IsYou()))
            {
                TimelineDamage += model.Damage;
                TotalDamage += model.Damage;
                if (model.Action.Category == ActionCategory.Weaponskill)
                    Weaponskill = model.Action.Name;

                _currentJob.StoreDamageDetails(model);
                //                if (this.IsYou)
                //                    Logging.Log(Logger, _potencyDamage.Debug());
            }
            else if (model.Target == this.Name || (this.IsYou && model.Target.IsYou()))
            {
                TimelineDamageTaken += model.Damage;
                TotalDamageTaken += model.Damage;
            }

            UpdateDps();
        }

        private readonly List<DetrimentalDamageInfo> _detrimentalDamage = new List<DetrimentalDamageInfo>();
        private readonly List<DetrimentalDamageInfo> _detrimentalDamageTaken = new List<DetrimentalDamageInfo>();

        /// <summary>
        /// Should only be called with detrimentals that have an lastUtc set and is done
        /// </summary>
        internal void UpdateStat(DetrimentalModel model)
        {
            if (model.Action.Potency == 0)
                return;

            // TODO: Taking detrimental ticks every 3 seconds at the moment...
            var ticks = (model.LastUtc.Value - model.TimeUtc).TotalSeconds / 3;

            if (model.Source == this.Name || (this.IsYou && model.Source.IsYou()))
            {
                _detrimentalDamage.Add(new DetrimentalDamageInfo((int)ticks, model.Action.Potency, Job));
            }
            else if (model.Target == this.Name || (this.IsYou && model.Target.IsYou()))
            {
                _detrimentalDamageTaken.Add(new DetrimentalDamageInfo((int)ticks, model.Action.Potency, Job, model.Source));
            }

            UpdateDps();
        }

        private void UpdateDps()
        {
            var dDmg = CalculateDetrimentalDamage();
            var dTaken = CalculateDetrimentalDamageTaken();

            DPS = Convert.ToInt32((TimelineDamage + dDmg) / (ulong)Math.Max((DateTime.UtcNow - _timelineStart).TotalSeconds, 1));
            TotalDPS = Convert.ToInt32((TotalDamage + dDmg) / (ulong)Math.Max((DateTime.UtcNow - _firstStart).TotalSeconds, 1));
            DTPS = Convert.ToInt32((TimelineDamageTaken + dTaken) / (ulong)Math.Max((DateTime.UtcNow - _timelineStart).TotalSeconds, 1));
            TotalDTPS = Convert.ToInt32((TotalDamageTaken + dTaken) / (ulong)Math.Max((DateTime.UtcNow - _firstStart).TotalSeconds, 1));
        }

        private ulong CalculateDetrimentalDamage()
        {
            return Convert.ToUInt64(_detrimentalDamage.Select(detr => _jobs[detr.Job].DetrimentalDamage(detr.Ticks, detr.Potency)).Sum());
        }

        private ulong CalculateDetrimentalDamageTaken()
        {
            // TODO:
            if (_detrimentalDamageTaken.Count > 0)
                Logging.Log(Logger, $"{nameof(ActorModel)}.{nameof(CalculateDetrimentalDamageTaken)} Not sure how to do this. currently {_detrimentalDamageTaken.Count} registered detrimentals.");
            return 0;
        }

        /// <summary>
        /// There have been more heals registered, lets add them to this actor
        /// </summary>
        /// <param name="model">Information about current heal to add</param>
        /// <remarks>
        /// This method will only add the heal if this actor is the source of the heal event.
        /// It will also update HPS for this actor
        /// </remarks>
        public void UpdateStat(CureModel model)
        {
            if (model.Source != this.Name && (!this.IsYou || !model.Source.IsYou()))
                return;

            TimelineHeal += model.Cure;
            TotalHeal += model.Cure;

            HPS = Convert.ToInt32(TimelineHeal / (ulong)Math.Max((DateTime.UtcNow - _timelineStart).TotalSeconds, 1));
            TotalHPS = Convert.ToInt32(TotalHeal / (ulong)Math.Max((DateTime.UtcNow - _firstStart).TotalSeconds, 1));
        }

        private void OnTimelineChange(object sender, TimelineChangeEvent args)
        {
            _timeline = args.Next.Name;
            _timelineStart = args.Next.StartUtc;

            // New timeline, lets start counting from this point
            TimelineDamage = 0;
            DPS = 0;
            TimelineDamageTaken = 0;
            DTPS = 0;
            TimelineHeal = 0;
            HPS = 0;
            PercentOfTimelineDamage = 0;
            PercentOfTimelineDamageTaken = 0;
            PercentOfTimelineHeal = 0;
        }

        internal void TotalDmgUpdated()
        {
            if (_isParty)
                PercentOfTimelineDamage = (double)TimelineDamage / _totalStats.PartyTotalDamage;
            else
                PercentOfTimelineDamage = (double)TimelineDamage / _totalStats.AllianceTotalDamage;
        }

        internal void TotalDmgTakenUpdated()
        {
            if (_isParty)
                PercentOfTimelineDamageTaken = (double)TimelineDamageTaken / _totalStats.PartyTotalDamageTaken;
            else
                PercentOfTimelineDamageTaken = (double)TimelineDamageTaken / _totalStats.AllianceTotalDamageTaken;
        }

        internal void TotalCureUpdated()
        {
            if (_isParty)
                PercentOfTimelineHeal = (double)TimelineHeal / _totalStats.PartyTotalHeal;
            else
                PercentOfTimelineHeal = (double)TimelineHeal / _totalStats.AllianceTotalHeal;
        }

        private string ExtractServerName(string chatName, string actorName)
        {
            // TODO: Let the user configure what server the user is from and use that here
            if (chatName.Length == actorName.Length)
                return "";

            return chatName.Substring(actorName.Length);
        }

        private struct DetrimentalDamageInfo
        {
            public int Ticks { get; }
            public int Potency { get; }
            public Job Job { get; }
            public string Source { get; }

            public DetrimentalDamageInfo(int ticks, int potency, Job job, string source = null)
            {
                Ticks = ticks;
                Potency = potency;
                Job = job;
                Source = source;
            }
        }
    }

    public enum ActorType
    {
        Player,
        NPC,
        Monster
    }
}