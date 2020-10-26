using FFXIVAPP.Plugin.TeastParse;

namespace FFXIVAPP.Plugin.TeastParse.ViewModels
{
    public class MainViewModel: ViewModelBase
    {
        private readonly IAppLocalization _localization;
        public MainViewModel(IAppLocalization localization)
        {
            _localization = localization;
        }

        public string LabelMain => _localization["Main"];
        public string LabelSettings => _localization["Settings"];
        public string LabelAbout => _localization["About"];
    }
}