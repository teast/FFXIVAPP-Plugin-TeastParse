using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.Factories;
using FFXIVAPP.Plugin.TeastParse.Models;
using FFXIVAPP.Plugin.TeastParse.RegularExpressions;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using NLog;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse.ChatParse
{
    internal class BeneficialParse : BaseChatParse
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IActorModelCollection _actors;
        private readonly ITimelineCollection _timeline;
        private readonly IBeneficialFactory _BeneficialFactory;

        /// <summary>
        /// A list of actions that have been used.
        /// </summary>
        /// <remarks>
        /// This list is used to fetch last action based on source/direction
        /// </remarks>
        private ActionCollection _lastAction = new ActionCollection();

        protected override Dictionary<ChatcodeType, ChatcodeTypeHandler> Handlers { get; }

        protected override List<ChatCodes> Codes { get; }

        public BeneficialParse(List<ChatCodes> codes, IActorModelCollection actors, ITimelineCollection timeline, IBeneficialFactory BeneficialFactory, IRepository repository) : base(repository)
        {
            _actors = actors;
            _timeline = timeline;
            _BeneficialFactory = BeneficialFactory;
            Codes = codes.Where(c => c.Type == ChatcodeType.Actions || c.Type == ChatcodeType.Beneficial).ToList();
            Handlers = new Dictionary<ChatcodeType, ChatcodeTypeHandler>
            {
                {ChatcodeType.Actions, _handleActions},
                {ChatcodeType.Beneficial, _handleBeneficial}
            };
        }

        private void HandleBeneficial(ChatCodes activeCode, Group group, Match match, ChatLogItem item)
        {
            var (model, target) = ToModel(match, item, group);
            target?.Beneficials?.Add(model);
        }

        /// <summary>
        /// Converts given regex match to an <see ref="DamageModel" />
        /// </summary>
        /// <param name="r">regex match</param>
        /// <param name="item">the actual chat log item</param>
        /// <param name="group">chatcodes group</param>
        /// <returns>an <see ref="DamaModel" /> based on input parameters</returns>
        private (BeneficialModel model, ActorModel target) ToModel(Match r, ChatLogItem item, Group group)
        {
            var target = r.Groups["target"].Value;
            var status = r.Groups["status"].Value;
            var action = "";
            var source = "";
            var code = item.Code;

            var la = _lastAction[group.Subject];
            if (!string.IsNullOrEmpty(la.Name) && !string.IsNullOrEmpty(la.Action))
            {
                source = la.Name;
                action = la.Action;
            }

            source = CleanName(source);
            target = CleanName(target);

            //var actorSource = string.IsNullOrEmpty(source) ? null : _actors.GetModel(source, group.Subject);
            var actorTarget = string.IsNullOrEmpty(target) ? null : _actors.GetModel(target, group.Direction, group.Subject);

            var model = _BeneficialFactory.GetModel(status, item.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.UtcNow,
                                                    source, target, code, group.Direction.ToString(), group.Subject.ToString());

            return (model, actorTarget);
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

        private ChatcodeTypeHandler _handleActions => new ChatcodeTypeHandler(
            ChatcodeType.Actions,
            new RegExDictionary(
                RegExDictionary.ActionsPlayer,
                RegExDictionary.ActionsMonster
            ),
            HandleAction,
            new RegExDictionary(
                RegExDictionary.MiscReadiesAction,
                RegExDictionary.MiscBeginCasting,
                RegExDictionary.MiscCancelAction,
                RegExDictionary.MiscInterruptedAction,
                RegExDictionary.MiscEnmityIncrease,
                RegExDictionary.MiscReadyTeleport,
                RegExDictionary.MiscMount,
                RegExDictionary.MiscTargetOutOfRange
            )
        );
        private ChatcodeTypeHandler _handleBeneficial => new ChatcodeTypeHandler(
            ChatcodeType.Beneficial,
            new RegExDictionary(
                RegExDictionary.DamagePlayerAction,
                RegExDictionary.BeneficialPlayer
            ),
            HandleBeneficial
        );
    }
}