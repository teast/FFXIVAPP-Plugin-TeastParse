using FFXIVAPP.Plugin.TeastParse.Actors;

namespace FFXIVAPP.Plugin.TeastParse.Models
{
    /// <summary>
    /// Represents an given damage that have been done.
    /// </summary>
    public struct DamageModel
    {
        /// <summary>
        /// Timestamp when it occurred (in UTC)
        /// </summary>
        public string OccurredUtc { get; set; }
        /// <summary>
        /// Timestamp from Shalayan
        /// </summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// Name of the actor that did the damage
        /// </summary>
        public string Source { get; set; }
        /// <summary>
        /// Name of the actor that received the damage
        /// </summary>
        public string Target { get; set; }
        public ulong Damage { get; set; }
        public string Modifier { get; set; }
        public string Action { get; set; }
        public bool Critical { get; set; }
        public bool DirectHit { get; set; }
        public bool Blocked { get; set; }
        public bool Parried { get; set; }

        /// <summary>
        /// This is used to determ what initial damage an detrimental attack had
        /// </summary>
        public ulong? InitDmg { get; set; }

        /// <summary>
        /// This is used to determ when an detrimental attack was ended
        /// </summary>
        public string EndTimeUtc { get; set; }

        /// <summary>
        /// Chat codes group <see cref="ChatCodeSubject" /> as string
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Chat codes group <see cref="ChatCodeDirection" /> as string
        /// </summary>
        public string Direction { get; set; }
        public string ChatCode { get; set; }

        /// <summary>
        /// True if this damage line is an detrimental damage.
        /// </summary>
        /// <remarks>
        /// Detrimental damage gets calculated different because it
        /// is more dynamic calculated due to no damage in logs
        /// </remarks>
        public bool IsDetrimental { get; set; }

        /// <summary>
        /// Name of action. If null then an auto-attack damage
        /// </summary>
        public string Actions { get; set; }
    }
}