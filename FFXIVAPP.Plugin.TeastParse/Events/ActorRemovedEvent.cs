using System;
using FFXIVAPP.Plugin.TeastParse.Actors;

namespace FFXIVAPP.Plugin.TeastParse.Events
{
    public class ActorRemovedEvent: EventArgs
    {
        public object Sender { get; }
        public ActorModel Actor { get; }
        
        public ActorRemovedEvent(object sender, ActorModel actor)
        {
            Sender = sender;
            Actor = actor;
        }
    }
}