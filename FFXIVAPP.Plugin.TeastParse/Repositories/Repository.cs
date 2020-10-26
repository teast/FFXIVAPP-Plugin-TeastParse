using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Dapper;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.Models;
using NLog;

namespace FFXIVAPP.Plugin.TeastParse.Repositories
{
    public interface IRepository : IDisposable
    {
        void AddDamage(DamageModel model);
        void AddCure(CureModel model);
        void AddActor(ActorModel model);
        void AddTimeline(TimelineModel model);
        void CloseTimeline(string name, DateTime endUtc);
    }

    public class Repository : IRepository
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly List<string> _addedActors;
        private readonly string _connectionString;
        private SQLiteConnection _connection;
        private bool _disposed;

        public Repository(string connectionString)
        {
            _disposed = false;
            _connectionString = connectionString;
            _addedActors = new List<string>();
        }

        public void AddDamage(DamageModel model)
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
                    @Damage,
                    @Modifier,
                    @Action,
                    @Critical,
                    @DirectHit,
                    @Blocked,
                    @Parried,
                    @Subject,
                    @Direction,
                    @ChatCode,
                    @Actions
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

        public void AddCure(CureModel model)
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

        public void AddActor(ActorModel model)
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
                    Job
                )
                VALUES
                (
                    @ActorType,
                    @Name,
                    @Level,
                    @Job
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

        public void AddTimeline(TimelineModel model)
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

        public void CloseTimeline(string name, DateTime endUtc)
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

        public void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            if (_connection != null && _connection.State == ConnectionState.Open)
                _connection.Close();
            _connection = null;
        }

        private void CreateDatabase()
        {
            if (_connection.QueryFirst<int>(@"SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name=@name;", new { name = "Damage" }) > 0)
                return;

            const string tblDamage = @"
                CREATE TABLE Damage
                (
                    Id          INTEGER PRIMARY KEY AUTOINCREMENT,
                    OccurredUtc TEXT NOT NULL,
                    Timestamp   TEXT,
                    Source      TEXT,
                    Target      TEXT,
                    Damage      INT,
                    Modifier    TEXT,
                    Action      TEXT,
                    Critical    INT,
                    DirectHit   INT,
                    Blocked     INT,
                    Parried     INT,
                    Subject     TEXT,
                    Direction   TEXT,
                    ChatCode    TEXT,
                    Actions     TEXT
                );
            ";

            const string tblActor = @"
                CREATE TABLE Actor
                (
                    ActorType       TEXT NOT NULL,
                    Name            TEXT NOT NULL,
                    Level           INT,
                    Job             TEXT
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

            _connection.Execute(tblDamage);
            _connection.Execute(tblActor);
            _connection.Execute(tblTimeline);
            _connection.Execute(tblCure);
        }

        private bool Connect()
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
    }
}