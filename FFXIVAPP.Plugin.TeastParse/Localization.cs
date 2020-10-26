using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        private readonly string _directory;

        private LocalizationFile _current;

        public AppLocalization(string directory)
        {
            var absolute = Path.GetFullPath(directory);
            if (!Directory.Exists(absolute))
                throw new ArgumentException($"{nameof(AppLocalization)}: directory do not exist (\"{directory}\") absolute path: \"{absolute}\".");

            _directory = absolute;

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
            var file = Path.Combine(_directory, prefix, $".{culture.Name.ToLowerInvariant()}.json");
            if (!File.Exists(file))
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
                _current = new LocalizationFile(Path.Combine(_directory, $"{prefix}.json"), "default");
            }
            catch (Exception ex)
            {
                Logging.Log(Logger, $"{nameof(AppLocalization)}: Could not load default language file", ex);
                _current = null;
            }
        }

        private class LocalizationFile
        {
            private readonly string _language;
            private readonly Dictionary<string, string> _strings;

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

            public LocalizationFile(string file, string language)
            {
                _language = language;
                if (!File.Exists(file))
                    throw new ArgumentException($"{nameof(LocalizationFile)}: localization file do not exist \"{file}\"");

                _strings = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(file));
            }
        }
    }
}