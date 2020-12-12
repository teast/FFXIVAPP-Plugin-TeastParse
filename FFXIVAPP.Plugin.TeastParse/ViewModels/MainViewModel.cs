using FFXIVAPP.Plugin.TeastParse.Models;

namespace FFXIVAPP.Plugin.TeastParse.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IAppLocalization _localization;
        private IParseContext _active = null;

        public MainViewModel(IAppLocalization localization, ICurrentParseContext current)
        {
            _localization = localization;
            Current = current;
        }

        public IParseContext Current { get; }
        public IParseContext Active
        {
            get => _active ?? Current;
            set => _active = value;
        }

        public string LabelMain => _localization["Main"];
        public string LabelSettings => _localization["Settings"];
        public string LabelAbout => _localization["About"];
        public string LabelLoad => _localization["Load"];
        public string LabelLoadHelp => _localization["Load a previous parse."];
        public string LabelActiveParse => _localization["Active parser:"];

        public string ActiveParserName => Active.Name;

        public void LoadParse(string parse)
        {
            if (Active.IsCurrent == false)
            {
                Active.Dispose();
            }

            Active = new ParseContext(parse);
            RaisePropertyChanged(nameof(ActiveParserName));
        }
    }
}