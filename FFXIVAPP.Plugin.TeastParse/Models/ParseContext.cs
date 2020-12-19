using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.ChatParse;
using FFXIVAPP.Plugin.TeastParse.Factories;
using FFXIVAPP.Plugin.TeastParse.Repositories;

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

        IActorModelCollection Actors { get; }
        ITimelineCollection Timeline { get; }
    }

    /// <summary>
    /// Parse context for "current" parser.
    /// That is the parser that parse directly from FFXIV
    /// </summary>
    public interface ICurrentParseContext : IParseContext
    {
        IEventHandler EventHandler { get; }
    }

    /// <summary>
    /// Handles reading of old parsers
    /// If the user wants to check statics from an old parse
    /// </summary>
    public class ParseContext : IParseContext
    {
        private bool _isDisposed = false;
        private readonly IRepository _repository;
        private readonly EventHandler _handler;

        public IActorModelCollection Actors { get; }
        public ITimelineCollection Timeline { get; }

        private readonly ParseClockFake _clock;

        public string Name { get; }
        public bool IsCurrent => false;

        public ParseContext(string fullPath, List<ChatCodes> codes, IDetrimentalFactory detrimentalFactory, IBeneficialFactory beneficialFactory,
            IActionFactory actionFactory, IActorItemHelper actorItemHelper, IRepositoryFactory repositoryFactory)
        {
            var connection = $"Data Source={fullPath};Version=3;";
            Name = Path.GetFileNameWithoutExtension(fullPath);
            _repository = repositoryFactory.Create(connection, true);
            Timeline = new TimelineCollection(_repository.GetTimelines().ToList());

            Actors = new ActorModelCollection(Timeline, actorItemHelper, actionFactory, _repository, _repository.GetActors(Timeline)?.ToList());

            _clock = new ParseClockFake(DateTime.MinValue);
            var facade = new ChatFacade(codes, Actors, Timeline, _repository, detrimentalFactory, beneficialFactory, actionFactory, _clock);
            _handler = new EventHandler(actorItemHelper, facade);
        }

        public Task Replay()
        {
            var task = Task.Run(() =>
            {
                var replay = new ParseReplay(_repository.GetChatLogs().ToList(), _clock, _handler);
                while (!replay.EOF)
                    replay.Tick();
            });

            return task;
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

    /// <summary>
    /// Handles current parsing from an active FFXIV session
    /// </summary>
    public class CurrentParseContext : ICurrentParseContext
    {
        private bool _isDisposed = false;
        public string Name => "Current";
        public bool IsCurrent => true;

        public IActorModelCollection Actors { get; }
        public IEventHandler EventHandler { get; }
        public ITimelineCollection Timeline { get; }
        private readonly IRepository _repository;

        public CurrentParseContext(List<ChatCodes> codes, IDetrimentalFactory detrimentalFactory, IBeneficialFactory beneficialFactory,
                                    IActionFactory actionFactory, IActorItemHelper actorItemHelper, IRepositoryFactory repositoryFactory)
        {
            var database = Path.Combine(Constants.PluginsParsesPath, $"parser{DateTime.Now.ToString("yyyyMMddHHmmss")}.db");
            var connection = $"Data Source={database};Version=3;";
            var clock = new ParseClockReal();
            Timeline = new TimelineCollection();
            _repository = repositoryFactory.Create(connection);
            Actors = new ActorModelCollection(Timeline, actorItemHelper, actionFactory, _repository);

            var facade = new ChatFacade(codes, Actors, Timeline, _repository, detrimentalFactory, beneficialFactory, actionFactory, clock);
            var actors = actorItemHelper;

            EventHandler = new EventHandler(actorItemHelper, facade);
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