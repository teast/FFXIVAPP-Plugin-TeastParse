using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using ChatCodesClass = FFXIVAPP.Plugin.TeastParse.ChatCodes;

namespace FFXIVAPP.Plugin.TeastParse
{
    internal static class ResourceReader
    {
        public static List<ChatCodesClass> ChatCodes()
        {
            using (var stream = typeof(Plugin).GetTypeInfo().Assembly.GetManifestResourceStream("FFXIVAPP.Plugin.TeastParse.Resources.ChatCodes.xml"))
            {
                var xdoc = new XmlDocument();
                xdoc.Load(stream);
                return ChatCodesClass.FromXmlDocument(xdoc).ToList();
            }
        }

        private static IEnumerable<FileWithContent> _allTranslationsCache;
        public static IEnumerable<FileWithContent> AllTranslations => _allTranslationsCache ?? (_allTranslationsCache = FetchAllTranslations());
        private static IEnumerable<FileWithContent> FetchAllTranslations()
        {
            var path = "FFXIVAPP.Plugin.TeastParse.Resources.i18n.";
            var assembly = typeof(Plugin).GetTypeInfo().Assembly;
            var resources = assembly.GetManifestResourceNames();
            foreach(var resource in resources.Where(name => name.StartsWith(path)))
            {
                using(var sr = new StreamReader(assembly.GetManifestResourceStream(resource)))
                    yield return new FileWithContent(resource.Replace(path, string.Empty), sr.ReadToEnd());
            }
        }
    }

    internal struct FileWithContent
    {
        public string Name { get; }
        public string Content { get; }

        public FileWithContent(string name, string content)
        {
            Name = name;
            Content = content;
        }
    }
}