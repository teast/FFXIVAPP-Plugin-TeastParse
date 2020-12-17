namespace FFXIVAPP.Plugin.TeastParse.Models
{
    public class ChatLogLine
    {
        public long Id { get; }
        public string OccurredUtc { get; }
        public string Timestamp { get; }
        public string ChatCode { get; }
        public string ChatLine { get; set; }

        public ChatLogLine(long id, string occurredUtc, string timestamp, string chatCode, string chatLine)
        {
            Id = id;
            OccurredUtc = occurredUtc;
            Timestamp = timestamp;
            ChatCode = chatCode;
            ChatLine = chatLine;
        }
    }
}