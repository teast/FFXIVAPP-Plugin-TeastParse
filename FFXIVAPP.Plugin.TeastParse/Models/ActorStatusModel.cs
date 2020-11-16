using System;

namespace FFXIVAPP.Plugin.TeastParse.Models
{
    /// <summary>
    /// Represents an detrimental/beneficial
    /// </summary>
    public abstract class ActorStatusModel
    {
        /// <summary>
        /// Name of actual detrimental/beneficial
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Name of action that gaved the detrimental/beneficial
        /// </summary>
        public string ActionName { get; }

        /// <summary>
        /// Timestamp from FFXIV when it occured
        /// </summary>
        public string Timestamp { get; }

        /// <summary>
        /// Timestamp from code when it occurred
        /// </summary>
        public DateTime TimeUtc { get; }

        /// <summary>
        /// If the detrimental/beneficial has a fix time, then this should contain when it ends.
        /// </summary>
        public DateTime? LastUtc { get; }

        /// <summary>
        /// who casted/created the detrimetnal
        /// </summary>
        public string Source { get; }

        /// <summary>
        /// Who received teh detrimental/beneficial
        /// </summary>
        public string Target { get; }

        /// <summary>
        /// Potency of the detrimental/beneficial
        /// </summary>
        /// <remarks>
        /// This is needed to do calculation on "actual" damage from the detrimental/beneficial
        /// </remarks>
        public int Potency { get; }

        /// <summary>
        /// Chatcode that created this detrimental/beneficial
        /// </summary>
        public string ChatCode { get; set; }

        /// <summary>
        /// Direction for the detrimental/beneficial
        /// </summary>
        public string Direction { get; set; }

        /// <summary>
        /// Subject for the detrimental/beneficial
        /// </summary>
        public string Subject { get; set; }

        public ActorStatusModel(string name, string actionName, int potency, string timestamp,
            DateTime timeUtc, DateTime? lastUtc, string source, string target,
            string chatCode, string direction, string subject)
        {
            Name = name;
            Timestamp = timestamp;
            TimeUtc = timeUtc;
            Source = source;
            Target = target;
            ActionName = actionName;
            LastUtc = lastUtc;
            Potency = potency;
        }
    }
}