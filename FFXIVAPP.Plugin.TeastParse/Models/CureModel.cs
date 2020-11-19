namespace FFXIVAPP.Plugin.TeastParse.Models
{
    public class CureModel
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

        /// <summary>
        /// The amount of cure
        /// </summary>
        public ulong Cure { get; set; }

        /// <summary>
        /// Any modifier info
        /// </summary>
        public string Modifier { get; set; }

        /// <summary>
        /// what action the cure was from
        /// </summary>
        public string Action { get; set; }
        /// <summary>
        /// If it was an critical
        /// </summary>
        public bool Critical { get; set; }

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
        /// Name of action. If null then an auto-attack damage
        /// </summary>
        public string Actions { get; set; }
    }
}