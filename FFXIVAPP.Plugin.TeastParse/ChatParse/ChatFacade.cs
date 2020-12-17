using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.Plugin.TeastParse.Factories;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using NLog;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse.ChatParse
{
    /// <summary>
    /// Handle everything related to chat log item parsing
    /// </summary>
    public interface IChatFacade
    {
        /// <summary>
        /// Parse given line
        /// </summary>
        /// <param name="line">actual chat log item</param>
        void HandleLine(ChatLogItem line);
    }

    internal class ChatFacade : IChatFacade
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly List<ChatCodes> _codes;
        private readonly IRepository _repository;
        private readonly IParseClock _clock;
        private readonly List<UInt64> _knownCodes;

        private readonly List<BaseParse> _parsers;

        public ChatFacade(List<ChatCodes> codes, IActorModelCollection actors, ITimelineCollection timeline, IRepository repository, IDetrimentalFactory detrimentalFactory, IBeneficialFactory beneficialFactory, IActionFactory actionFactory, IParseClock clock)
        {
            //_codes = ioc.Get<List<ChatCodes>>();
            //_repository = ioc.Get<IRepository>();
            _codes = codes;
            _repository = repository;
            _clock = clock;
            _knownCodes = _codes
                            .SelectMany(c => c.Groups)
                            .SelectMany(g => g.Codes)
                            .Select(cc => cc.Key)
                            .ToList();
            var actionParse = new ActionParse(_codes, actionFactory, _repository);
            _parsers = new List<BaseParse>
            {
                //ioc.Instantiate<BattleParse>(), // new BattleParse(_codes, entities, _repository),
                //ioc.Instantiate<Timeline>() //new Timeline(_repository)
                new Timeline(timeline, _clock, _repository),
                actionParse,
                new BattleParse(_codes, actors, timeline, actionParse, _clock, _repository),
                new CureParse(_codes, actors, timeline, actionParse, _clock, _repository),
                new DetrimentalParse(_codes, actors, timeline, detrimentalFactory, actionParse, _clock, _repository),
                new BeneficialParse(_codes, actors, timeline, beneficialFactory, actionParse, _clock, _repository)
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
                {
                    _repository.AddChatLog(new Models.ChatLogLine(0, _clock.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"), line.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), line.Code, line.Line));
                    parser.Handle(code, line);
                }
        }
    }
}