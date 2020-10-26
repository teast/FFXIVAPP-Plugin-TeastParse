using System;
using System.Collections.Generic;
using FFXIVAPP.Common.Core;

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

        // TODO: Load this from main app when I got config fixed
        public static GameLanguage Language => GameLanguage.English;

        public static bool IsYou(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;
            return You[Language].Equals(s, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}