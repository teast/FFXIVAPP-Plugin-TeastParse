using FFXIVAPP.Plugin.TeastParse.Actors;

namespace FFXIVAPP.Plugin.TeastParse.Extensions
{
    public static class ActorTypeExtensions
    {
        public static ActorType ToActorType(this ChatCodeSubject subject)
        {
            switch (subject)
            {
                case ChatCodeSubject.Alliance:
                case ChatCodeSubject.Party:
                case ChatCodeSubject.You:
                    return ActorType.Player;
                case ChatCodeSubject.NPC:
                case ChatCodeSubject.Pet:
                case ChatCodeSubject.PetAlliance:
                case ChatCodeSubject.PetOther:
                case ChatCodeSubject.PetParty:
                    return ActorType.Player;
                case ChatCodeSubject.Engaged:
                case ChatCodeSubject.UnEngaged:
                    return ActorType.Monster;
                default:
                    return ActorType.NPC;
            }
        }

        public static ActorType ToActorType(this ChatCodeDirection direction, ChatCodeSubject subject)
        {
            switch(direction)
            {
            case ChatCodeDirection.UnEngaged:
            case ChatCodeDirection.Engaged:
                return ActorType.Monster;
            case ChatCodeDirection.You:
            case ChatCodeDirection.Alliance:
            case ChatCodeDirection.Party:
            case ChatCodeDirection.Pet:
            case ChatCodeDirection.PetAlliance:
            case ChatCodeDirection.PetParty:
                return ActorType.Player;
            case ChatCodeDirection.Self:
                return subject.ToActorType();
            case ChatCodeDirection.Multi:
            case ChatCodeDirection.Other:
            case ChatCodeDirection.PetOther:
            case ChatCodeDirection.To:
            case ChatCodeDirection.Unknown:
            default:
                return ActorType.NPC;
            }
        }
    }
}