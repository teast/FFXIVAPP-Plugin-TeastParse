using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FFXIVAPP.Common.Core;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.Models;
using FFXIVAPP.Plugin.TeastParse.RegularExpressions;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using NLog;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse.ChatParse
{
    internal class CureParse : BaseParse
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

        public CureParse(List<ChatCodes> codes, IActorModelCollection actors, ITimelineCollection timelines, IRepository repository) : base(repository)
        {
            _actors = actors;
            _timelines = timelines;
            _repository = repository;

            Codes = codes.Where(code => code.Type == ChatcodeType.Cure || code.Type == ChatcodeType.Actions).ToList();
        }

        public override void Handle(ulong code, ChatLogItem item)
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

            if (activeCode.Type == ChatcodeType.Cure)
                HandleCure(activeCode, group, item);
            else if (activeCode.Type == ChatcodeType.Actions)
                HandleAction(activeCode, group, item);
        }

        private void HandleCure(ChatCodes activeCode, Group group, ChatLogItem item)
        {
            var isMatch = false;
            Match match = null;
            foreach (var regex in _matcher[activeCode.Type][group.Subject])
            {
                match = regex[Constants.Language].Match(item.Line);
                if (!match.Success)
                    continue;

                isMatch = true;
                break;
            }

            if (!isMatch)
            {
                Logging.Log(Logger, $"No match for Cure in {nameof(CureParse)} and chat line \"{item.Line}\"");
                return;
            }

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
        private void HandleAction(ChatCodes activeCode, Group group, ChatLogItem item)
        {
            var isMatch = false;
            Match match = null;
            foreach (var regex in _matcher[activeCode.Type][group.Subject])
            {
                match = regex[Constants.Language].Match(item.Line);
                if (!match.Success)
                    continue;

                isMatch = true;
                break;
            }

            if (!isMatch)
            {
                if (!_toIgnore.Subjects.ContainsKey(Constants.Language) || !_toIgnore.Subjects[Constants.Language].Any(r => r.IsMatch(item.Line)))
                    Logging.Log(Logger, $"No match for Action in {nameof(CureParse)} and chat line \"{item.Line}\"");
                return;
            }

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

        private string CleanName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return name;

            if (Constants.The.ContainsKey(Constants.Language))
            {
                // Some pets (like The Automaton Queen) has The at start of their name.. so lets try
                // and filter that away... condition are: multiple spaces and starts with "The"
                foreach (var t in Constants.The[Constants.Language])
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
        /// Lines that are known but not interested for us.
        /// </summary>
        /// <remarks>
        /// This only exist because we output in the log if we finds an unknown line format in case we missed to add
        /// an specific pattern.
        /// </remarks>
        private readonly RegExDictionary _toIgnore = new RegExDictionary(
            new RegExTypePair(null, null, Tuple.Create(GameLanguage.English, @"^(?<source>You|.+) readies (?<action>.+)\.$")),
            new RegExTypePair(null, null, Tuple.Create(GameLanguage.English, @"^(?<source>You|.+) (begin)s? casting (?<action>.+)\.$")),
            new RegExTypePair(null, null, Tuple.Create(GameLanguage.English, @"^(?<source>You|.+) (cancel)s? (?<action>.+)\.$")),
            new RegExTypePair(null, null, Tuple.Create(GameLanguage.English, @"^(?<source>You|.+)('s|rs) (?<action>.+) is interrupted\.$")),
            new RegExTypePair(null, null, Tuple.Create(GameLanguage.English, @"^ ⇒ (?<source>You|.+)('s|rs) enmity increases\.$")),
            new RegExTypePair(null, null, Tuple.Create(GameLanguage.English, @"^(?<source>You|.+) ready Teleport.$")),
            // This one can show up from enemies and therefore I have it here too.
            new RegExTypePair(null, null, Tuple.Create(GameLanguage.English, @"^(?<source>You|.+) (use|cast)s? (?<action>.+)\.$"))
        );

        private readonly static ChatCodeSubject _playerSubject = ChatCodeSubject.Alliance | ChatCodeSubject.Party | ChatCodeSubject.You |
                                                          ChatCodeSubject.Pet | ChatCodeSubject.PetAlliance | ChatCodeSubject.PetParty |
                                                          ChatCodeSubject.PetOther | ChatCodeSubject.Other | ChatCodeSubject.NPC |
                                                          ChatCodeSubject.Other | ChatCodeSubject.Unknown;

        /// <summary>
        /// All known patterns for each chat group and language
        /// </summary>
        private readonly Dictionary<ChatcodeType, RegExDictionary> _matcher = new Dictionary<ChatcodeType, RegExDictionary>
        {
            {ChatcodeType.Actions, new RegExDictionary(
                // Player actions
                new RegExTypePair(_playerSubject, null,
                    Tuple.Create(GameLanguage.German, @"^(?<source>Du|.+) (setzt (?<action>.+) ein|wirks?t (?<action>.+))\.$"),
                    Tuple.Create(GameLanguage.English, @"^(?<source>You|.+) (use|cast)s? (?<action>.+)\.$"),
                    Tuple.Create(GameLanguage.France, @"^(?<source>Vous|.+) (utilise|lance)z? (?<action>.+)\.$"),
                    Tuple.Create(GameLanguage.Japanese, @"^(?<source>.+)の「(?<action>.+)」$"),
                    Tuple.Create(GameLanguage.Chinese, @"^:(?<source>You|.+)(发动了|咏唱了|正在咏唱|正在发动)“(?<action>.+)”。$"))
            )},

            {ChatcodeType.Cure, new RegExDictionary(
                new RegExTypePair(_playerSubject, null,
                    Tuple.Create(GameLanguage.German, @"^( ⇒ )?(?<crit>Kritischer Treffer ?! )?(D(u|einer|(i|e)r|ich|as|ie|en) )?(?<target>.+) regeneriers?t (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?(?<type>\w+)\.$"),
                    Tuple.Create(GameLanguage.English, @"( ⇒ )?(?<crit>Critical! )?((T|t)he )?(?<target>You|.+) (recover|absorb)?s? (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?(?<type>\w+)\.$"),
                    Tuple.Create(GameLanguage.France, @"^( ⇒ )?(?<crit>Critique ?! )?(?<target>Vous|.+) récup(é|è)rez? (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?(?<type>\w+)\.$"),
                    Tuple.Create(GameLanguage.Japanese, @"^( ⇒ )?(?<crit>クリティカル！ )?(?<target>.+)((に|は)、?)(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?(?<type>\w+)回復。$"),
                    Tuple.Create(GameLanguage.Chinese, @"^:( ⇒ )?(?<crit>暴击！ )?(?<target>You|.+)恢复了?(?<amount>\d+)?(\((?<modifier>.\d+)%\))?点(?<type>\w+)。$"))
            )}
        };
    }
}