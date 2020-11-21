namespace FFXIVAPP.Plugin.TeastParse.Models
{
    /// <summary>
    /// Represents an given damage that have been done.
    /// </summary>
    public struct DamageModel
    {
        private bool? _isCombo;
        private int? _potency;

        /// <summary>
        /// Timestamp when it occurred (in UTC)
        /// </summary>
        public string OccurredUtc { get; }
        /// <summary>
        /// Timestamp from Shalayan
        /// </summary>
        public string Timestamp { get; }

        /// <summary>
        /// Name of the actor that did the damage
        /// </summary>
        public string Source { get; }
        /// <summary>
        /// Name of the actor that received the damage
        /// </summary>
        public string Target { get; }
        public ulong Damage { get; }
        public string Modifier { get; }
        public ActionModel Action { get; }
        public string ActionName => Action?.Name;
        public bool Critical { get; }
        public bool DirectHit { get; }
        public bool Blocked { get; }
        public bool Parried { get; }
        public bool IsCombo
        {
            get => _isCombo ?? Action?.IsCombo ?? false;
            set => _isCombo = value;
        }
        public int Potency
        {
            get => _potency ?? Action?.Potency ?? 0;
            set => _potency = value;
        }

        /// <summary>
        /// This is used to determ what initial damage an detrimental attack had
        /// </summary>
        public ulong? InitDmg { get; }

        /// <summary>
        /// This is used to determ when an detrimental attack was ended
        /// </summary>
        public string EndTimeUtc { get; }

        /// <summary>
        /// Chat codes group <see cref="ChatCodeSubject" /> as string
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// Chat codes group <see cref="ChatCodeDirection" /> as string
        /// </summary>
        public string Direction { get; }
        public string ChatCode { get; }

        /// <summary>
        /// True if this damage line is an detrimental damage.
        /// </summary>
        /// <remarks>
        /// Detrimental damage gets calculated different because it
        /// is more dynamic calculated due to no damage in logs
        /// </remarks>
        public bool IsDetrimental { get; }

        public DamageModel(
            string occurredUtc,
            string timestamp,
            string source,
            string target,
            ulong damage,
            string modifier,
            ActionModel action,
            bool critical,
            bool directHit,
            bool blocked,
            bool parried,
            ulong? initDmg,
            string endTimeUtc,
            string subject,
            string direction,
            string chatCode,
            bool isDetrimental
        )
        {
            _isCombo = null;
            _potency = null;
            OccurredUtc = occurredUtc;
            Timestamp = timestamp;
            Source = source;
            Target = target;
            Damage = damage;
            Modifier = modifier;
            Action = action;
            Critical = critical;
            DirectHit = directHit;
            Blocked = blocked;
            Parried = parried;
            InitDmg = initDmg;
            EndTimeUtc = endTimeUtc;
            Subject = subject;
            Direction = direction;
            ChatCode = chatCode;
            IsDetrimental = isDetrimental;
        }
    }
}