using FFXIVAPP.Plugin.TeastParse;

namespace FFXIVAPP.Plugin.TeastParse.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IAppLocalization _localization;
        public SettingsViewModel(IAppLocalization localization)
        {
            _localization = localization;
        }

        public string LabelMainSettings => _localization["Main Settings"];
        public string LabelDPSWidget => _localization["DPS Widget"];
        public string LabelResetSettings => _localization["Reset Settings"];
        public string LabelOpenNow => _localization["Open Now"];
        public string LabelDTPSWidget => _localization["DTPS Widget"];
        public string LabelHPSWidget => _localization["HPS Widget"];
    }
}