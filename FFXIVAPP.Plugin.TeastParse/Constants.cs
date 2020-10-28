using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using static System.Environment;

namespace FFXIVAPP.Plugin.TeastParse
{
    public static class Constants
    {
        public static Dictionary<GameLanguage, string> You => new Dictionary<GameLanguage, string> {
            {GameLanguage.English, "you"},
            {GameLanguage.German, "du"},
            {GameLanguage.France, "vous"},
            {GameLanguage.Chinese, "you"}
        };

        public static Dictionary<GameLanguage, string[]> The => new Dictionary<GameLanguage, string[]> {
            {GameLanguage.English, new [] { "the" }},
            {GameLanguage.German, new [] { "du", "deiner", "dir", "der", "dich", "das", "die", "den" }},
            {GameLanguage.France, new [] { "las", "les", "laes" }}
        };

        public static GameLanguage Language { get; set; }

        public static Dictionary<string, string> AutoTranslate { get; internal set; }
        public static Dictionary<string, string[]> Colors { get; internal set; }
        public static CultureInfo CultureInfo { get; internal set; }
        public static string CharacterName { get; internal set; }
        public static string ServerName { get; internal set; }
        public static bool EnableHelpLabels { get; internal set; }

        public static string PluginsSettingsPath
        {
            get
            {
                var path = Path.Combine(Environment.GetFolderPath(SpecialFolder.MyDocuments), "FFXIVAPP", "Plugins", "TeastParse");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        public static string PluginsParsesPath
        {
            get
            {
                var path = Path.Combine(PluginsSettingsPath, "parses");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                return path;
            }
        }

        public static bool IsYou(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            return You[Language].Equals(s, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}