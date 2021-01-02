using System;

namespace FFXIVAPP.Plugin.TeastParse
{
    /// <summary>
    /// Represents current "real time"
    /// </summary>
    public interface IParseClock
    {
        DateTime UtcNow { get; }
        IParseTimer CreateTimer();
    }

    /// <summary>
    /// Will use .Net <see cref="DateTime.UtcNow" /> as current time
    /// </summary>
    internal class ParseClockReal : IParseClock
    {
        public DateTime UtcNow => DateTime.UtcNow;

        public IParseTimer CreateTimer() => new ParseTimerReal();
    }

    /// <summary>
    /// Will use whatever time is set as current time
    /// </summary>
    public class ParseClockFake : IParseClock
    {
        public event EventHandler<ParseClockFake> OnTimeChanged;

        private DateTime _now;

        public ParseClockFake(DateTime now)
        {
            _now = now;
        }

        public DateTime UtcNow
        {
            get => _now;
            set
            {
                _now = value;
                OnTimeChanged?.Invoke(this, this);
            }
        }

        public IParseTimer CreateTimer() => new ParseTimerFake(this);
    }
}