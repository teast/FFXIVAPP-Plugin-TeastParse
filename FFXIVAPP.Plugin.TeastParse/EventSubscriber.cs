using System;
using FFXIVAPP.IPluginInterface;
using FFXIVAPP.IPluginInterface.Events;
using FFXIVAPP.Plugin.TeastParse.ChatParse;
using FFXIVAPP.Plugin.TeastParse.Actors;
using Sharlayan.Core;
using NLog;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.Common.Core.Constant;

namespace FFXIVAPP.Plugin.TeastParse
{
    public class EventSubscriber
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private readonly IChatFactory _factory;
        private readonly IActorItemHelper _actors;

        public EventSubscriber(IChatFactory factory, IActorItemHelper actors)
        {
            _factory = factory;
            _actors = actors;
        }

        public void Subscribe(IPluginHost plugin)
        {
            if (plugin == null)
                return;

            plugin.ConstantsUpdated += OnConstantsUpdated;
            plugin.ChatLogItemReceived += OnChatLogItemReceived;
            plugin.MonsterItemsUpdated += OnMonsterItemsUpdated;
            plugin.NPCItemsUpdated += OnNPCItemsUpdated;
            plugin.PCItemsUpdated += OnPCItemsUpdated;
            plugin.CurrentPlayerUpdated += OnCurrentPlayerUpdated;
        }

        public void UnSubscribe(IPluginHost plugin)
        {
            if (plugin == null)
                return;

            plugin.ConstantsUpdated -= OnConstantsUpdated;
            plugin.ChatLogItemReceived -= OnChatLogItemReceived;
            plugin.MonsterItemsUpdated -= OnMonsterItemsUpdated;
            plugin.NPCItemsUpdated -= OnNPCItemsUpdated;
            plugin.PCItemsUpdated -= OnPCItemsUpdated;
            plugin.CurrentPlayerUpdated -= OnCurrentPlayerUpdated;
        }

        private static void OnConstantsUpdated(object sender, ConstantsEntityEvent constantsEntityEvent) {
            // delegate event from constants, not required to subsribe, but recommended as it gives you app settings
            if (sender == null) {
                return;
            }

            ConstantsEntity constantsEntity = constantsEntityEvent.ConstantsEntity;
            Constants.AutoTranslate = constantsEntity.AutoTranslate;

            Constants.Colors = constantsEntity.Colors;
            Constants.CultureInfo = constantsEntity.CultureInfo;
            Constants.CharacterName = constantsEntity.CharacterName;
            Constants.ServerName = constantsEntity.ServerName;
            Constants.GameLanguage = (GameLanguageEnum)Enum.Parse(typeof(GameLanguageEnum), constantsEntity.GameLanguage);
            Settings.Default.EnableHelpLabels = constantsEntity.EnableHelpLabels;
        }

        private void OnCurrentPlayerUpdated(object sender, CurrentPlayerEvent currentPlayer)
        {
            _actors.CurrentPlayer = currentPlayer.CurrentPlayer;
        }

        private void OnChatLogItemReceived(object sender, ChatLogItemEvent chatLogItemEvent)
        {
            // delegate event from chat log, not required to subsribe
            // this updates 100 times a second and only sends a line when it gets a new one
            if (sender == null)
            {
                return;
            }

            ChatLogItem chatLogItem = chatLogItemEvent.ChatLogItem;
            try
            {
                Logging.Log(Logger, $"Chat: {chatLogItem.TimeStamp} [{chatLogItem.Code}] \"{chatLogItem.Line}\"");
                _factory.HandleLine(chatLogItem);
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"FFXIVAPP.Plugin.TeastParse.{nameof(EventSubscriber)}.{nameof(OnChatLogItemReceived)}: Unhandled exception", ex);
            }
        }

        private void OnMonsterItemsUpdated(object sender, ActorItemsEvent actorItemsEvent)
        {
            // delegate event from monster entities from ram, not required to subsribe
            // this updates 10x a second and only sends data if the items are found in ram
            // currently there no change/new/removed event handling (looking into it)
            if (sender == null)
            {
                return;
            }

            try
            {
                _actors.HandelUpdate(actorItemsEvent.ActorItems, ActorType.Monster);
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"FFXIVAPP.Plugin.TeastParse.{nameof(EventSubscriber)}.{nameof(OnMonsterItemsUpdated)}: Unhandled exception", ex);
            }
        }

        private void OnNPCItemsUpdated(object sender, ActorItemsEvent actorItemsEvent)
        {
            // delegate event from npc entities from ram, not required to subsribe
            // this list includes anything that is not a player or monster
            // this updates 10x a second and only sends data if the items are found in ram
            // currently there no change/new/removed event handling (looking into it)
            if (sender == null)
            {
                return;
            }

            try
            {
                _actors.HandelUpdate(actorItemsEvent.ActorItems, ActorType.NPC);
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"FFXIVAPP.Plugin.TeastParse.{nameof(EventSubscriber)}.{nameof(OnNPCItemsUpdated)}: Unhandled exception", ex);
            }
        }

        private void OnPCItemsUpdated(object sender, ActorItemsEvent actorItemsEvent)
        {
            // delegate event from player entities from ram, not required to subsribe
            // this updates 10x a second and only sends data if the items are found in ram
            // currently there no change/new/removed event handling (looking into it)
            if (sender == null)
            {
                return;
            }

            try
            {
                _actors.HandelUpdate(actorItemsEvent.ActorItems, ActorType.Player);
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"FFXIVAPP.Plugin.TeastParse.{nameof(EventSubscriber)}.{nameof(OnPCItemsUpdated)}: Unhandled exception", ex);
            }
        }
    }
}