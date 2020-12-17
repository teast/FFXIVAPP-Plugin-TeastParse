using System;

namespace FFXIVAPP.Plugin.TeastParse
{
    /// <summary>
    /// Represents current "real time"
    /// </summary>
    internal interface IParseClock
    {
        DateTime UtcNow { get; }
    }

    /// <summary>
    /// Will use .Net <see cref="DateTime.UtcNow" /> as current time
    /// </summary>
    internal class ParseClockReal : IParseClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

    /// <summary>
    /// Will use whatever time is set as current time
    /// </summary>
    internal class ParseClockFake : IParseClock
    {
        private DateTime _now;

        public ParseClockFake(DateTime now)
        {
            _now = now;
        }

        public DateTime UtcNow
        {
            get => _now;
            set => _now = value;
        }
    }
}