using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.Models;
using FFXIVAPP.Plugin.TeastParse.RegularExpressions;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using NLog;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse.ChatParse
{
    internal class CureParse : BaseChatParse
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// Contains latest found actions (<see cref="ActionParse" /> for actual parsing of actions)
        /// </summary>
        private readonly IActionCollection _actions;

        private readonly IActorModelCollection _actors;
        private readonly ITimelineCollection _timelines;
        private readonly IRepository _repository;

        protected override List<ChatCodes> Codes { get; }

        protected override Dictionary<ChatcodeType, ChatcodeTypeHandler> Handlers { get; }

        public CureParse(List<ChatCodes> codes, IActorModelCollection actors, ITimelineCollection timelines, IActionCollection actions, IRepository repository) : base(repository)
        {
            _actors = actors;
            _timelines = timelines;
            _actions = actions;
            _repository = repository;

            Codes = codes.Where(code => code.Type == ChatcodeType.Cure).ToList();
            Handlers = new Dictionary<ChatcodeType, ChatcodeTypeHandler>
            {
                { ChatcodeType.Cure, _handlerCures },
            };
        }

        private void HandleCure(ChatCodes activeCode, Group group, Match match, ChatLogItem item)
        {
            CureModel model;
            ActorModel source;
            ActorModel target;

            // TODO: Refactor this, Maybe use KeyValuePair for Handlers instead of a dictionary so you can have multiple handles for a ChatcodeType
            if (RegExDictionary.CurePlayerAction.Items[Constants.GameLanguage].IsMatch(item.Line))
                (model, source, target) = ToModelFromBeneficial(match, item, group);
            else
                (model, source, target) = ToModel(match, item, group);

            // Model can be null if this event was an +MP instead of HP.
            if (model == null)
                return;

            source?.UpdateStat(model);
            //target?.UpdateStat(model);
            _actors.AddToTotalCure(source, model);
            StoreCure(model);
        }

        private (CureModel model, ActorModel source, ActorModel target) ToModelFromBeneficial(Match match, ChatLogItem item, Group group)
        {
            var action = match.Groups["action"].Value;
            var target = CleanName(match.Groups["target"].Value);
            var amount = match.Groups["amount"].Value;
            var code = item.Code;
            var actorTarget = _actors.GetModel(target, group.Direction, group.Subject);
            if (actorTarget == null)
            {
                Logging.Log(Logger, $"Could not find actor for target \"{target}\" so cannot get who casted the beneficial status for: [{code}] \"{item.Line}\"");
                return (null, null, null);
            }

            var beneficial = actorTarget.Beneficials.FirstOrDefault(b => b.ActionName == action);
            if (beneficial == null)
            {
                Logging.Log(Logger, $"Could not find beneficial that generates action \"{action}\" on actor target \"{target}\" so cannot get who casted the beneficial status for: [{code}] \"{item.Line}\"");
                return (null, null, null);
            }

            actorTarget.Beneficials.Remove(beneficial);
            var actorSource = _actors.GetModel(beneficial.Source, group.Subject);

            var model = new CureModel
            {
                OccurredUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Timestamp = item.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                Source = beneficial.Source,
                Target = target,
                Action = action,
                Cure = string.IsNullOrWhiteSpace(amount) ? 0 : Convert.ToUInt64(amount),
                Modifier = "",
                Critical = false, //!string.IsNullOrWhiteSpace(crit),
                Subject = group.Subject.ToString(),
                Direction = group.Direction.ToString(),
                ChatCode = code
                // TODO: Uncomment this to see what actions have been recored at this time: Actions = Newtonsoft.Json.JsonConvert.SerializeObject(_lastAction._actions)
            };

            return (model, actorSource, actorTarget);
        }

        private (CureModel model, ActorModel source, ActorModel target) ToModel(Match match, ChatLogItem item, Group group)
        {
            var source = "";
            var action = "";
            var target = match.Groups["target"].Value;
            var amount = match.Groups["amount"].Value;
            var modifier = match.Groups["modifier"].Value;
            var crit = match.Groups["crit"].Value;
            var type = match.Groups["type"].Value;
            var code = item.Code;

            if (type != "HP")
                return (null, null, null);

            if (_actions.TryGet(group.Subject, out var la))
            {
                source = la.Name;
                action = la.Action;
            }

            source = CleanName(source);
            target = CleanName(target);

            var actorSource = _actors.GetModel(source, group.Subject);
            var actorTarget = _actors.GetModel(target, group.Direction, group.Subject);

            var model = new CureModel
            {
                OccurredUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Timestamp = item.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                Source = source,
                Target = target,
                Action = action,
                Cure = string.IsNullOrWhiteSpace(amount) ? 0 : Convert.ToUInt64(amount),
                Modifier = modifier,
                Critical = !string.IsNullOrWhiteSpace(crit),
                Subject = group.Subject.ToString(),
                Direction = group.Direction.ToString(),
                ChatCode = code
                // TODO: Uncomment this to see what actions have been recored at this time: Actions = Newtonsoft.Json.JsonConvert.SerializeObject(_lastAction._actions)
            };

            return (model, actorSource, actorTarget);
        }

        private ChatcodeTypeHandler _handlerCures => new ChatcodeTypeHandler(
            ChatcodeType.Cure,
            new RegExDictionary(
                RegExDictionary.CurePlayer,
                RegExDictionary.CurePlayerAction
            ),
            HandleCure
        );
    }
}