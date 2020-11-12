using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        /// A list of actions that have been used.
        /// </summary>
        /// <remarks>
        /// This list is used to fetch last action based on source/direction
        /// </remarks>
        private readonly ActionCollection _lastAction = new ActionCollection();

        private readonly IActorModelCollection _actors;
        private readonly ITimelineCollection _timelines;
        private readonly IRepository _repository;

        protected override List<ChatCodes> Codes { get; }

        protected override Dictionary<ChatcodeType, ChatcodeTypeHandler> Handlers { get; }

        public CureParse(List<ChatCodes> codes, IActorModelCollection actors, ITimelineCollection timelines, IRepository repository) : base(repository)
        {
            _actors = actors;
            _timelines = timelines;
            _repository = repository;

            Codes = codes.Where(code => code.Type == ChatcodeType.Cure || code.Type == ChatcodeType.Actions).ToList();
            Handlers = new Dictionary<ChatcodeType, ChatcodeTypeHandler>
            {
                { ChatcodeType.Actions, _handlerActions },
                { ChatcodeType.Cure, _handlerCures },
            };
        }

        private void HandleCure(ChatCodes activeCode, Group group, Match match, ChatLogItem item)
        {
            var (model, source, target) = ToModel(match, item, group);
            // Model can be null if this event was an +MP instead of HP.
            if (model == null)
                return;

            source?.UpdateStat(model);
            //target?.UpdateStat(model);
            _actors.AddToTotalCure(source, model);
            StoreCure(model);
        }

        /// <summary>
        /// Handle chat lines that are for an action
        /// </summary>
        /// <param name="activeCode">chat code</param>
        /// <param name="group">the chat codes group entity (good for source/direction enum)</param>
        /// <param name="item">the actual chat log item</param>
        private void HandleAction(ChatCodes activeCode, Group group, Match match, ChatLogItem item)
        {
            var source = match.Groups["source"].Value;
            var action = match.Groups["action"].Value;

            _lastAction[group.Subject] = new ActionSubject(group.Subject, source, action);
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

            var la = _lastAction[group.Subject];
            if (!string.IsNullOrEmpty(la.Name) && !string.IsNullOrEmpty(la.Action))
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

        private ChatcodeTypeHandler _handlerActions => new ChatcodeTypeHandler(
            ChatcodeType.Actions,
            new RegExDictionary(RegExDictionary.ActionsPlayer),
            HandleAction,
            new RegExDictionary(
            RegExDictionary.MiscReadiesAction,
            RegExDictionary.MiscBeginCasting,
            RegExDictionary.MiscCancelAction,
            RegExDictionary.MiscInterruptedAction,
            RegExDictionary.MiscEnmityIncrease,
            RegExDictionary.MiscReadyTeleport,
            RegExDictionary.MiscMount,
            RegExDictionary.MiscTargetOutOfRange,
            // This one can show up from enemies and therefore I have it here too.
            RegExDictionary.ActionsMonster
            )
        );

        private ChatcodeTypeHandler _handlerCures => new ChatcodeTypeHandler(
            ChatcodeType.Cure,
            new RegExDictionary(RegExDictionary.CurePlayer),
            HandleCure
        );
    }
}