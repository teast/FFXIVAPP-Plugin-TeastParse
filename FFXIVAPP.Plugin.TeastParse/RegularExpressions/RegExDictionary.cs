using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using FFXIVAPP.Common.Core;

namespace FFXIVAPP.Plugin.TeastParse.RegularExpressions
{
    public class RegExDictionary
    {
        private readonly Dictionary<ChatCodeSubject, List<RegExTypePair>> _subjects;
        private readonly List<RegExTypePair> _subjectsAll;
        private readonly Dictionary<ChatCodeDirection, List<RegExTypePair>> _directions;
        private readonly List<RegExTypePair> _directionsAll;

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

        public RegExDictionary(params RegExTypePair[] items)
        {
            _subjects = items.GroupBy(i => i.Subject).ToDictionary(i => i.Key, i => i.Select(s => s).ToList());
            _subjectsAll = _subjects.Keys.Where(k => k.HasFlag(ChatCodeSubject.DontMatter)).Select(k => _subjects[k]).SelectMany(s => s).ToList();
            _directions = items.GroupBy(i => i.Direction).ToDictionary(i => i.Key, i => i.Select(s => s).ToList());
            _directionsAll = _directions.Keys.Where(k => k.HasFlag(ChatCodeDirection.DontMatter)).Select(k => _directions[k]).SelectMany(s => s).ToList();
        }
    }

    public class RegExTypePair
    {
        public ChatCodeSubject Subject { get; }
        public ChatCodeDirection Direction { get; }
        public Dictionary<GameLanguageEnum, Regex> Items { get; }

        public Regex this[GameLanguageEnum language] => Items[language];

        public RegExTypePair(ChatCodeSubject? subject = null, ChatCodeDirection? direction = null, params System.Tuple<GameLanguageEnum, string>[] pair)
        {
            Subject = subject ?? ChatCodeSubject.DontMatter;
            Direction = direction ?? ChatCodeDirection.DontMatter;
            Items = pair.ToDictionary(p => p.Item1, p => new Regex(p.Item2, RegexOptions.Compiled | RegexOptions.ExplicitCapture));
        }
    }
}