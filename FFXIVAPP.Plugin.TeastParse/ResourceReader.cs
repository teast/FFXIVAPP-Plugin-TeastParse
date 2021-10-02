using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Avalonia.Media.Imaging;
using FFXIVAPP.Common.Utilities;
using FFXIVAPP.ResourceFiles;
using NLog;
using ChatCodesClass = FFXIVAPP.Plugin.TeastParse.ChatCodes;

namespace FFXIVAPP.Plugin.TeastParse
{
    internal static class ResourceReader
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static List<ChatCodesClass> ChatCodes()
        {
            using (var stream = typeof(Plugin).GetTypeInfo().Assembly.GetManifestResourceStream("FFXIVAPP.Plugin.TeastParse.Resources.ChatCodes.xml"))
            {
                var xdoc = new XmlDocument();
                xdoc.Load(stream);
                return ChatCodesClass.FromXmlDocument(xdoc).ToList();
            }
        }

        public static string Actions()
        {
            using (var sr = new StreamReader(typeof(Plugin).GetTypeInfo().Assembly.GetManifestResourceStream("FFXIVAPP.Plugin.TeastParse.Resources.actions.json")))
            {
                return sr.ReadToEnd();
            }
        }

        public static Bitmap GetActionIcon(string iconName)
        {
            try
            {
                const string name = "FFXIVAPP.Plugin.TeastParse.Resources.actions.";

                var assembly = typeof(Plugin).Assembly;
                var t = assembly.GetManifestResourceNames();
                return new Bitmap(assembly.GetManifestResourceStream($"{name}{iconName}.png"));
            }
            catch(Exception)
            {
                Logging.Log(Logger, $"Exception reading action icon \"{iconName}\".");
                return Game.Unknown;
            }
        }

        private static IEnumerable<FileWithContent> _allTranslationsCache;
        public static IEnumerable<FileWithContent> AllTranslations => _allTranslationsCache ?? (_allTranslationsCache = FetchAllTranslations());
        private static IEnumerable<FileWithContent> FetchAllTranslations()
        {
            var path = "FFXIVAPP.Plugin.TeastParse.Resources.i18n.";
            //var path = "FFXIVAPP.Plugin.TeastParse.";
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