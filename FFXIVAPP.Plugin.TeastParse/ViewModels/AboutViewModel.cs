using FFXIVAPP.Plugin.TeastParse;

namespace FFXIVAPP.Plugin.TeastParse.ViewModels
{
    public class AboutViewModel: ViewModelBase
    {
        private readonly IAppLocalization _localization;
        public AboutViewModel(IAppLocalization localization)
        {
            _localization = localization;
        }

        public string LabelMain => _localization["Main"];
        public string LabelSettings => _localization["Settings"];
        public string LabelAbout => _localization["About"];
    }
}