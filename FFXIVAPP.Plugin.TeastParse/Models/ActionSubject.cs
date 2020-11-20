namespace FFXIVAPP.Plugin.TeastParse.Models
{
    /// <summary>
    /// Contains an "KeyValuePair" mapping for an action that is based on the actions subject value + name of the subject
    /// </summary>
    internal struct ActionSubject
    {
        public ChatCodeSubject Subject { get; }
        public string Name { get; }
        public string Action { get; set; }

        public ActionSubject(ChatCodeSubject subject, string name, string action)
        {
            Subject = subject;
            Name = name;
            Action = action;
        }

        public override int GetHashCode()
        {
            var modifier = 271;
            var hash = 1002569;

            hash += Subject.GetHashCode() * modifier;
            hash += Name.GetHashCode() * modifier;
            hash += Action.GetHashCode() * modifier;

            return hash;
        }

        public override string ToString() => $"({Subject.ToString()}, {Name}, {Action})";
    }
}