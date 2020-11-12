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
    /// <summary>
    /// Handles all parsing regarding actions and damage
    /// </summary>
    internal class BattleParse : BaseChatParse
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// A list of actions that have been used.
        /// </summary>
        /// <remarks>
        /// This list is used to fetch last action based on source/direction
        /// </remarks>
        private ActionCollection _lastAction = new ActionCollection();

        /// <summary>
        /// All known actors.
        /// </summary>
        private readonly IActorModelCollection _actors;
        private readonly ITimelineCollection _timelines;

        /// <summary>
        /// Contains all chat codes that relates to action and damage
        /// </summary>
        protected override List<ChatCodes> Codes { get; }

        /// <summary>
        /// All known chat patterns to find
        /// </summary>
        protected override Dictionary<ChatcodeType, ChatcodeTypeHandler> Handlers { get; }

        /// <summary>
        /// Initialize a new instance of <see ref="BattleParse" />.
        /// </summary>
        /// <param name="codes">all known chat codes</param>
        /// <param name="actors">actor handler</param>
        /// <param name="repository">database repository</param>
        public BattleParse(List<ChatCodes> codes, IActorModelCollection actors, ITimelineCollection timelines, IRepository repository) : base(repository)
        {
            _actors = actors;
            _timelines = timelines;
            Codes = codes.Where(c => c.Type == ChatcodeType.Actions || c.Type == ChatcodeType.Damage).ToList();
            Handlers = new Dictionary<ChatcodeType, ChatcodeTypeHandler>
            {
                {ChatcodeType.Actions, _handleActions },
                {ChatcodeType.Damage, _handleDamage}
            };
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

        /// <summary>
        /// Handle chat lines that are for an damage
        /// </summary>
        /// <param name="activeCode">chat code</param>
        /// <param name="group">the chat codes group entity (good for source/direction enum)</param>
        /// <param name="item">the actual chat log item</param>
        private void HandleDamage(ChatCodes activeCode, Group group, Match match, ChatLogItem item)
        {
            var (model, source, target) = ToModel(match, item, group);
            source?.UpdateStat(model);
            target?.UpdateStat(model);
            _actors.AddToTotalDamage(source, model);
            _actors.AddToTotalDamageTaken(target, model);
            StoreDamage(model);
        }

        /// <summary>
        /// Converts given regex match to an <see ref="DamageModel" />
        /// </summary>
        /// <param name="r">regex match</param>
        /// <param name="item">the actual chat log item</param>
        /// <param name="group">chatcodes group</param>
        /// <returns>an <see ref="DamaModel" /> based on input parameters</returns>
        private (DamageModel model, ActorModel source, ActorModel target) ToModel(Match r, ChatLogItem item, Group group)
        {
            var source = r.Groups["source"].Value;
            var target = r.Groups["target"].Value;
            var amount = r.Groups["amount"].Value;
            var modifier = r.Groups["modifier"].Value;
            var crit = r.Groups["crit"].Value;
            var block = r.Groups["block"].Value;
            var parry = r.Groups["parry"].Value;
            var direct = r.Groups["direct"].Value;
            var action = "";
            var code = item.Code;
            var selfInflictedDamage = group.Subject == ChatCodeSubject.You && group.Direction == ChatCodeDirection.Self;

            if (string.IsNullOrWhiteSpace(source) && !selfInflictedDamage)
            {
                var la = _lastAction[group.Subject];
                if (!string.IsNullOrEmpty(la.Name) && !string.IsNullOrEmpty(la.Action))
                {
                    source = la.Name;
                    action = la.Action;
                }
            }

            source = CleanName(source);
            target = CleanName(target);

            var actorSource = string.IsNullOrEmpty(source) ? null : _actors.GetModel(source, group.Subject);
            var actorTarget = string.IsNullOrEmpty(target) ? null : _actors.GetModel(target, group.Direction, group.Subject);

            var model = new DamageModel
            {
                OccurredUtc = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                Timestamp = item.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                Source = source,
                Target = target,
                Action = action,
                Damage = string.IsNullOrWhiteSpace(amount) ? 0 : Convert.ToUInt64(amount),
                Modifier = modifier,
                Critical = !string.IsNullOrWhiteSpace(crit),
                Blocked = !string.IsNullOrWhiteSpace(block),
                Parried = !string.IsNullOrWhiteSpace(parry),
                DirectHit = !string.IsNullOrWhiteSpace(direct),
                Subject = group.Subject.ToString(),
                Direction = group.Direction.ToString(),
                ChatCode = code
                // TODO: Uncomment this to see what actions have been recored at this time: Actions = Newtonsoft.Json.JsonConvert.SerializeObject(_lastAction._actions)
            };

            return (model, actorSource, actorTarget);
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

        private ChatcodeTypeHandler _handleDamage => new ChatcodeTypeHandler(
            ChatcodeType.Damage,
            new RegExDictionary(
                RegExDictionary.DamagePlayerAction,
                RegExDictionary.DamagePlayerAutoAttack,
                RegExDictionary.DamageMonsterAction,
                RegExDictionary.DamageMonsterAutoAttack
            ),
            HandleDamage);
    }
}