using System;
using FFXIVAPP.Common.Core.Constant;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.IPluginInterface.Events;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.ChatParse;
using NLog;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse
{
    /// <summary>
    /// The actual logic for incoming data to handle (and parse)
    /// </summary>
    public interface IEventHandler
    {
        void OnConstantsUpdated(ConstantsEntityEvent constantsEntityEvent);
        void OnCurrentPlayerUpdated(CurrentPlayerEvent currentPlayer);
        void OnChatLogItemReceived(ChatLogItemEvent chatLogItemEvent);
        void OnMonsterItemsUpdated(ActorItemsEvent actorItemsEvent);
        void OnNPCItemsUpdated(ActorItemsEvent actorItemsEvent);
        void OnPCItemsUpdated(ActorItemsEvent actorItemsEvent);
    }

    /// <summary>
    /// The actual implementation for handling incoming events
    /// </summary>
    internal class EventHandler : IEventHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IActorItemHelper _actors;
        private readonly IChatFacade _chatFacade;

        //private readonly IParseContext _parseContext;

        public EventHandler(IActorItemHelper actors, IChatFacade chatFacade)
        {
            _actors = actors;
            _chatFacade = chatFacade;
        }

        public void OnConstantsUpdated(ConstantsEntityEvent constantsEntityEvent)
        {
            ConstantsEntity constantsEntity = constantsEntityEvent.ConstantsEntity;
            Constants.AutoTranslate = constantsEntity.AutoTranslate;

            Constants.Colors = constantsEntity.Colors;
            Constants.CultureInfo = constantsEntity.CultureInfo;
            Constants.CharacterName = constantsEntity.CharacterName;
            Constants.ServerName = constantsEntity.ServerName;
            if (constantsEntity.GameLanguage != null && Enum.TryParse<GameLanguageEnum>(constantsEntity.GameLanguage, out var gameLanguage))
                Constants.GameLanguage = gameLanguage;
            else
                Constants.GameLanguage = GameLanguageEnum.English;

            Settings.Default.EnableHelpLabels = constantsEntity.EnableHelpLabels;
        }

        public void OnCurrentPlayerUpdated(CurrentPlayerEvent currentPlayer)
        {
            _actors.CurrentPlayer = currentPlayer.CurrentPlayer;
        }

        public void OnChatLogItemReceived(ChatLogItemEvent chatLogItemEvent)
        {
            ChatLogItem chatLogItem = chatLogItemEvent.ChatLogItem;
            try
            {
                Logging.Log(Logger, $"Chat: {chatLogItem.TimeStamp} [{chatLogItem.Code}] \"{chatLogItem.Line}\"");
                _chatFacade.HandleLine(chatLogItem);
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"FFXIVAPP.Plugin.TeastParse.{nameof(EventSubscriber)}.{nameof(OnChatLogItemReceived)}: Unhandled exception", ex);
            }
        }

        public void OnMonsterItemsUpdated(ActorItemsEvent actorItemsEvent)
        {
            try
            {
                _actors.HandleUpdate(actorItemsEvent.ActorItems, ActorType.Monster);
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"FFXIVAPP.Plugin.TeastParse.{nameof(EventSubscriber)}.{nameof(OnMonsterItemsUpdated)}: Unhandled exception", ex);
            }
        }

        public void OnNPCItemsUpdated(ActorItemsEvent actorItemsEvent)
        {
            try
            {
                _actors.HandleUpdate(actorItemsEvent.ActorItems, ActorType.NPC);
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"FFXIVAPP.Plugin.TeastParse.{nameof(EventSubscriber)}.{nameof(OnNPCItemsUpdated)}: Unhandled exception", ex);
            }
        }

        public void OnPCItemsUpdated(ActorItemsEvent actorItemsEvent)
        {
            try
            {
                _actors.HandleUpdate(actorItemsEvent.ActorItems, ActorType.Player);
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"FFXIVAPP.Plugin.TeastParse.{nameof(EventSubscriber)}.{nameof(OnPCItemsUpdated)}: Unhandled exception", ex);
            }
        }

    }
}