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
    /// <summary>
    /// Handles all parsing regarding actions and damage
    /// </summary>
    internal class BattleParse : BaseParse
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
        }

        /// <summary>
        /// Parsers chat lines that has something to do with actions and/or damage
        /// </summary>
        /// <param name="code">chat code</param>
        /// <param name="item">actual chat item</param>
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

            if (activeCode.Type == ChatcodeType.Damage)
                HandleDamage(activeCode, group, item);
            else if (activeCode.Type == ChatcodeType.Actions)
                HandleAction(activeCode, group, item);

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
                match = regex[Constants.GameLanguage].Match(item.Line);
                if (!match.Success)
                    continue;

                isMatch = true;
                break;
            }

            if (!isMatch && (!_toIgnore.Subjects.ContainsKey(Constants.GameLanguage) || !_toIgnore.Subjects[Constants.GameLanguage].Any(r => r.IsMatch(item.Line))))
            {
                Logging.Log(Logger, $"No match for Action in {nameof(BattleParse)} and chat line \"{item.Line}\"");
                return;
            }

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
        private void HandleDamage(ChatCodes activeCode, Group group, ChatLogItem item)
        {
            var isMatch = false;
            Match match = null;
            foreach (var regex in _matcher[activeCode.Type][group.Subject])
            {
                match = regex[Constants.GameLanguage].Match(item.Line);
                if (!match.Success)
                    continue;

                isMatch = true;
                break;
            }

            if (!isMatch)
            {
                Logging.Log(Logger, $"No match for Damage in {nameof(BattleParse)} and chat line \"{item.Line}\"");
                return;
            }

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

            if (string.IsNullOrWhiteSpace(source))
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

            var actorSource = _actors.GetModel(source, group.Subject);
            var actorTarget = _actors.GetModel(target, group.Direction, group.Subject);

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

        private string CleanName(string name)
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

        private readonly static ChatCodeSubject _playerSubject = ChatCodeSubject.Alliance | ChatCodeSubject.Party | ChatCodeSubject.You |
                                                          ChatCodeSubject.Pet | ChatCodeSubject.PetAlliance | ChatCodeSubject.PetParty |
                                                          ChatCodeSubject.PetOther | ChatCodeSubject.Other | ChatCodeSubject.NPC |
                                                          ChatCodeSubject.Other | ChatCodeSubject.Unknown;
        private readonly static ChatCodeSubject _monsterSubject = ChatCodeSubject.Engaged | ChatCodeSubject.UnEngaged | ChatCodeSubject.Unknown;

        /// <summary>
        /// Lines that are known but not interested for us.
        /// </summary>
        /// <remarks>
        /// This only exist because we output in the log if we finds an unknown line format in case we missed to add
        /// an specific pattern.
        /// </remarks>
        private readonly RegExDictionary _toIgnore = new RegExDictionary(
            new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^(?<source>You|.+) readies (?<action>.+)\.$")),
            new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^(?<source>You|.+) (begin)s? casting (?<action>.+)\.$")),
            new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^(?<source>You|.+) (cancel)s? (?<action>.+)\.$")),
            new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^(?<source>You|.+)('s|rs) (?<action>.+) is interrupted\.$")),
            new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^ ⇒ (?<source>You|.+)('s|rs) enmity increases\.$")),
            new RegExTypePair(null, null, Tuple.Create(GameLanguageEnum.English, @"^(?<source>You|.+) ready Teleport.$"))
        );

        /// <summary>
        /// All known patterns for each chat group and language
        /// </summary>
        private readonly Dictionary<ChatcodeType, RegExDictionary> _matcher = new Dictionary<ChatcodeType, RegExDictionary>
        {
            {ChatcodeType.Actions, new RegExDictionary(
                // Player actions
                new RegExTypePair(_playerSubject, null,
                    Tuple.Create(GameLanguageEnum.German, @"^(?<source>Du|.+) (setzt (?<action>.+) ein|wirks?t (?<action>.+))\.$"),
                    Tuple.Create(GameLanguageEnum.English, @"^(?<source>You|.+) (use|cast)s? (?<action>.+)\.$"),
                    Tuple.Create(GameLanguageEnum.France, @"^(?<source>Vous|.+) (utilise|lance)z? (?<action>.+)\.$"),
                    Tuple.Create(GameLanguageEnum.Japanese, @"^(?<source>.+)の「(?<action>.+)」$"),
                    Tuple.Create(GameLanguageEnum.Chinese, @"^:(?<source>You|.+)(发动了|咏唱了|正在咏唱|正在发动)“(?<action>.+)”。$")),

                // Monster actions
                new RegExTypePair(_monsterSubject, null,
                    Tuple.Create(GameLanguageEnum.German, @"^(D(u|einer|(i|e)r|ich|as|ie|en) )?(?<source>.+) (setzt (?<action>.+) ein|wirks?t (?<action>.+))\.$"),
                    Tuple.Create(GameLanguageEnum.English, @"^((T|t)he )?(?<source>.+) (use|cast)s? (?<action>.+)\.$"),
                    Tuple.Create(GameLanguageEnum.France, @"^(L[aes] |[LEAD]')?(?<source>.+) (utilise|lance)z? (?<action>.+)\.$"),
                    Tuple.Create(GameLanguageEnum.Japanese, @"^(?<source>.+)の「(?<action>.+)」$"),
                    Tuple.Create(GameLanguageEnum.Chinese, @"^:(?<source>.+)(发动了|咏唱了|正在咏唱|正在发动)“(?<action>.+)”。$"))
            )},
            {ChatcodeType.Damage, new RegExDictionary(
                // Damage from actions
                new RegExTypePair(_playerSubject, null,
                    Tuple.Create(GameLanguageEnum.German, @"^ ⇒ (?<block>Geblockt! ?)?(?<parry>Pariert! ?)?(?<crit>Kritischer Treffer! ?)?(D(u|einer|(i|e)r|ich|as|ie|en) )?(?<target>.+) erleides?t (nur )?(?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?Punkte? (Schaden|reduziert)\.$"),
                    Tuple.Create(GameLanguageEnum.English, @"^ ⇒ (?<block>Blocked! )?(?<parry>Parried! )?(?<crit>Critical(?<direct> direct hit)?! )?(?<direct>Direct hit! )?((T|t)he )?(?<target>.+) takes? (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?damage\.$"),
                    Tuple.Create(GameLanguageEnum.France, @"^ ⇒ (?<parry>Parade ?! )?(?<block>Blocage ?! )?(?<crit>Critique ?! )?(L[aes] |[LEAD]')?(?<target>.+) subit (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?points? de dégâts?\.$"),
                    Tuple.Create(GameLanguageEnum.Japanese, @"^ ⇒ (?<crit>クリティカル！ )?(?<target>.+)((に|は)、?)(?<block>ブロックした！ )?(?<parry>受け流した！ )?(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?ダメージ。$"),
                    Tuple.Create(GameLanguageEnum.Chinese, @"^: ⇒ (?<crit>暴击！ )?(?<target>.+?)(?<block>招架住了！ )?(?<parry>格挡住了！ )?(受到(了)?)(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?点伤害。$")),

                // "DamageAuto"
                new RegExTypePair(_playerSubject, null,
                    Tuple.Create(GameLanguageEnum.German, @"^(?! ⇒)(?<block>Geblockt! ?)?(?<parry>Pariert! ?)?(?<crit>Kritischer Treffer! ?)?(?<source>Du|.+) triffs?t (d(u|einer|(i|e)r|ich|as|ie|en) )?(?<target>.+) und verursachs?t (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?Punkte? (Schaden|reduziert)\.$"),
                    Tuple.Create(GameLanguageEnum.English, @"^(?! ⇒)(?<block>Blocked! )?(?<parry>Parried! )?(?<crit>Critical(?<direct> direct hit)?! )?(?<direct>Direct hit! )?(?<source>You|.+) hits? ((T|t)he )?(?<target>.+) for (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?damage\.$"),
                    Tuple.Create(GameLanguageEnum.France, @"^(?! ⇒)(?<parry>Parade ?! )?(?<block>Blocage ?! )?(?<crit>Critique ?! )?(?<source>Vous|.+) infligez? \w+ (l[aes] |[lead]')?(?<target>.+) (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?points? de dégâts?\.$"),
                    Tuple.Create(GameLanguageEnum.Japanese, @"^(?<source>.+)の攻撃( ⇒ )?(?<crit>クリティカル！ )?(?<target>.+)((に|は)、?)(?<block>ブロックした！ )?(?<parry>受け流した！ )?(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?ダメージ。$"),
                    Tuple.Create(GameLanguageEnum.Chinese, @"^:(?<source>.+)发动攻击( ⇒ )?(?<crit>暴击！ )?(?<target>.+?)(?<block>招架住了！ )?(?<parry>格挡住了！ )?(受到(了)?)(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?点伤害。$")),

                // Monster damage from actions
                new RegExTypePair(_monsterSubject, null,
                    Tuple.Create(GameLanguageEnum.German, @"^ ⇒ (?<block>Geblockt! ?)?(?<parry>Pariert! ?)?(?<crit>Kritischer Treffer! ?)?(?<target>dich|.+)( erleides?t (nur )?|, aber der Schaden wird auf )(?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?Punkte? (Schaden|reduziert)\.$"),
                    Tuple.Create(GameLanguageEnum.English, @"^ ⇒ (?<block>Blocked! )?(?<parry>Parried! )?(?<crit>Critical! )?(?<target>You|.+) takes? (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?damage\.$"),
                    Tuple.Create(GameLanguageEnum.France, @"^ ⇒ (?<parry>Parade ?! )?(?<block>Blocage ?! )?(?<crit>Critique ?! )?(?<target>Vous|.+) subi(t|ssez?)? (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?points? de dégâts?\.$"),
                    Tuple.Create(GameLanguageEnum.Japanese, @"^ ⇒ (?<crit>クリティカル！ )?(?<target>.+)((に|は)、?)(?<block>ブロックした！ )?(?<parry>受け流した！ )?(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?ダメージ。$"),
                    Tuple.Create(GameLanguageEnum.Chinese, @"^: ⇒ (?<crit>暴击！ )?(?<target>.+?)(?<block>招架住了！ )?(?<parry>格挡住了！ )?(受到(了)?)(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?点伤害。$")),

                // "DamageAuto" for monster
                new RegExTypePair(_monsterSubject, null,
                    Tuple.Create(GameLanguageEnum.German, @"^(?! ⇒)(?<block>Geblockt! ?)?(?<parry>Pariert! ?)?(?<crit>Kritischer Treffer! ?)?(D(u|einer|(i|e)r|ich|as|ie|en) )?(?<source>.+) triffs?t (?<target>dich|.+)( und verursachs?t |, aber der Schaden wird auf )(?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?Punkte? (Schaden|reduziert)\.$"),
                    Tuple.Create(GameLanguageEnum.English, @"^(?! ⇒)(?<block>Blocked! )?(?<parry>Parried! )?(?<crit>Critical! )?((T|t)he )?(?<source>.+) hits? (?<target>you|.+) for (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?damage\.$"),
                    Tuple.Create(GameLanguageEnum.France, @"^(?! ⇒)(?<parry>Parade ?! )?(?<block>Blocage ?! )?(?<crit>Critique ?! )?(L[aes] |[LEAD]')?(?<source>.+) ((?<target>Vous|.+) infligez?|infligez? à (?<target>vous|.+)) (?<amount>\d+) ?(\((?<modifier>.\d+)%\) )?points? de dégâts?\.$"),
                    Tuple.Create(GameLanguageEnum.Japanese, @"^(?! ⇒)(?<source>.+)の攻撃( ⇒ )?(?<crit>クリティカル！ )?(?<target>.+)((に|は)、?)(?<block>ブロックした！ )?(?<parry>受け流した！ )?(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?ダメージ。$"),
                    Tuple.Create(GameLanguageEnum.Chinese, @"^:(?<source>.+)发动攻击( ⇒ )?(?<crit>暴击！ )?(?<target>.+?)(?<block>招架住了！ )?(?<parry>格挡住了！ )?(受到(了)?)(?<amount>\d+) ?(\((?<modifier>.\d+)%\) ?)?点伤害。$"))
            )}
        };
    }
}