using System;
using System.Collections.Generic;
using FFXIVAPP.IPluginInterface.Events;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse.Models
{
    /// <summary>
    /// "Play" up an previous recorded parse events from FFXIV
    /// </summary>
    internal class ParseReplay
    {
        /// <summary>
        /// Contains all chat lines for the previous parse
        /// </summary>
        private readonly List<ChatLogLine> _lines;

        /// <summary>
        /// The clock based from the previous parse
        /// </summary>
        private readonly ParseClockFake _clock;

        /// <summary>
        /// The actual logic that handles chat line parsing
        /// </summary>
        private readonly IEventHandler _handler;

        /// <summary>
        /// Next line to read
        /// </summary>
        private int _index;

        /// <summary>
        /// True if we have reached the end and have nothing less to parse.
        /// </summary>
        public bool EOF => _index >= _lines.Count;

        public ParseReplay(List<ChatLogLine> lines, ParseClockFake clock, IEventHandler handler)
        {
            _lines = lines;
            _clock = clock;
            _handler = handler;
            _index = 0;
        }

        /// <summary>
        /// Read and parse next chat line and move the pointer forward
        /// </summary>
        public void Tick()
        {
            if (EOF)
                return;

            var line = _lines[_index++];
            _clock.UtcNow = DateTime.SpecifyKind(DateTime.Parse(line.OccurredUtc), DateTimeKind.Utc);
            _handler.OnChatLogItemReceived(new ChatLogItemEvent(this, new ChatLogItem
            {
                TimeStamp = DateTime.Parse(line.Timestamp),
                Code = line.ChatCode,
                Line = line.ChatLine
            }));

            // Make sure to run all timers to the end...
            if (EOF)
                _clock.UtcNow = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
        }
    }
}