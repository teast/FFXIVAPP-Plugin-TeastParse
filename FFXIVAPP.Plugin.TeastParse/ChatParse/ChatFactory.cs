using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using NLog;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse.ChatParse
{
    /// <summary>
    /// Handle everything related to chat log item parsing
    /// </summary>
    public interface IChatFactory
    {
        /// <summary>
        /// Parse given line
        /// </summary>
        /// <param name="line">actual chat log item</param>
        void HandleLine(ChatLogItem line);
    }

    internal class ChatFactory : IChatFactory
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly List<ChatCodes> _codes;
        private readonly IRepository _repository;
        private readonly List<UInt64> _knownCodes;

        private readonly List<BaseParse> _parsers;

        //public ChatFactory(Ioc ioc) // List<ChatCodes> codes, IActorFactory entities, IRepository repository)
        public ChatFactory(List<ChatCodes> codes, IActorModelCollection actors, ITimelineCollection timeline, IRepository repository)
        {
            //_codes = ioc.Get<List<ChatCodes>>();
            //_repository = ioc.Get<IRepository>();
            _codes = codes;
            _repository = repository;
            _knownCodes = _codes
                            .SelectMany(c => c.Groups)
                            .SelectMany(g => g.Codes)
                            .Select(cc => cc.Key)
                            .ToList();
            _parsers = new List<BaseParse>
            {
                //ioc.Instantiate<BattleParse>(), // new BattleParse(_codes, entities, _repository),
                //ioc.Instantiate<Timeline>() //new Timeline(_repository)
                new BattleParse(_codes, actors, timeline, _repository),
                new Timeline(timeline, _repository),
                new CureParse(_codes, actors, timeline, _repository)
            };
        }

        public void HandleLine(ChatLogItem line)
        {
            var code = Convert.ToUInt64(line.Code, 16);
            if (_knownCodes.Contains(code) == false)
            {
                Logging.Log(Logger, $"Unknown line code [{line.Code}] chat: \"{line.Line}\"");
                return;
            }

            foreach (var parser in _parsers)
                if (parser.CanHandle(code))
                    parser.Handle(code, line);
        }
    }
}