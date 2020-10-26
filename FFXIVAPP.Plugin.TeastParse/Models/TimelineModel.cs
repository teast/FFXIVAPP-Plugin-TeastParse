using System;

namespace FFXIVAPP.Plugin.TeastParse.Models
{
    public class TimelineModel
    {
        public string Name { get; }
        public DateTime StartUtc { get; }
        public DateTime? EndUtc { get; set; }

        public TimelineModel(string name, DateTime startUtc, DateTime? endUtc = null)
        {
            Name = name;
            StartUtc = startUtc;
            EndUtc = endUtc;
        }
    }
}