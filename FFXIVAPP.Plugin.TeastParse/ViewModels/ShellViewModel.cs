using FFXIVAPP.Plugin.TeastParse;

namespace FFXIVAPP.Plugin.TeastParse.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly IAppLocalization _localization;
        public ShellViewModel(IAppLocalization localization, MainViewModel mainViewModel, SettingsViewModel settingsViewModel, AboutViewModel aboutViewModel)
        {
            _localization = localization;

            MainViewModel = mainViewModel;
            SettingsViewModel = settingsViewModel;
            AboutViewModel = aboutViewModel;
        }

        public string LabelMain => _localization["Main"];
        public string LabelSettings => _localization["Settings"];
        public string LabelAbout => _localization["About"];

        public MainViewModel MainViewModel { get; }
        public SettingsViewModel SettingsViewModel { get; }
        public AboutViewModel AboutViewModel { get; }
    }
}