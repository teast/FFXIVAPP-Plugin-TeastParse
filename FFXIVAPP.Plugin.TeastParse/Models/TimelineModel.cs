using System;

namespace FFXIVAPP.Plugin.TeastParse.Models
{
    public class TimelineModel
    {
        private static int TimelineCounter = 0;

        public int Index { get; }
        public string Name { get; }
        public DateTime StartUtc { get; }
        public DateTime? EndUtc { get; set; }

        public TimelineModel(string name, string startUtc, string endUtc)
        : this(name, DateTime.Parse(startUtc), string.IsNullOrEmpty(endUtc) ? (DateTime?)null : DateTime.Parse(endUtc))
        {
        }

        public TimelineModel(string name, DateTime startUtc, DateTime? endUtc = null)
        {
            Name = name;
            StartUtc = startUtc;
            EndUtc = endUtc;
            Index = ++TimelineCounter;
        }
    }
}