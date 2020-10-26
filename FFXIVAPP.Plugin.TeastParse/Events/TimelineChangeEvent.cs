using System;
using FFXIVAPP.Plugin.TeastParse.Models;

namespace FFXIVAPP.Plugin.TeastParse.Events
{
    public class TimelineChangeEvent: EventArgs
    {
        public object Sender { get; }
        public TimelineModel Previous { get; }
        public TimelineModel Next { get; }
        
        public TimelineChangeEvent(object sender, TimelineModel previous, TimelineModel next)
        {
            Sender = sender;
            Previous = previous;
            Next = next;
        }
    }
}