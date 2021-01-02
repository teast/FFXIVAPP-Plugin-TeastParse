using System;
using System.Timers;

namespace FFXIVAPP.Plugin.TeastParse
{
    /// <summary>
    /// Custom timer interface for use with <see cref="IParseClock" />
    /// </summary>
    public interface IParseTimer
    {
        double Interval { get; set; }
        event Action Elapsed;
        void Stop();
        void Start();
    }

    /// <summary>
    /// Implements <see cref="IParseTimer" /> using the frameworks <see cref="Timer" /> as base
    /// </summary>
    internal class ParseTimerReal : IParseTimer
    {
        private Timer _timer;
        public event Action Elapsed;

        public double Interval
        {
            get => _timer.Interval;
            set => _timer.Interval = value;
        }

        public ParseTimerReal()
        {
            _timer = new Timer();
            _timer.Elapsed += (s, a) => Elapsed?.Invoke();
        }

        public void Start() => _timer.Start();
        public void Stop() => _timer.Stop();
    }

    /// <summary>
    /// Implements an <see cref="IParseTimer" /> that uses an <see cref="ParserClockFake" /> as trigger
    /// </summary>
    public class ParseTimerFake : IParseTimer
    {
        private bool _running;
        private double _interval;
        private DateTime _next;
        private IParseClock _clock;

        public event Action Elapsed;

        public double Interval
        {
            get => _interval;
            set
            {
                _interval = value;
                _next = _clock.UtcNow.AddMilliseconds(_interval);
            }
        }

        public ParseTimerFake(ParseClockFake clock)
        {
            _running = false;
            _clock = clock;
            _interval = 0;
            clock.OnTimeChanged += TimeTick;
        }

        private void TimeTick(object sender, ParseClockFake clockFake)
        {
            if (!_running)
                return;

            if (clockFake.UtcNow < _next)
                return;

            _running = false;
            Elapsed?.Invoke();
        }

        public void Start() => _running = true;
        public void Stop() => _running = false;

    }
}