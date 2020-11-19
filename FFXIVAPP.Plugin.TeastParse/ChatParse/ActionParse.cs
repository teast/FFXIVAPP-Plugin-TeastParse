using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FFXIVAPP.Plugin.TeastParse.Models;
using FFXIVAPP.Plugin.TeastParse.RegularExpressions;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse.ChatParse
{
    /// <summary>
    /// Handles all parsing regarding actions and damage
    /// </summary>
    internal class ActionParse : BaseChatParse, IActionCollection
    {
        /// <summary>
        /// A list of actions that have been used.
        /// </summary>
        /// <remarks>
        /// This list is used to fetch last action based on source/direction
        /// </remarks>
        private ActionCollection _lastAction = new ActionCollection();

        /// <summary>
        /// Contains all chat codes that relates to action and damage
        /// </summary>
        protected override List<ChatCodes> Codes { get; }

        /// <summary>
        /// All known chat patterns to find
        /// </summary>
        protected override Dictionary<ChatcodeType, ChatcodeTypeHandler> Handlers { get; }

        public ActionParse(List<ChatCodes> codes, IRepository repository) : base(repository)
        {
            Codes = codes.Where(c => c.Type == ChatcodeType.Actions).ToList();
            Handlers = new Dictionary<ChatcodeType, ChatcodeTypeHandler>
            {
                { ChatcodeType.Actions, _handleActions }
            };
        }

        public bool TryGet(ChatCodeSubject subject, out ActionSubject action) => _lastAction.TryGet(subject, out action);

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
    }
}