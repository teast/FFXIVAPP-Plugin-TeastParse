using System;
using System.Collections.Concurrent;
using System.Data.SQLite;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using FFXIVAPP.IPluginInterface;
using FFXIVAPP.IPluginInterface.Events;
using FFXIVAPP.Plugin.TeastParse;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.Models;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using FFXIVAPP.Plugin.TeastParse.ViewModels;
using FluentAssertions;
using Moq;
using NLog;
using NLog.Targets;
using Sharlayan.Core;

namespace Tests
{
    public class World
    {
        private readonly ConcurrentDictionary<uint, ActorItem> _players = new ConcurrentDictionary<uint, ActorItem>();
        private readonly ConcurrentDictionary<uint, ActorItem> _monsters = new ConcurrentDictionary<uint, ActorItem>();
        private readonly ConcurrentDictionary<uint, ActorItem> _npc = new ConcurrentDictionary<uint, ActorItem>();
        private GameLanguageEnum _language = GameLanguageEnum.English;
        private readonly EventSubscriber _event;
        private readonly Mock<IPluginHost> _pluginHost;

        private Repository _repository;

        private readonly Random _random = new Random();
        private readonly ParserIoc _ioc;
        private readonly SQLiteConnection _connection;

        public MainViewModel MainViewModel => _ioc.Get<MainViewModel>();

        public World()
        {
            // Output any exception so it is easier to find probelms in a test
            //Log.Logger = new LoggerConfiguration()
            //.MinimumLevel.Warning()
            //.WriteTo.Console().CreateLogger();

            // Output any exception so it is easier to find probelms in a test
            var config = new NLog.Config.LoggingConfiguration();
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, new ConsoleTarget("logconsole"));
            NLog.LogManager.Configuration = config;
            NLog.LogManager.ThrowExceptions = true;

            SqlMapper.AddTypeHandler(new ActionModelHandler());

            var factory = new Mock<IRepositoryFactory>();
            var connectionString = string.Format("FullUri=file:{0}?mode=memory&cache=shared", Guid.NewGuid().ToString("N"));
            _connection = new SQLiteConnection(connectionString);
            _connection.Open();
            _repository = new Repository(connectionString);
            factory.Setup(_ => _.Create(It.IsAny<string>(), It.IsAny<bool>())).Returns(_repository);

            _ioc = new ParserIoc();
            _ioc.Singelton<IRepositoryFactory>(() => factory.Object);
            _pluginHost = new Mock<IPluginHost>();
            _event = new EventSubscriber(_ioc.Get<ICurrentParseContext>());
            _event.Subscribe(_pluginHost.Object);

            // Make sure we bind our Actor collection to the parser
            _pluginHost.Raise(_ => _.PCItemsUpdated += null, new ActorItemsEvent(this, _players));
            _pluginHost.Raise(_ => _.NPCItemsUpdated += null, new ActorItemsEvent(this, _npc));
            _pluginHost.Raise(_ => _.MonsterItemsUpdated += null, new ActorItemsEvent(this, _monsters));
        }

        public void AfterScenario()
        {
            _repository.CloseConnection();
            _repository.Dispose();
            _connection.Close();
            _connection.Dispose();
        }

        public void ClearActorList()
        {
            _players.Clear();
        }

        public void CreatePlayer(string name)
        {
            uint thirtyBits = (uint)_random.Next(1 << 30);
            uint twoBits = (uint)_random.Next(1 << 2);
            uint fullRange = (thirtyBits << 2) | twoBits;
            var result = _players.TryAdd(fullRange, new ActorItem
            {
                Name = string.Join(" ", name.Split(' ').Select(s => s.First().ToString().ToUpper() + s.Substring(1)))
            });

            if (!result)
                throw new Exception($"Was not able to add player \"{name}\" to the fake ConcurrentDictionary");
        }

        public void CreateMonster(string name)
        {
            uint thirtyBits = (uint)_random.Next(1 << 30);
            uint twoBits = (uint)_random.Next(1 << 2);
            uint fullRange = (thirtyBits << 2) | twoBits;
            var result = _monsters.TryAdd(fullRange, new ActorItem
            {
                Name = string.Join(" ", name.Split(' ').Select(s => s.First().ToString().ToUpper() + s.Substring(1)))
            });

            if (!result)
                throw new Exception($"Was not able to add monster \"{name}\" to the fake ConcurrentDictionary");
        }

        public void PlayerIsYou(string name)
        {
            var fullName = string.Join(" ", name.Split(' ').Select(s => s.First().ToString().ToUpper() + s.Substring(1)));

            _pluginHost.Raise(_ => _.CurrentPlayerUpdated += null, new CurrentPlayerEvent(this, new CurrentPlayer
            {
                Name = fullName
            }));
        }

        public void PlayerIsHe(string name)
        {
            var fullName = string.Join(" ", name.Split(' ').Select(s => s.First().ToString().ToUpper() + s.Substring(1)));

            var player = _players.First(p => p.Value.Name == fullName);
            player.Value.Sex = Sharlayan.Core.Enums.Actor.Sex.Male;
        }

        public void PlayerIsShe(string name)
        {
            var fullName = string.Join(" ", name.Split(' ').Select(s => s.First().ToString().ToUpper() + s.Substring(1)));

            var player = _players.First(p => p.Value.Name == fullName);
            player.Value.Sex = Sharlayan.Core.Enums.Actor.Sex.Male;
        }

        public void SetLanguage(GameLanguageEnum language)
        {
            _language = language;
        }

        public void RaiseChatLog(string code, string line)
        {
            _pluginHost.Raise(_ => _.ChatLogItemReceived += null, new ChatLogItemEvent(this, new ChatLogItem
            {
                Line = line,
                Code = code
            }));
        }

        public void ContainSingleCure(Expression<Func<CureModel, bool>> expression)
        {
            var data = _connection.Query<CureModel>("SELECT * FROM Cure");
            data.Should().ContainSingle(expression);
        }

        public void ContainSingleDamage(Expression<Func<DamageModel, bool>> expression)
        {
            var data = _connection.Query<DamageModel>("SELECT * FROM Damage");
            data.Should().ContainSingle(expression);
        }

        public void NotContainDamage(Expression<Func<DamageModel, bool>> expression)
        {
            var data = _connection.Query<DamageModel>("SELECT * FROM Damage");
            data.Should().NotContain(expression);
        }

        public void ContainActor(Expression<Func<ActorModel, bool>> expression)
        {
            var data = _repository.GetActors(_ioc.Get<ICurrentParseContext>().Timeline);
            data.Should().Contain(expression);
        }
    }
}