using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using FFXIVAPP.Common.Core;
using FFXIVAPP.Plugin.TeastParse.Models;
using FFXIVAPP.Plugin.TeastParse.RegularExpressions;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using NLog;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse.ChatParse
{
    /// <summary>
    /// Parsing all chat codes that can generate a new timeline
    /// </summary>
    internal class Timeline : BaseParse
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        //private readonly List<string> _criticalEngagement;
        //private readonly Timer _criticalTimer;

        private readonly RegExDictionary _matcherStart = new RegExDictionary(
                new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^(?<dungeon>.+) has begun\.$"))
        );
        private readonly RegExDictionary _matcherEnd = new RegExDictionary(
                new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^(?<dungeon>.+) has ended\.$"))
        );

        /// <summary>
        /// Collection of all generated timelines
        /// </summary>
        private readonly ITimelineCollection _collection;

        protected override List<ChatCodes> Codes => new List<ChatCodes>
        {
            new ChatCodes("leave/enter", ChatcodeType.System, new [] {
                new Group("leave/enter", ChatCodeSubject.Unknown, ChatCodeDirection.Unknown, new [] {
                    new Code(Convert.ToUInt64("0839", 16), "Enter/Leave trial"),
                })
            })
        };

        public Timeline(ITimelineCollection collection, IRepository repository) : base(repository)
        {
            _collection = collection;
            //_criticalEngagement = new List<string>();
            //_criticalTimer = new Timer(5*60);
            var first = new TimelineModel("#Start", DateTime.UtcNow);
            _collection.Add(first);
            StoreTimeline(first);
        }

        public override void Handle(ulong code, ChatLogItem item)
        {
/*
TODO: The new Critical engagement has following two interesting lines to follow when you start it:
        The second like is that the user have registered and can press commense.


[0839] "You have registered to join the following duty:Kill It with Fire"
2020-10-17 12:40:26.977 +02:00 [INF] Chat: 2020-10-17 12:40:48 [0039] "You have joined the critical engagement, Kill It with Fire. Access the Resistance Recruitment menu to join the fray!"

This is how it looks if you do not commence. it happens ~3-5minutes after the 2nd line above...:
2020-10-17 13:09:24.513 +02:00 [INF] Chat: 2020-10-17 13:09:36 [08B0] "You lose the effect of Marching Orders: Kill It with Fire."
*/

            if (!HandleStart(code, item))
                HandleEnd(code, item);
            //if (!HandleEnd(code, item))
            //    Log.Fatal($"{nameof(Timeline)}.{nameof(Handle)}: Unknown line: [{item.Code}] \"{item.Line}\"");
        }

        private bool HandleStart(ulong code, ChatLogItem item)
        {
            var isMatch = false;
            Match match = null;
            foreach (var regex in _matcherStart.Subjects[Constants.GameLanguage])
            {
                match = regex.Match(item.Line);
                if (!match.Success)
                    continue;

                isMatch = true;
                break;
            }

            if (!isMatch)
                return false;

            var dungeon = match.Groups["dungeon"].Value;

            if (string.IsNullOrWhiteSpace(dungeon))
                return false;

            var timeline = new TimelineModel(dungeon, DateTime.UtcNow);
            StoreTimeline(timeline);
            _collection.Add(timeline);
            return true;
        }

        private bool HandleEnd(ulong code, ChatLogItem item)
        {
            var isMatch = false;
            Match match = null;
            foreach (var regex in _matcherEnd.Subjects[Constants.GameLanguage])
            {
                match = regex.Match(item.Line);
                if (!match.Success)
                    continue;

                isMatch = true;
                break;
            }

            if (!isMatch)
                return false;

            var dungeon = match.Groups["dungeon"].Value;

            if (string.IsNullOrWhiteSpace(dungeon))
                return false;

            var timeline = _collection.Close(dungeon);
            CloseTimeline(dungeon, timeline.EndUtc.Value);
            return true;
        }
    }
}