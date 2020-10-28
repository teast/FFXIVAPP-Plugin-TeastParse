using System.ComponentModel;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace FFXIVAPP.Plugin.TeastParse
{
    public class Settings : INotifyPropertyChanged
    {
        private static Settings _default;
        public static Settings Default => _default ?? (_default = new Settings());

        public Settings()
        {

        }

        private bool _enableHelpLabels;

        [JsonIgnore]
        public bool EnableHelpLabels
        {
            get => _enableHelpLabels;
            set => Set(ref _enableHelpLabels, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T field, T value, [CallerMemberName] string name = "")
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}