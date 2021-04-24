using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FFXIVAPP.Plugin.TeastParse.Actors;

namespace FFXIVAPP.Plugin.TeastParse.RegularExpressions
{
    /// <summary>
    /// Helper class for grouping multiple regex based on subejcts and language
    /// </summary>
    internal partial class RegExDictionary
    {
        private readonly Dictionary<ActorType, List<RegExTypePair>> _regexs;

        /// <summary>
        /// Retrieve all regex grouped on <see cref="GameLanguageEnum" />
        /// </summary>
        public Dictionary<GameLanguageEnum, List<Regex>> Subjects
        {
            get
            {
                return _regexs
                    .SelectMany(g => g.Value)
                    .SelectMany(g => g.Items)
                    .GroupBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.Select(i => i.Value).ToList());
            }
        }

        /// <summary>
        /// Retrieve all <see cref="RegExTypePair" /> that matches <see cref="subject" />
        /// </summary>
        public List<RegExTypePair> this[ActorType subject]
        {
            get
            {
                return _regexs.ContainsKey(subject) ? _regexs[subject] : new List<RegExTypePair>();
            }
        }

        /// <summary>
        /// Initialize a new <see cref="RegExDictionary" /> with available <see cref="RegExTypePair" />
        /// </summary>
        /// <param name="items">all <see cref="RegExTypePair" /> that should be in this <see cref="RegExDictionary" /></param>
        public RegExDictionary(params RegExTypePair[] items)
        {
            _regexs = items.SelectMany(i => i.ActorTypes, (i, t) => new { Type = t, Items = i }).GroupBy(i => i.Type).ToDictionary(i => i.Key, i => i.Select(s => s.Items).ToList());
            //_regexs = items.GroupBy(i => i.ActorTypes).ToDictionary(i => i.Key, i => i.Select(s => s).ToList());
        }
    }

    /// <summary>
    /// Helper class for binding all regex strings regarding an chat line in all languages.
    /// </summary>
    internal class RegExTypePair
    {
        /// <summary>
        /// <see cref="ChatCodeSubject" /> that this specific regex should react to
        /// </summary>
        public List<ActorType> ActorTypes { get; }

        /// <summary>
        /// Pair of <see cref="GameLanguageEnum" /> and what <see cref="Regex" /> to use for it
        /// </summary>
        public Dictionary<GameLanguageEnum, Regex> Items { get; }

        /// <summary>
        /// Get <see cref="Regex" /> for wanted <see cref="language" />.
        /// </summary>
        public Regex this[GameLanguageEnum language] => Items[language];

        public RegExTypePair(params System.Tuple<GameLanguageEnum, string>[] pair)
            : this(Enum.GetValues(typeof(ActorType)).Cast<ActorType>().ToList(), pair)
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="RegExTypePair" />.
        /// </summary>
        /// <param name="actorType"></param>
        /// <param name="pair">Each language and its corresponding regex</param>
        public RegExTypePair(ActorType actorType, params System.Tuple<GameLanguageEnum, string>[] pair)
            : this(new List<ActorType> { actorType }, pair)
        {
        }

        public RegExTypePair(List<ActorType> actorTypes, params System.Tuple<GameLanguageEnum, string>[] pair)
        {
            ActorTypes = actorTypes;
            Items = pair.ToDictionary(p => p.Item1, p => new Regex(p.Item2, RegexOptions.Compiled | RegexOptions.ExplicitCapture));
        }
    }
}