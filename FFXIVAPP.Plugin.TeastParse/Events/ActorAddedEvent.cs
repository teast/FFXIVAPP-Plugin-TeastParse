using System;
using FFXIVAPP.Plugin.TeastParse.Actors;

namespace FFXIVAPP.Plugin.TeastParse.Events
{
    public class ActorAddedEvent: EventArgs
    {
        public object Sender { get; }
        public ActorModel Actor { get; }
        
        public ActorAddedEvent(object sender, ActorModel actor)
        {
            Sender = sender;
            Actor = actor;
        }
    }
}