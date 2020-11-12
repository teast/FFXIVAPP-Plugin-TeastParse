using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.Plugin.TeastParse.RegularExpressions;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using NLog;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse.ChatParse
{
    /// <summary>
    /// Handler callback when there is a match from <see cref="ChatcodeTypeHandler" />
    /// </summary>
    /// <param name="activeCode">The ChatCode that matched one of the wanted code numbers</param>
    /// <param name="group">ChatCode Group that the ChatCode belongs to</param>
    /// <param name="match">Regex <see cref="Match" /> result</param>
    /// <param name="item">actual chat line from FFXIV</param>
    internal delegate void ChatcodeHandler(ChatCodes activeCode, Group group, Match match, ChatLogItem item);
    
    /// <summary>
    /// Represents an combination of <see cref="ChatcodeType" />, <see cref="RegExDictionary" /> and <see cref="ChatcodeHandler" />
    /// </summary>
    internal class ChatcodeTypeHandler
    {
        /// <summary>
        /// What <see cref="ChatcodeType" /> to "listen" to
        /// </summary>
        public ChatcodeType ChatType { get; }

        /// <summary>
        /// <see cref="RegExDictionary" /> for matching actual chat line
        /// </summary>
        public RegExDictionary RegEx { get; }

        /// <summary>
        /// If no match was found check this before logging warning
        /// </summary>
        public RegExDictionary Ignore { get; }

        /// <summary>
        /// Actual handler to call if we have a match
        /// </summary>
        public ChatcodeHandler Handler { get; }

        /// <summary>
        /// Initialize a new instance of <see cref="ChatcodeTypeHandler" />.
        /// </summary>
        /// <param name="chatType">What <see cref="ChatcodeType" /> to "listen" to</param>
        /// <param name="regEx"><see cref="RegExDictionary" /> for matching actual chat line</param>
        /// <param name="handler">Actual handler to call if we have a match</param>
        /// <param name="ignore">If no match was found check this before logging warning</param>
        public ChatcodeTypeHandler(ChatcodeType chatType, RegExDictionary regEx, ChatcodeHandler handler, RegExDictionary ignore = null)
        {
            ChatType = chatType;
            RegEx = regEx;
            Handler = handler;
            Ignore = ignore ?? new RegExDictionary();
        }
    }

    /// <summary>
    /// Handles parsing of chat code lines
    /// </summary>
    internal abstract class BaseChatParse : BaseParse
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        /// <summary>
        /// All handlers that are available
        /// </summary>
        protected abstract Dictionary<ChatcodeType, ChatcodeTypeHandler> Handlers { get; }

        /// <summary>
        /// Initialize a new instance of <see cref="BaseChatParse" />
        /// </summary>
        /// <param name="repository">Active repository to use</param>
        protected BaseChatParse(IRepository repository) : base(repository)
        {
        }

        /// <summary>
        /// All chat codes to handle
        /// </summary>
        protected abstract override List<ChatCodes> Codes { get; }

        /// <summary>
        /// Will find active chat code and then match it to any handler
        /// </summary>
        /// <param name="code"><see cref="ChatLogItem" /> chat code</param>
        /// <param name="item">actual FFXIV chat line</param>        
        public sealed override void Handle(ulong code, ChatLogItem item)
        {
            ChatCodes activeCode = null;
            Group group = null;

            foreach (var c in Codes)
            {
                foreach (var g in c.Groups)
                {
                    foreach (var cc in g.Codes)
                    {
                        if (cc.Key == code)
                        {
                            group = g;
                            activeCode = c;
                            break;
                        }
                    }

                    if (group != null)
                        break;
                }

                if (group != null)
                    break;
            }

            if (Handlers.ContainsKey(activeCode.Type))
                HandleActiveCode(activeCode, group, item);
        }

        /// <summary>
        /// Removes prefix "The" if it exists and make sure first character is upper case
        /// </summary>
        /// <param name="name">name to clean</param>
        /// <returns><see cref="name" /> cleaned</returns>
        protected string CleanName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            if (Constants.The.ContainsKey(Constants.GameLanguage))
            {
                // Some pets (like The Automaton Queen) has The at start of their name.. so lets try
                // and filter that away... condition are: multiple spaces and starts with "The"
                foreach (var t in Constants.The[Constants.GameLanguage])
                {
                    if (!name.StartsWith(t, StringComparison.InvariantCultureIgnoreCase))
                        continue;

                    if (name.Count(c => c == ' ') > 1)
                    {
                        name = name.Substring(t.Length).Trim();
                        break;
                    }
                }
            }

            return name.First().ToString().ToUpper() + name.Substring(1);
        }

        /// <summary>
        /// If we found an matching code and handler, then try and match handlers regex with it
        /// </summary>
        private void HandleActiveCode(ChatCodes activeCode, Group group, ChatLogItem item)
        {
            var isMatch = false;
            Match match = null;
            foreach (var regex in Handlers[activeCode.Type].RegEx[group.Subject])
            {
                match = regex[Constants.GameLanguage].Match(item.Line);
                if (!match.Success)
                    continue;

                isMatch = true;
                break;
            }

            if (!isMatch)
            {
                if (!Handlers[activeCode.Type].Ignore.Subjects.ContainsKey(Constants.GameLanguage) || !Handlers[activeCode.Type].Ignore.Subjects[Constants.GameLanguage].Any(r => r.IsMatch(item.Line)))
                    Logging.Log(Logger, $"No match for {activeCode.Type.ToString()} in {this.GetType().Name} and chat line \"{item.Line}\" ({item.Code})");

                return;
            }

            Handlers[activeCode.Type].Handler?.Invoke(activeCode, group, match, item);
        }
    }
}