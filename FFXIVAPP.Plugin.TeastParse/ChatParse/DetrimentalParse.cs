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
    internal class DetrimentalParse : BaseChatParse
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IActorModelCollection _actors;
        private readonly ITimelineCollection _timeline;
        private readonly IDetrimentalFactory _detrimentalFactory;

        /// <summary>
        /// Contains latest found actions (<see cref="ActionParse" /> for actual parsing of actions)
        /// </summary>
        private readonly IActionCollection _actions;

        protected override Dictionary<ChatcodeType, ChatcodeTypeHandler> Handlers { get; }

        protected override List<ChatCodes> Codes { get; }

        public DetrimentalParse(List<ChatCodes> codes, IActorModelCollection actors, ITimelineCollection timeline, IDetrimentalFactory detrimentalFactory, IActionCollection actions, IRepository repository) : base(repository)
        {
            _actors = actors;
            _timeline = timeline;
            _actions = actions;
            _detrimentalFactory = detrimentalFactory;
            Codes = codes.Where(c => c.Type == ChatcodeType.Detrimental).ToList();
            Handlers = new Dictionary<ChatcodeType, ChatcodeTypeHandler>
            {
                {ChatcodeType.Detrimental, _handleDetrimental}
            };
        }

        private void HandleDetrimental(ChatCodes activeCode, Group group, Match match, ChatLogItem item)
        {
            var (model, target) = ToModel(match, item, group);
            target?.Detrimentals?.Add(model);
            StoreDamage(model);
        }

        /// <summary>
        /// Converts given regex match to an <see cref="DamageModel" />
        /// </summary>
        /// <param name="r">regex match</param>
        /// <param name="item">the actual chat log item</param>
        /// <param name="group">chatcodes group</param>
        /// <returns>an <see cref="DamaModel" /> based on input parameters</returns>
        private (DetrimentalModel model, ActorModel target) ToModel(Match r, ChatLogItem item, Group group)
        {
            var target = r.Groups["target"].Value;
            var status = r.Groups["status"].Value;
            var code = item.Code;
            var source = string.Empty;
            var action = string.Empty;

            if (_actions.TryGet(group.Subject, out var la))
            {
                source = la.Name;
                action = la.Action;
            }

            target = CleanName(target);

            //var actorSource = string.IsNullOrEmpty(source) ? null : _actors.GetModel(source, group.Subject);
            var actorTarget = string.IsNullOrEmpty(target) ? null : _actors.GetModel(target, group.Direction, group.Subject);

            var model = _detrimentalFactory.GetModel(status, item.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), DateTime.UtcNow,
                                                    source, target, code, group.Direction.ToString(), group.Subject.ToString(),
                                                    _actions.Factory);

            return (model, actorTarget);
        }

        private ChatcodeTypeHandler _handleDetrimental => new ChatcodeTypeHandler(
            ChatcodeType.Detrimental,
            new RegExDictionary(
                RegExDictionary.DamagePlayerAction,
                RegExDictionary.DetrimentalPlayer
            ),
            HandleDetrimental
        );
    }
}