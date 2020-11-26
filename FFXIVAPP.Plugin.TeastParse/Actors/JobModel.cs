using FFXIVAPP.Plugin.TeastParse.Actors.Potency;
using FFXIVAPP.Plugin.TeastParse.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using static Sharlayan.Core.Enums.Actor;

namespace FFXIVAPP.Plugin.TeastParse.Actors
{
    /// <summary>
    /// <see cref="JobModel" /> contains data that is related to specific job an <see cref="ActorModel" /> has.
    /// </summary>
    /// <remarks>
    /// Main purpose of <see cref="JobModel" /> is to keep track on "damage per potency" for given job and actor.
    /// </remarks>
    public class JobModel : ViewModelBase
    {
        #region Fields
        private ActorPotencyFacade _potencyDamage;
        #endregion

        #region Read-Only Properties
        [JsonConverter(typeof(StringEnumConverter))]
        public Job Job { get; }
        public int Level { get; }
        #endregion

        public JobModel(Job job, int level)
        {
            _potencyDamage = new ActorPotencyFacade();
            Job = job;
            Level = level;
        }

        public void StoreDamageDetails(DamageModel model) => _potencyDamage.StoreDamageDetails(model);

        public double DetrimentalDamage(int ticks, int potency) => _potencyDamage.DetrimentalDamage(ticks, potency);
    }
}