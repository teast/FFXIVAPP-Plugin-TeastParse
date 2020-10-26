using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVAPP.Plugin.TeastParse.Models;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using Sharlayan.Core;

namespace FFXIVAPP.Plugin.TeastParse.ChatParse
{
    /// <summary>
    /// Base class for handling chat message parsing
    /// </summary>
    internal abstract class BaseParse
    {
        /// <summary>
        /// An flated version of all chat codses that are known
        /// </summary>
        private ulong[] _codeIds;

        /// <summary>
        /// Database repository
        /// </summary>
        private readonly IRepository _repository;

        /// <summary>
        /// All known chat codes
        /// </summary>
        protected abstract List<ChatCodes> Codes { get; }

        /// <summary>
        /// A flaten version of all chat codes that are known
        /// </summary>
        /// <returns></returns>
        protected ulong[] CodeIds => _codeIds ?? (_codeIds = Codes
            .SelectMany(c => c.Groups)
            .SelectMany(g => g.Codes)
            .Select(cc => cc.Key)
            .ToArray());

        /// <summary>
        /// Check if this instance of <see ref="BaseParse" /> Can handle given chat code
        /// </summary>
        /// <param name="code">chat code</param>
        /// <returns>true if this instance can handle the code</returns>
        public bool CanHandle(ulong code) => CodeIds.Contains(code);

        /// <summary>
        /// Initialize a new instance of <see ref="BaseParse" />
        /// </summary>
        /// <param name="repository">Database repository to use</param>
        public BaseParse(IRepository repository)
        {
            this._repository = repository;
        }

        /// <summary>
        /// Will be called on each code that <see ref="CanHandle" /> approves
        /// </summary>
        /// <param name="code">chat code</param>
        /// <param name="item">the given chatlog item</param>
        public abstract void Handle(ulong code, ChatLogItem item);

        /// <summary>
        /// Store given <see ref="DamageModel" /> in database
        /// </summary>
        /// <param name="model">model to store</param>
        protected void StoreDamage(DamageModel model) => _repository.AddDamage(model);

        /// <summary>
        /// Creates a new Timeline in database based on given model.
        /// </summary>
        /// <param name="model">timeline model to store</param>
        protected void StoreTimeline(TimelineModel model) => _repository.AddTimeline(model);

        /// <summary>
        /// will set end date for given timeline name in database.
        /// </summary>
        /// <param name="name">name of timeline to set end date for</param>
        /// <param name="endUtc">end date to set</param>
        protected void CloseTimeline(string name, DateTime endUtc) => _repository.CloseTimeline(name, endUtc);
 
        /// <summary>
        /// Store given <see ref="CureModel" /> in database
        /// </summary>
        /// <param name="model">model to store</param>
        protected void StoreCure(CureModel model) => _repository.AddCure(model);
   }
}