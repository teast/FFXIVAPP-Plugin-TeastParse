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
    internal interface IActionFactory
    {
        ActionModel GetAction(string name, ActorModel actor = null);
    }

    internal class ActionFactory : IActionFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly List<ActionRaw> _actionDb;

        public ActionFactory(string actionJson)
        {
            _actionDb = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ActionRaw>>(actionJson);
        }

        public ActionModel GetAction(string name, ActorModel actor = null)
        {
            try
            {
                var action = _actionDb.FirstOrDefault(_ => _.Names[Constants.GameLanguage.ToString()] == name);
                if (action == null)
                {
                    Logging.Log(Logger, $"Action with name {name} not found for game language {Constants.GameLanguage}.");
                    return new ActionModel(name, ActionCategory.Item);
                }

                var isCombo = !string.IsNullOrEmpty(action.Combo) && action.Combo == actor.Weaponskill;
                var potency = GetPotency(action, isCombo);

                return new ActionModel(name, (ActionCategory)Enum.Parse(typeof(ActionCategory), action.Category), potency, isCombo);
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, new LogItem($"Unhandled exception in {nameof(ActionFactory)}.{nameof(GetAction)}", ex, true));
            }

            return new ActionModel(name, ActionCategory.Item);
        }

        private int GetPotency(ActionRaw action, bool isCombo) => action.Potency
                            .Where(_ => _.Combo == isCombo)
                            .Select(_ => _.Value)
                            .Sum() / action.Potency.Count;

        private class ActionRaw
        {
            public int Id { get; }
            public Dictionary<string, string> Names { get; }
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

            public ActionRaw(
                int id,
                string category,
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
                bool isPlayer
            )
            {
                Id = id;
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
            }
        }

        private struct Potency
        {
            public string Text { get; }
            public int Value { get; }
            public bool Combo { get; }
            public string Raw { get; }

            public Potency(string text, int value, bool combo, string raw)
            {
                Text = text;
                Value = value;
                Combo = combo;
                Raw = raw;
            }
        }
    }
}