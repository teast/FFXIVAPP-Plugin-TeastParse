using FFXIVAPP.IPluginInterface;
using FFXIVAPP.IPluginInterface.Events;
using FFXIVAPP.Plugin.TeastParse.Models;
using NLog;

namespace FFXIVAPP.Plugin.TeastParse
{
    /// <summary>
    /// Listens to actual event streams from FFXIVAPP and deligates
    /// them to <see cref="IEventHandler" /> for <see cref="ICurrentParseContext" />.
    /// </summary>
    public class EventSubscriber
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly IEventHandler _handler;

        public EventSubscriber(ICurrentParseContext parseContext)
        {
            _handler = parseContext.EventHandler;
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

        private void OnConstantsUpdated(object sender, ConstantsEntityEvent constantsEntityEvent) {
            if (sender == null)
                return;

            _handler.OnConstantsUpdated(constantsEntityEvent);
        }

        private void OnCurrentPlayerUpdated(object sender, CurrentPlayerEvent currentPlayer)
        {
            if (sender == null)
                return;

            _handler.OnCurrentPlayerUpdated(currentPlayer);
        }

        private void OnChatLogItemReceived(object sender, ChatLogItemEvent chatLogItemEvent)
        {
            if (sender == null)
                return;
            _handler.OnChatLogItemReceived(chatLogItemEvent);
        }

        private void OnMonsterItemsUpdated(object sender, ActorItemsEvent actorItemsEvent)
        {
            if (sender == null)
                return;
            
            _handler.OnMonsterItemsUpdated(actorItemsEvent);

        }

        private void OnNPCItemsUpdated(object sender, ActorItemsEvent actorItemsEvent)
        {
            if (sender == null)
                return;

            _handler.OnNPCItemsUpdated(actorItemsEvent);
        }

        private void OnPCItemsUpdated(object sender, ActorItemsEvent actorItemsEvent)
        {
            if (sender == null)
                return;

            _handler.OnPCItemsUpdated(actorItemsEvent);
        }
    }
}