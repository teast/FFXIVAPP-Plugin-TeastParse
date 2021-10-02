using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using Dapper;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.Factories;
using FFXIVAPP.Plugin.TeastParse.Models;
using NLog;

namespace FFXIVAPP.Plugin.TeastParse.Repositories
{
    /// <summary>
    /// Represents an CRUD repository for db access
    /// </summary>
    public interface IRepository : IDisposable
    {
        void AddDamage(DamageModel model);
        void AddCure(CureModel model);
        void AddActor(ActorModel model);
        void AddTimeline(TimelineModel model);
        void CloseTimeline(string name, DateTime endUtc);
        void AddChatLog(ChatLogLine line);
        void UpdateActor(ActorModel actor);

        IEnumerable<ChatLogLine> GetChatLogs();
        ChatLogLine GetChatLog(int id);
        IEnumerable<ActorModel> GetActors(ITimelineCollection timeline);
        IEnumerable<TimelineModel> GetTimelines();

        IEnumerable<ActorActionModel> GetActorActions(string actor, bool isYou);
    }

    /// <summary>
    /// Implemets the actual CRUD repository
    /// </summary>
    public class Repository : RepositoryReadOnly, IRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly List<string> _addedActors;
        private bool _disposed;

        public Repository(string connectionString, IActionFactory actionFactory) : base(connectionString, actionFactory)
        {
            _disposed = false;
            _addedActors = new List<string>();
        }

        public override void AddDamage(DamageModel model)
        {
            if (!Connect())
                return;

            const string query = @"
                INSERT INTO Damage
                (
                    OccurredUtc,
                    Timestamp,
                    Source,
                    Target,
                    Damage,
                    Modifier,
                    Action,
                    Critical,
                    DirectHit,
                    Blocked,
                    Parried,
                    InitDmg,
                    EndTimeUtc,
                    Subject,
                    Direction,
                    ChatCode,
                    IsDetrimental,
                    IsCombo,
                    Potency
                )
                VALUES
                (
                    @OccurredUtc,
                    @Timestamp,
                    @Source,
                    @Target,
                    @Damage,
                    @Modifier,
                    @ActionName,
                    @Critical,
                    @DirectHit,
                    @Blocked,
                    @Parried,
                    @InitDmg,
                    @EndTimeUtc,
                    @Subject,
                    @Direction,
                    @ChatCode,
                    @IsDetrimental,
                    @IsCombo,
                    @Potency
                );
            ";

            try
            {
                if (_connection.Execute(query, model) != 1)
                    Logging.Log(Logger, $"Problem storing damage information in database.");
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"{nameof(Repository)}.{nameof(AddDamage)}: Unhandled exception", ex);
            }
        }

        public override void AddCure(CureModel model)
        {
            if (!Connect())
                return;

            const string query = @"
                INSERT INTO Cure
                (
                    OccurredUtc,
                    Timestamp,
                    Source,
                    Target,
                    Cure,
                    Modifier,
                    Action,
                    Critical,
                    Subject,
                    Direction,
                    ChatCode,
                    Actions
                )
                VALUES
                (
                    @OccurredUtc,
                    @Timestamp,
                    @Source,
                    @Target,
                    @Cure,
                    @Modifier,
                    @Action,
                    @Critical,
                    @Subject,
                    @Direction,
                    @ChatCode,
                    @Actions
                );
            ";

            try
            {
                if (_connection.Execute(query, model) != 1)
                    Logging.Log(Logger, $"Problem storing cure information in database.");
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"{nameof(Repository)}.{nameof(AddCure)}: Unhandled exception", ex);
            }
        }

        public override void AddActor(ActorModel model)
        {
            if (_addedActors.Contains(model.Name))
                return;

            if (!Connect())
                return;

            const string query = @"
                INSERT INTO Actor
                (
                    ActorType,
                    Name,
                    Level,
                    Job,
                    IsYou,
                    IsParty,
                    IsAlliance,
                    IsFromMemory
                )
                VALUES
                (
                    @ActorType,
                    @Name,
                    @Level,
                    @Job,
                    @IsYou,
                    @IsParty,
                    @IsAlliance,
                    @IsFromMemory
                );
            ";

            try
            {
                if (_connection.Execute(query, model) != 1)
                    Logging.Log(Logger, $"Problem storing actor information in database.");
                _addedActors.Add(model.Name);
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"{nameof(Repository)}.{nameof(AddActor)}: Unhandled exception", ex);
            }
        }

        public override void UpdateActor(ActorModel model)
        {
            if (!Connect())
                return;

            const string query = @"
                UPDATE Actor
                SET
                    ActorType = @ActorType,
                    Level = @Level,
                    Job = @Job,
                    IsYou = @IsYou,
                    IsParty = @IsParty,
                    IsAlliance = @IsAlliance,
                    IsFromMemory = @IsFromMemory
                WHERE Name = @Name;
            ";

            try
            {
                if (_connection.Execute(query, model) != 1)
                    Logging.Log(Logger, $"Problem updating actor information in database.");
                _addedActors.Add(model.Name);
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"{nameof(Repository)}.{nameof(AddActor)}: Unhandled exception", ex);
            }
        }

        public override void AddTimeline(TimelineModel model)
        {
            if (!Connect())
                return;

            const string query = @"
                INSERT INTO Timeline
                (
                    Name,
                    StartUtc,
                    EndUtc
                )
                VALUES
                (
                    @Name,
                    @StartUtc,
                    @EndUtc
                );
            ";

            try
            {
                if (_connection.Execute(query, model) != 1)
                    Logging.Log(Logger, $"Problem storing timeline information in database.");
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"{nameof(Repository)}.{nameof(AddTimeline)}: Unhandled exception", ex);
            }
        }

        public override void CloseTimeline(string name, DateTime endUtc)
        {
            if (!Connect())
                return;

            const string query = @"UPDATE Timeline Set EndUtc = @EndUtc WHERE Name = @Name AND EndUtc IS NULL;";

            try
            {
                if (_connection.Execute(query, new { Name = name, EndUtc = endUtc }) != 1)
                    Logging.Log(Logger, $"Problem updating timeline information in database.");
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"{nameof(Repository)}.{nameof(CloseTimeline)}: Unhandled exception", ex);
            }
        }

        public override void AddChatLog(ChatLogLine line)
        {
            if (!Connect())
                return;

            const string query = @"
                INSERT INTO ChatLog
                (
                    OccurredUtc,
                    Timestamp,
                    ChatCode,
                    ChatLine
                )
                VALUES
                (
                    @OccurredUtc,
                    @Timestamp,
                    @ChatCode,
                    @ChatLine
                );
            ";

            try
            {
                if (_connection.Execute(query, line) != 1)
                    Logging.Log(Logger, $"Problem inserting chatline information into database.");
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"{nameof(Repository)}.{nameof(CloseTimeline)}: Unhandled exception", ex);
            }
        }


        public override void CloseConnection()
        {
            if (_disposed)
                return;
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }

        public override void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
            _connection = null;
        }

        protected override bool Connect()
        {
            if (_disposed)
                return false;

            if (_connection == null)
                _connection = new SQLiteConnection(_connectionString);

            if (_connection.State == ConnectionState.Open)
                return true;
            _connection.Open();
            CreateDatabase();

            return true;
        }

        private void CreateDatabase()
        {
            if (_connection.QueryFirst<int>(@"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@name;", new { name = "Damage" }) > 0)
                return;

            const string tblDamage = @"
                CREATE TABLE Damage
                (
                    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
                    OccurredUtc     TEXT NOT NULL,
                    Timestamp       TEXT,
                    Source          TEXT,
                    Target          TEXT,
                    Damage          INT,
                    Modifier        TEXT,
                    Action          TEXT,
                    Critical        INT,
                    DirectHit       INT,
                    Blocked         INT,
                    Parried         INT,
                    InitDmg         INT,
                    EndTimeUtc      TEXT,
                    Subject         TEXT,
                    Direction       TEXT,
                    ChatCode        TEXT,
                    IsDetrimental   INT,
                    IsCombo         INT,
                    Potency         INT
                );
            ";

            const string tblActor = @"
                CREATE TABLE Actor
                (
                    ActorType       TEXT NOT NULL,
                    Name            TEXT NOT NULL,
                    Level           INT,
                    Job             TEXT,
                    IsYou           INT,
                    IsParty         INT,
                    IsAlliance      INT,
                    IsFromMemory    INT

                );
            ";

            const string tblTimeline = @"
                CREATE TABLE Timeline
                (
                    Name        TEXT NOT NULL,
                    StartUtc    TEXT NOT NULL,
                    EndUtc      TEXT
                );
            ";

            const string tblCure = @"
                CREATE TABLE Cure
                (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    OccurredUtc TEXT NOT NULL,
                    Timestamp   TEXT,
                    Source      TEXT,
                    Target      TEXT,
                    Cure        INT,
                    Modifier    TEXT,
                    Action      TEXT,
                    Critical    INT,
                    Subject     TEXT,
                    Direction   TEXT,
                    ChatCode    TEXT,
                    Actions     TEXT
                );
            ";

            const string tblChatLog = @"
                CREATE TABLE ChatLog
                (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    OccurredUtc TEXT NOT NULL,
                    Timestamp   TEXT,
                    ChatCode    TEXT,
                    ChatLine    TEXT
                );
            ";

            _connection.Execute(tblDamage);
            _connection.Execute(tblActor);
            _connection.Execute(tblTimeline);
            _connection.Execute(tblCure);
            _connection.Execute(tblChatLog);
        }
    }

    /// <summary>
    /// Implements an "read-only" version of the repository
    /// </summary>
    public class RepositoryReadOnly : IRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private bool _disposed;

        protected readonly string _connectionString;
        protected SQLiteConnection _connection;

        protected readonly IActionFactory _actionFactory;

        public RepositoryReadOnly(string connectionString, IActionFactory actionFactory)
        {
            _disposed = false;
            _connectionString = connectionString;
            _actionFactory = actionFactory;
        }

        public virtual void AddActor(ActorModel model)
        {
        }

        public virtual void UpdateActor(ActorModel actor)
        {
        }

        public virtual void AddChatLog(ChatLogLine line)
        {
        }

        public virtual void AddCure(CureModel model)
        {
        }

        public virtual void AddDamage(DamageModel model)
        {
        }

        public virtual void AddTimeline(TimelineModel model)
        {
        }

        public virtual void CloseTimeline(string name, DateTime endUtc)
        {
        }

        public virtual IEnumerable<ChatLogLine> GetChatLogs()
        {
            if (!Connect())
                return null;

            return _connection.Query<ChatLogLine>("SELECT * FROM ChatLog ORDER BY ID ASC");
        }

        public virtual ChatLogLine GetChatLog(int id)
        {
            if (!Connect())
                return null;

            return _connection.Query<ChatLogLine>("SELECT * FROM ChatLog WHERE Id = @id", new { Id = id }).FirstOrDefault();
        }

        public IEnumerable<ActorModel> GetActors(ITimelineCollection timeline)
        {
            if (!Connect())
                return null;

            var data = _connection.Query<ActorModelDb>("SELECT * FROM Actor").ToList();
            return data.Select(model => new ActorModel(model.Name, model.IsFromMemory ? new Sharlayan.Core.ActorItem
            {
                Name = model.Name,
                Job = model.Job,
                JobID = (byte)model.Job,
                Level = (byte)model.Level
            } : null, model.ActorType, timeline, model.IsYou, model.IsParty, model.IsAlliance));
        }

        public IEnumerable<TimelineModel> GetTimelines()
        {
            if (!Connect())
                return null;

            return _connection.Query<TimelineModel>("SELECT * FROM Timeline");
        }

        public IEnumerable<ActorActionModel> GetActorActions(string actorName, bool isYou)
        {
            if (!Connect())
                return null;
            var result = new List<ActorActionModel>();

            var dmgs = _connection.Query<ActorActionModel>("SELECT OccurredUtc, Timestamp, Action, Damage FROM Damage WHERE (Source = @source OR Source = @source2) AND Action <> ''", param: new { Source = actorName, Source2 = isYou ? "You" : actorName });
            var heals = _connection.Query<ActorActionModel>("SELECT OccurredUtc, Timestamp, Action, Cure as Damage FROM Cure WHERE (Source = @source OR Source = @source2) AND Action <> ''", param: new { Source = actorName, Source2 = isYou ? "You" : actorName });
            result.AddRange(dmgs.Select(dmg => {
                var action = _actionFactory.GetAction(dmg.Name);

                return new ActorActionModel(dmg.OccurredUtc.ToString(), dmg.Timestamp, dmg.Name, dmg.Damage, action);
            }));

            result.AddRange(heals.Select(cure => {
                var action = _actionFactory.GetAction(cure.Name);

                return new ActorActionModel(cure.OccurredUtc.ToString(), cure.Timestamp, cure.Name, cure.Damage, action);
            }));

            return result.OrderBy(x => x.OccurredUtc);
        }


        private struct ActorModelDb
        {
            public ActorType ActorType { get; set; }
            public string Name { get; set; }
            public int Level { get; set; }
            public Sharlayan.Core.Enums.Actor.Job Job { get; set; }
            public bool IsYou { get; set; }
            public bool IsParty { get; set; }
            public bool IsAlliance { get; set; }
            public bool IsFromMemory { get; set; }
        }

        public virtual void CloseConnection()
        {
            if (_disposed)
                return;
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
        }

        public virtual void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
            _connection = null;
        }

        protected virtual bool Connect()
        {
            if (_disposed)
                return false;

            if (_connection == null)
                _connection = new SQLiteConnection(_connectionString);

            if (_connection.State == ConnectionState.Open)
                return true;

            _connection.Open();
            return true;
        }
    }
}