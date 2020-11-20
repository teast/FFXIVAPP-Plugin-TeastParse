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
        /// <summary>
        /// All known actors.
        /// </summary>
        private readonly IActorModelCollection _actors;
        private readonly ITimelineCollection _timelines;

        /// <summary>
        /// Contains latest found actions (<see cref="ActionParse" /> for actual parsing of actions)
        /// </summary>
        private readonly IActionCollection _actions;

        /// <summary>
        /// Contains all chat codes that relates to action and damage
        /// </summary>
        protected override List<ChatCodes> Codes { get; }

        /// <summary>
        /// All known chat patterns to find
        /// </summary>
        protected override Dictionary<ChatcodeType, ChatcodeTypeHandler> Handlers { get; }

        /// <summary>
        /// Initialize a new instance of <see cref="BattleParse" />.
        /// </summary>
        /// <param name="codes">all known chat codes</param>
        /// <param name="actors">actor handler</param>
        /// <param name="repository">database repository</param>
        public BattleParse(List<ChatCodes> codes, IActorModelCollection actors, ITimelineCollection timelines, IActionCollection actions, IRepository repository) : base(repository)
        {
            _actors = actors;
            _timelines = timelines;
            _actions = actions;
            Codes = codes.Where(c => c.Type == ChatcodeType.Damage).ToList();
            Handlers = new Dictionary<ChatcodeType, ChatcodeTypeHandler>
            {
                {ChatcodeType.Damage, _handleDamage}
            };
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
        /// Converts given regex match to an <see cref="DamageModel" />
        /// </summary>
        /// <param name="r">regex match</param>
        /// <param name="item">the actual chat log item</param>
        /// <param name="group">chatcodes group</param>
        /// <returns>an <see cref="DamaModel" /> based on input parameters</returns>
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
                if (_actions.TryGet(group.Subject, out var la))
                {
                    source = la.Name;
                    action = la.Action;
                }
            }

            source = CleanName(source);
            target = CleanName(target);

            var actorSource = string.IsNullOrEmpty(source) ? null : _actors.GetModel(source, group.Subject);
            var actorTarget = string.IsNullOrEmpty(target) ? null : _actors.GetModel(target, group.Direction, group.Subject);

            var model = new DamageModel(
                occurredUtc: DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                timestamp: item.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"),
                source: source,
                target: target,
                damage: string.IsNullOrWhiteSpace(amount) ? 0 : Convert.ToUInt64(amount),
                modifier: modifier,
                action: _actions.Factory.GetAction(action, actorSource),
                critical: !string.IsNullOrWhiteSpace(crit),
                directHit: !string.IsNullOrWhiteSpace(direct),
                blocked: !string.IsNullOrWhiteSpace(block),
                parried: !string.IsNullOrWhiteSpace(parry),
                initDmg: null,
                endTimeUtc: null,
                subject: group.Subject.ToString(),
                direction: group.Direction.ToString(),
                chatCode: code,
                isDetrimental: false
            );

            return (model, actorSource, actorTarget);
        }

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