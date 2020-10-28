using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using static System.Environment;

namespace FFXIVAPP.Plugin.TeastParse
{
    public static class Constants
    {
        public static Dictionary<GameLanguageEnum, string> You => new Dictionary<GameLanguageEnum, string> {
            {GameLanguageEnum.English, "you"},
            {GameLanguageEnum.German, "du"},
            {GameLanguageEnum.France, "vous"},
            {GameLanguageEnum.Chinese, "you"}
        };

        public static Dictionary<GameLanguageEnum, string[]> The => new Dictionary<GameLanguageEnum, string[]> {
            {GameLanguageEnum.English, new [] { "the" }},
            {GameLanguageEnum.German, new [] { "du", "deiner", "dir", "der", "dich", "das", "die", "den" }},
            {GameLanguageEnum.France, new [] { "las", "les", "laes" }}
        };

        public static GameLanguageEnum GameLanguage { get; set; }

        public static Dictionary<string, string> AutoTranslate { get; internal set; }
        public static Dictionary<string, string[]> Colors { get; internal set; }
        public static CultureInfo CultureInfo { get; internal set; }
        public static string CharacterName { get; internal set; }
        public static string ServerName { get; internal set; }

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
            return You[GameLanguage].Equals(s, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}