using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.ChatParse;
using FFXIVAPP.Plugin.TeastParse.Factories;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse.Models
{
    /// <summary>
    /// A Parse context contains information
    /// about one specfiic parse session.
    /// </summary>
    public interface IParseContext : IDisposable
    {
        string Name { get; }
        bool IsCurrent { get; }
    }

    /// <summary>
    /// Current parse context is the context that keeps
    /// track of current parsing.
    /// </summary>
    public interface ICurrentParseContext : IParseContext
    {
        CurrentPlayer CurrentPlayer { get; set; }
        IActorModelCollection Actors { get; }

        void HandleLine(ChatLogItem line);
        void ActorUpdate(ConcurrentDictionary<uint, ActorItem> actorItems, ActorType type);
    }

    public class ParseContext : IParseContext
    {
        private bool _isDisposed = false;

        public string Name { get; }
        public bool IsCurrent => false;

        public ParseContext(string fullPath)
        {
            Name = Path.GetFileNameWithoutExtension(fullPath);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
        }
    }

    public class CurrentParseContext : ICurrentParseContext
    {
        private bool _isDisposed = false;
        public string Name => "Current";
        public bool IsCurrent => true;

        public IActorModelCollection Actors { get; }

        private readonly IChatFacade _facade;
        private readonly IActorItemHelper _actors;
        private readonly IRepository _repository;

        public CurrentParseContext(List<ChatCodes> codes, ITimelineCollection timeline,
                                    IDetrimentalFactory detrimentalFactory, IBeneficialFactory beneficialFactory,
                                    IActionFactory actionFactory, IActorItemHelper actorItemHelper, IRepositoryFactory repositoryFactory)
        {
            var database = Path.Combine(Constants.PluginsParsesPath, $"parser{DateTime.Now.ToString("yyyyMMddHHmmss")}.db");
            var connection = $"Data Source={database};Version=3;";
            _repository = repositoryFactory.Create(connection);
            Actors = new ActorModelCollection(timeline, actorItemHelper, actionFactory, _repository);

            _facade = new ChatFacade(codes, Actors, timeline, _repository, detrimentalFactory, beneficialFactory, actionFactory);
            _actors = actorItemHelper;
        }

        public CurrentPlayer CurrentPlayer
        {
            get => _actors.CurrentPlayer;
            set => _actors.CurrentPlayer = value;
        }

        public void HandleLine(ChatLogItem line)
        {
            _facade.HandleLine(line);
        }

        public void ActorUpdate(ConcurrentDictionary<uint, ActorItem> actorItems, ActorType type)
        {
            _actors.HandleUpdate(actorItems, type);
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            try
            {
                _repository.Dispose();
            }
            catch (Exception) { }
        }
    }
}