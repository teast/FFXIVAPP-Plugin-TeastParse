using System.Collections.Generic;
using System.Linq;
using FFXIVAPP.Plugin.TeastParse.Factories;

namespace FFXIVAPP.Plugin.TeastParse.Models
{
    /// <summary>
    /// Interface to fetching last used action for given <see cref="ChatCodeSubject" />
    /// </summary>
    internal interface IActionCollection
    {
        bool TryGet(ChatCodeSubject subject, out ActionSubject action);
        IActionFactory Factory { get; }
    }

    /// <summary>
    /// Collection of last used action based on <see cref="ChatCodeSubject" />
    /// </summary>
    internal class ActionCollection : IActionCollection
    {
        private SortedList<ulong, ActionSubject> _actions = new SortedList<ulong, ActionSubject>();
        private ulong _actionIndex = ulong.MaxValue;

        public IActionFactory Factory { get; }

        public ActionCollection(IActionFactory factory)
        {
            Factory = factory;
        }

        public bool TryGet(ChatCodeSubject subject, out ActionSubject action)
        {
            action = this[subject];
            return (!string.IsNullOrEmpty(action.Name) && !string.IsNullOrEmpty(action.Action));
        }

        public ActionSubject this[ChatCodeSubject subject]
        {
            get
            {
                subject = TranslateSubject(subject);
                var result = new List<KeyValuePair<string, string>>();

                foreach (var action in _actions)
                {
                    if (action.Value.Subject != subject)
                        continue;

                    return action.Value;
                }

                return new ActionSubject();
            }
            set
            {
                if (string.IsNullOrEmpty(value.Name))
                    return;

                // Make sure we translate the subject
                value = new ActionSubject(TranslateSubject(value.Subject), value.Name, value.Action);

                // Make sure to reindex if we need to rewind from 0 to ulong.maxValue
                if (_actionIndex == 0)
                {
                    // TODO: Not 100% sure that the new key allready exist and gets overwritten (and then deleted)
                    var _newIndex = ulong.MaxValue;
                    foreach (var key in _actions.Keys.OrderByDescending(l => l).ToList())
                    {
                        _actions[_newIndex] = _actions[key];
                        _actions.Remove(key);
                        _newIndex--;
                    }

                    _actionIndex = _newIndex;

                }

                var ids = _actions.Where(a => a.Value.Name == value.Name).Select(a => a.Key);
                ids.ToList().ForEach(key => _actions.Remove(key));
                _actions[_actionIndex] = value;
                _actionIndex--;
            }
        }

        /// <summary>
        /// Some subjects are equal to each other when it comes to actions
        /// </summary>
        private ChatCodeSubject TranslateSubject(ChatCodeSubject subject)
        {
            if (subject == ChatCodeSubject.UnEngaged)
                return ChatCodeSubject.Engaged;
            return subject;
        }
    }
}