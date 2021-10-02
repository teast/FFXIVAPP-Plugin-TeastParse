using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FFXIVAPP.Plugin.TeastParse.Events;
using FFXIVAPP.Plugin.TeastParse.Models;

namespace FFXIVAPP.Plugin.TeastParse
{
    /// <summary>
    /// Contains all found timelines and keep track on latest active timeline
    /// </summary>
    /// <remarks>
    /// a timeline represents an new event in FFXIV.
    /// For example if a new instance starts it will be registered as a new timeline
    /// </remarks>
    public interface ITimelineCollection
    {
        event EventHandler<TimelineChangeEvent> CurrentTimelineChange;
        int Count { get; }
        TimelineModel Current { get; }
        TimelineModel this[int index] { get; }
        TimelineModel this[string name] { get; }
        void Add(TimelineModel model);
        TimelineModel Close(string dungeon);
        IEnumerator<TimelineModel> GetEnumerator();
        List<TimelineModel> ToList();
    }

    internal class TimelineCollection : ITimelineCollection
    {
        private TimelineModel _current;
        private List<TimelineModel> _timelines;

        public event EventHandler<TimelineChangeEvent> CurrentTimelineChange;

        public int Count => _timelines.Count;

        public TimelineModel Current => GetOrUpdateCurrent();

        public TimelineModel this[int index] => _timelines[index];
        public TimelineModel this[string name] => _timelines.FirstOrDefault(t => t.Name == name);

        public TimelineCollection(List<TimelineModel> timelines = null)
        {
            _timelines = timelines ?? new List<TimelineModel>();
        }

        public IEnumerator<TimelineModel> GetEnumerator() => _timelines.GetEnumerator();
        public List<TimelineModel> ToList() => _timelines.ToList();

        public void Add(TimelineModel model)
        {
            var old = _current;
            _current = model;
            _timelines.Add(_current);
            RaiseTimelineChange(old, _current);
        }

        public TimelineModel Close(string dungeon)
        {
            var timeline = this[dungeon];
            if (timeline == null)
                return GetOrUpdateCurrent();

            timeline.EndUtc = DateTime.UtcNow;

            if (timeline.Name == Current.Name)
            {
                var old = _current;
                _current = null;
                GetOrUpdateCurrent();
                RaiseTimelineChange(old, _current);
            }

            return timeline;
        }

        private void RaiseTimelineChange(TimelineModel previous, TimelineModel next)
        {
            var arg = new TimelineChangeEvent(this, previous, next);
            CurrentTimelineChange?.Invoke(this, arg);
        }

        private TimelineModel GetOrUpdateCurrent() => _current ?? (_current = _timelines
                    .Where(t => t.EndUtc.HasValue == false)
                    .OrderByDescending(t => t.StartUtc)
                    .FirstOrDefault());
    }
}