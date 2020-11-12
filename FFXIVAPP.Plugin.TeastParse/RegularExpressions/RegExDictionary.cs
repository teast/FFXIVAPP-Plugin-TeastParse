using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FFXIVAPP.Plugin.TeastParse.RegularExpressions
{
    /// <summary>
    /// Helper class for grouping multiple regex based on subejcts and language
    /// </summary>
    internal partial class RegExDictionary
    {
        /// <summary>
        /// Contains all <see cref="RegExTypePair" /> grouped on their <see cref="ChatCodeSubject" />
        /// </summary>
        private readonly Dictionary<ChatCodeSubject, List<RegExTypePair>> _subjects;
        /// <summary>
        /// Contains all <see cref="RegExTypePair" /> that has no <see cref="ChatCodeSubject" />
        /// </summary>
        private readonly List<RegExTypePair> _subjectsAll;
        /// <summary>
        /// Contains all <see cref="RegExTypePair" /> grouped on their <see cref="ChatCodeDirection" />
        /// </summary>
        private readonly Dictionary<ChatCodeDirection, List<RegExTypePair>> _directions;

        /// <summary>
        /// Contains all <see cref="RegExTypePair" /> that has no <see cref="ChatCodeDirection" />
        /// </summary>
        private readonly List<RegExTypePair> _directionsAll;

        /// <summary>
        /// Retrieve all regex grouped on <see cref="GameLanguageEnum" />
        /// </summary>
        public Dictionary<GameLanguageEnum, List<Regex>> Subjects
        {
            get
            {
                return _subjectsAll
                    .SelectMany(g => g.Items)
                    .GroupBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.Select(i => i.Value).ToList());
            }
        }

        /* TODO: Not in used (yet?)
        public Dictionary<GameLanguageEnum, List<Regex>> Directions
        {
            get
            {
                return _directionsAll
                    .SelectMany(g => g.Items)
                    .GroupBy(g => g.Key)
                    .ToDictionary(g => g.Key, g => g.Select(i => i.Value).ToList());
            }
        }
        */

        /// <summary>
        /// Retrieve all <see cref="RegExTypePair" /> that matches <see cref="subject" />
        /// </summary>
        public List<RegExTypePair> this[ChatCodeSubject subject]
        {
            get
            {
                return _subjects.Keys.Where(k => k.HasFlag(subject))
                        .Select(k => _subjects[k])
                        .SelectMany(i => i)
                        .Concat(_subjectsAll).ToList();
            }
        }

        /* TODO: Not in used (yet?)
        public List<RegExTypePair> this[ChatCodeDirection direction]
        {
            get
            {
                return _directions.Keys.Where(k => k.HasFlag(direction))
                        .Select(k => _directions[k])
                        .SelectMany(i => i)
                        .Concat(_directionsAll).ToList();
            }
        }
        */

        /// <summary>
        /// Initialize a new <see cref="RegExDictionary" /> with available <see cref="RegExTypePair" />
        /// </summary>
        /// <param name="items">all <see cref="RegExTypePair" /> that should be in this <see cref="RegExDictionary" /></param>
        public RegExDictionary(params RegExTypePair[] items)
        {
            _subjects = items.GroupBy(i => i.Subject).ToDictionary(i => i.Key, i => i.Select(s => s).ToList());
            _subjectsAll = _subjects.Keys.Where(k => k.HasFlag(ChatCodeSubject.DontMatter)).Select(k => _subjects[k]).SelectMany(s => s).ToList();
            _directions = items.GroupBy(i => i.Direction).ToDictionary(i => i.Key, i => i.Select(s => s).ToList());
            _directionsAll = _directions.Keys.Where(k => k.HasFlag(ChatCodeDirection.DontMatter)).Select(k => _directions[k]).SelectMany(s => s).ToList();
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
        public ChatCodeSubject Subject { get; }

        /// <summary>
        /// <see cref="ChatCodeDirection" /> that this specific regex should react to
        /// </summary>
        public ChatCodeDirection Direction { get; }

        /// <summary>
        /// Pair of <see cref="GameLanguageEnum" /> and what <see cref="Regex" /> to use for it
        /// </summary>
        public Dictionary<GameLanguageEnum, Regex> Items { get; }

        /// <summary>
        /// Get <see cref="Regex" /> for wanted <see cref="language" />.
        /// </summary>
        public Regex this[GameLanguageEnum language] => Items[language];

        /// <summary>
        /// Initialize a new instance of <see cref="RegExTypePair" />.
        /// </summary>
        /// <param name="subject">What subject(s) this instance should react to. null == all subjects</param>
        /// <param name="direction">What direction(s) this instance should react to. null == all directions</param>
        /// <param name="pair">Each language and its corresponding regex</param>
        public RegExTypePair(ChatCodeSubject? subject = null, ChatCodeDirection? direction = null, params System.Tuple<GameLanguageEnum, string>[] pair)
        {
            Subject = subject ?? ChatCodeSubject.DontMatter;
            Direction = direction ?? ChatCodeDirection.DontMatter;
            Items = pair.ToDictionary(p => p.Item1, p => new Regex(p.Item2, RegexOptions.Compiled | RegexOptions.ExplicitCapture));
        }
    }
}