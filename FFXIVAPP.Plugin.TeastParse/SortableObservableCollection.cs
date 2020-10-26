using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace FFXIVAPP.Plugin.TeastParse
{
    /// <summary>
    /// Add possibility to sort an observable collection
    /// </summary>
    public class SortableObservableCollection<T> : ObservableCollection<T>
    {
        private Func<T, object> _sortingSelector;
        private bool _descending;

        public SortableObservableCollection(Func<T, object> sort, bool descending = false) : base()
        {
            _sortingSelector = sort;
            _descending = descending;
        }

        public SortableObservableCollection(Func<T, object> sort, bool descending, IEnumerable<T> list) : base(list)
        {
            _sortingSelector = sort;
            _descending = descending;
            DoSort();
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
            if (_sortingSelector == null
                || e.Action == NotifyCollectionChangedAction.Remove
                || e.Action == NotifyCollectionChangedAction.Reset)
                return;

            DoSort();
        }

        public SortableObservableCollection<T> Sort(Func<T, object> sortSelector, bool descending)
        {
            _sortingSelector = sortSelector;
            _descending = descending;
            DoSort();

            return this;
        }

        public void DoSort()
        {
            var query = this
              .Select((item, index) => (Item: item, Index: index));
            query = _descending == false
              ? query.OrderBy(tuple => _sortingSelector(tuple.Item))
              : query.OrderByDescending(tuple => _sortingSelector(tuple.Item));

            var map = query.Select((tuple, index) => (OldIndex: tuple.Index, NewIndex: index))
             .Where(o => o.OldIndex != o.NewIndex);

            using (var enumerator = map.GetEnumerator())
                if (enumerator.MoveNext())
                    Move(enumerator.Current.OldIndex, enumerator.Current.NewIndex);
        }
    }
}