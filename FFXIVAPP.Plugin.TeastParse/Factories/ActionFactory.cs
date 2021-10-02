using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVAPP.Common.Models;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.Models;
using NLog;

namespace FFXIVAPP.Plugin.TeastParse.Factories
{
    public interface IActionFactory
    {
        ActionModel GetAction(string name, bool isDetrimental = false, ActorModel actor = null);

        /// <summary>
        /// Some Detrimentals do not have an real action bound to them.
        /// </summary>
        ActionModel GetFakeAction(string actionName);
        bool ActionExist(string name);
    }

    internal class ActionFactory : IActionFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly List<ActionRaw> _actionDb;

        public ActionFactory(string actionJson)
        {
            _actionDb = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ActionRaw>>(actionJson);
        }

        public bool ActionExist(string name) => _actionDb.Any(_ => _.IsPvp == false && _.Names[Constants.GameLanguage.ToString()] == name);

        public ActionModel GetFakeAction(string actionName) => new ActionModel(actionName, actionName, ActionCategory.Special);

        public ActionModel GetAction(string name, bool isDetrimental = false, ActorModel actor = null)
        {
            try
            {
                // https://allaganstudies.akhmorning.com/
                if (string.IsNullOrEmpty(name))
                    return new ActionModel(name, name, ActionCategory.AutoAttack, 110);

                // TODO: Handle pvp
                var action = _actionDb.FirstOrDefault(_ => _.IsPvp == false && _.Names[Constants.GameLanguage.ToString()] == name);
                if (action == null)
                {
                    Logging.Log(Logger, $"Action with name {name} not found for game language {Constants.GameLanguage}.");
                    return new ActionModel(name, name, ActionCategory.AutoAttack, 100);
                }

                var isCombo = !string.IsNullOrEmpty(action.Combo) && MatchComboName(action.Combo, actor?.Weaponskill);
                var potency = GetPotency(action, isCombo, isDetrimental);
                var duration = action.Duration.FirstOrDefault().Value;
                return new ActionModel(name, action.Names["English"], (ActionCategory)Enum.Parse(typeof(ActionCategory), action.Category), potency, isCombo, duration, action.Icon);
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, new LogItem($"Unhandled exception in {nameof(ActionFactory)}.{nameof(GetAction)}", ex, true));
            }

            return new ActionModel(name, name, ActionCategory.AutoAttack, 100);
        }

        /// <summary>
        /// Some actions only has combo to the "non-enhanced" version. example is "Split Shot" that can also be called "Heated Split Shot"
        /// </summary>
        private bool MatchComboName(string actionComboName, string weaponskillName)
        {
            if (actionComboName == "Split Shot")
                return weaponskillName == actionComboName || weaponskillName == "Heated Split Shot";
            if (actionComboName == "Slug Shot")
                return weaponskillName == actionComboName || weaponskillName == "Heated Slug Shot";

            return actionComboName == weaponskillName;
        }

        private int GetPotency(ActionRaw action, bool isCombo, bool isDetrimental)
        {
            // Some actions do not have "IsDetrimental" set and if that is the case then do not check for detrimental
            isDetrimental = isDetrimental && action.Potency.Any(_ => _.IsDetrimental);

            return action.Potency
                .Where(_ => _.Combo == isCombo && _.IsDetrimental == isDetrimental)
                .Select(_ => _.Value)
                .Sum() / Math.Max(action.Potency.Where(_ => _.Combo == isCombo && _.IsDetrimental == isDetrimental).Count(), 1);
        }

        private class ActionRaw
        {
            public int Id { get; }
            public Dictionary<string, string> Names { get; }
            public string Icon { get; }
            public string Category { get; }
            public int PrimaryCostType { get; }
            public int PrimaryValue { get; }
            public int SecondaryCostType { get; }
            public string SecondaryValue { get; }
            public string Combo { get; }
            public int CastTime { get; }
            public int RecastTime { get; }
            public bool IsPvp { get; }
            public string Jobs { get; }
            public string Descriptions { get; }
            public List<Potency> Potency { get; }
            public bool IsPlayer { get; set; }
            public List<Potency> Duration { get; }

            public ActionRaw(
                int id,
                string category,
                string icon,
                int primaryCostType,
                int primaryValue,
                int secondaryCostType,
                string secondaryValue,
                string combo,
                int castTime,
                int recastTime,
                Dictionary<string, string> names,
                string descriptions,
                bool isPvp,
                string jobs,
                List<Potency> potency,
                bool isPlayer,
                List<Potency> duration
            )
            {
                Id = id;
                Icon = icon;
                Category = category;
                PrimaryCostType = primaryCostType;
                PrimaryValue = primaryValue;
                SecondaryCostType = secondaryCostType;
                SecondaryValue = secondaryValue;
                Combo = combo;
                CastTime = castTime;
                RecastTime = recastTime;
                Names = names;
                Descriptions = descriptions;
                IsPvp = isPvp;
                Jobs = jobs;
                Potency = potency;
                IsPlayer = isPlayer;
                Duration = duration;
            }
        }

        private struct Potency
        {
            public string Text { get; }
            public int Value { get; }
            public bool Combo { get; }
            public bool IsDetrimental { get; }
            public string Raw { get; }

            public Potency(string text, int value, bool combo, bool isDetrimental, string raw)
            {
                Text = text;
                Value = value;
                Combo = combo;
                IsDetrimental = isDetrimental;
                Raw = raw;
            }

            public override string ToString()
            {
                return $"[{Text}: {Value}, IsDetrimental: {IsDetrimental}, Combo: {Combo}]";
            }
        }
    }
}