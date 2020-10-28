using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using FFXIVAPP.Common.Utilities;
using NLog;

namespace FFXIVAPP.Plugin.TeastParse
{
    public interface IAppLocalization
    {
        string this[string index] { get; }

        bool SetLanguage(CultureInfo culture);
    }

    public class AppLocalization : IAppLocalization
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private const string prefix = "ffxivapp";
        private LocalizationObject _current;

        /// <summary>
        /// Initialize a new <see cref="AppLocalization" /> for fetching translations from an json formatted file
        /// </summary>
        public AppLocalization()
        {
            LoadDefault();
        }

        public string this[string index]
        {
            get
            {
                if (_current == null)
                    return index;
                return _current[index];
            }
        }

        public bool SetLanguage(CultureInfo culture)
        {
            var file = $"{prefix}.{culture.Name.ToLowerInvariant()}.json";
            if (!ResourceReader.AllTranslations.Any(resource => resource.Name == file))
                return false;

            var prev = _current;
            try
            {
                _current = new LocalizationFile(file, culture.Name);
                return true;
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"{nameof(AppLocalization)}: Could not load default language file", ex);
            }

            _current = prev;
            return false;
        }

        private void LoadDefault()
        {
            try
            {
                _current = new LocalizationResource("ffxivapp.json", "default");
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"{nameof(AppLocalization)}: Could not load default language file", ex);
                _current = null;
            }
        }

        /// <summary>
        /// Base class for handling a language.
        /// </summary>
        private abstract class LocalizationObject
        {
            private readonly string _language;
            protected Dictionary<string, string> _strings;

            /// <summary>
            /// Retrieve the translation for string <see cref="index" />.
            /// If there is no translation the string <see cref="index" /> will be return
            /// </summary>
            public string this[string index]
            {
                get
                {
                    if (_strings == null || !_strings.ContainsKey(index))
                    {
                        Logging.Log(Logger, $"Language \"{ _language}\" missing translation for \"{index}\"");
                        return $"[{index}]({_language})";
                    }

                    return _strings[index];
                }
            }

            /// <summary>
            /// Initialize a new instance of <see cref="LocalizationObject" />
            /// </summary>
            /// <param name="json">string containing an json in the format of <see cref="Dictionary<string, string>" /></param>
            /// <param name="language">given translated language</param>
            public LocalizationObject(string language)
            {
                _language = language;
            }
        }

        /// <summary>
        /// Uses <see cref="ResourceReader"/> to fetch translation
        /// </summary>
        private class LocalizationResource : LocalizationObject
        {
            public LocalizationResource(string resourceName, string language) : base(language)
            {
                var content = ResourceReader.AllTranslations.FirstOrDefault(resource => resource.Name == resourceName).Content;
                if (string.IsNullOrEmpty(content))
                    throw new ArgumentException($"{nameof(LocalizationResource)}: localization resource do not exist \"{resourceName}\"");

                _strings = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(content);
            }
        }

        /// <summary>
        /// fetches translation from given file
        /// </summary>
        private class LocalizationFile : LocalizationObject
        {
            public LocalizationFile(string file, string language) : base(language)
            {
                if (!File.Exists(file))
                    throw new ArgumentException($"{nameof(LocalizationFile)}: localization file do not exist \"{file}\"");

                _strings = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(file));
            }
        }
    }
}