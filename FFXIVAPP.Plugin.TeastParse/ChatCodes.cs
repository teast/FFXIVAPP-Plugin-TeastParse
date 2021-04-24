using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.Extensions;

namespace FFXIVAPP.Plugin.TeastParse
{
    public class ChatCodes
    {
        public Group[] Groups { get; }
        public string Name { get; }
        public ChatcodeType Type { get; }

        public ChatCodes(string name, ChatcodeType type, IEnumerable<Group> groups)
        {
            Name = name;
            Type = type;
            Groups = groups.ToArray();
        }

        internal static IEnumerable<ChatCodes> FromXmlDocument(XmlDocument xdoc)
        {
            var groups = xdoc.DocumentElement.SelectNodes("/Codes/Group");
            foreach (XmlNode group in groups)
            {
                yield return ChatCodes.FromXmlNode(group);
            }
        }

        private static ChatCodes FromXmlNode(XmlNode group)
        {
            var name = group.Attributes.GetNamedItem("Name").Value;
            var type = (ChatcodeType)Enum.Parse(typeof(ChatcodeType), group.Attributes.GetNamedItem("Type").Value);
            return new ChatCodes(name, type, Group.FromXmlList(group.SelectNodes("Group")));
        }
    }

    public class Group
    {
        public Code[] Codes { get; }

        public string Name { get; }
        public ChatCodeSubject Subject { get; }
        public ChatCodeDirection Direction { get; }
        public ActorType SubjectActorType { get; }
        public ActorType DirectionActorType { get; }

        public Group(string name, ChatCodeSubject subject, ChatCodeDirection direction, IEnumerable<Code> codes)
        {
            Name = name;
            Subject = subject;
            Direction = direction;
            Codes = codes.ToArray();
            SubjectActorType = Subject.ToActorType();
            DirectionActorType = Direction.ToActorType(Subject);
        }

        internal static IEnumerable<Group> FromXmlList(XmlNodeList groups)
        {
            foreach (XmlNode group in groups)
            {
                yield return Group.FromXmlNode(group);
            }
        }

        private static Group FromXmlNode(XmlNode group)
        {
            var name = group.Attributes.GetNamedItem("Name").Value;
            var subject = (ChatCodeSubject)Enum.Parse(typeof(ChatCodeSubject), group.Attributes.GetNamedItem("Subject").Value);
            var direction = (ChatCodeDirection)Enum.Parse(typeof(ChatCodeDirection), group.Attributes.GetNamedItem("Direction").Value);

            return new Group(name, subject, direction, Code.FromXmlList(group.SelectNodes("Code")));
        }
    }

    public class Code
    {
        public ulong Key { get; }
        public string Description { get; }

        public Code(ulong key, string description)
        {
            Key = key;
            Description = description;
        }

        internal static IEnumerable<Code> FromXmlList(XmlNodeList codes)
        {
            foreach (XmlNode code in codes)
            {
                yield return Code.FromXmlNode(code);
            }
        }

        private static Code FromXmlNode(XmlNode code)
        {
            var key = code.Attributes.GetNamedItem("Key").Value;
            var description = code.SelectSingleNode("Description").InnerText?.Trim() ?? "";

            return new Code(Convert.ToUInt64(key, 16), description);
        }
    }

    public enum ChatcodeType
    {
        Actions,
        Beneficial,
        Chat,
        Cure,
        Damage,
        Defeats,
        Detrimental,
        Failed,
        Items,
        Loot,
        System,
        Revived,
        Crafting,
        Mining
    }

    [Flags]
    public enum ChatCodeDirection
    {
        Alliance = 1,
        Engaged = 2,
        Multi = 4,
        Other = 8,
        Party = 16,
        Pet = 32,
        PetAlliance = 64,
        PetOther = 128,
        PetParty = 256,
        Self = 512,
        To = 1024,
        UnEngaged = 2048,
        Unknown = 4096,
        You = 8192,
        /// <summary>
        /// Special enum value that indicate that chatcode is not needed (used in code only to specify all)
        /// </summary>
        DontMatter = 16384
    }

    [Flags]
    public enum ChatCodeSubject
    {
        Alliance = 1,
        Engaged = 2,
        NPC = 4,
        Other = 8,
        Party = 16,
        Pet = 32,
        PetAlliance = 64,
        PetOther = 128,
        PetParty = 256,
        UnEngaged = 512,
        Unknown = 1024,
        You = 2048,
        /// <summary>
        /// Special enum value that indicate that chatcode is not needed (used in code only to specify all)
        /// </summary>
        DontMatter = 4096
    }
}