using System.Collections.Generic;
using System.Threading.Tasks;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.Factories;
using FFXIVAPP.Plugin.TeastParse.Models;
using FFXIVAPP.Plugin.TeastParse.Repositories;

namespace FFXIVAPP.Plugin.TeastParse.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IAppLocalization _localization;
        private readonly List<ChatCodes> _codes;
        private readonly ITimelineCollection _timeline;
        private readonly IDetrimentalFactory _detrimentalFactory;
        private readonly IBeneficialFactory _beneficialFactory;
        private readonly IActionFactory _actionFactory;
        private readonly IActorItemHelper _actorItemHelper;
        private readonly IRepositoryFactory _repositoryFactory;
        private IParseContext _active = null;

        public MainViewModel(IAppLocalization localization, ICurrentParseContext current,
            List<ChatCodes> codes, ITimelineCollection timeline,
            IDetrimentalFactory detrimentalFactory, IBeneficialFactory beneficialFactory,
            IActionFactory actionFactory, IActorItemHelper actorItemHelper, IRepositoryFactory repositoryFactory
        )
        {
            _localization = localization;
            Current = current;
            _codes = codes;
            _timeline = timeline;
            _detrimentalFactory = detrimentalFactory;
            _beneficialFactory = beneficialFactory;
            _actionFactory = actionFactory;
            _actorItemHelper = actorItemHelper;
            _repositoryFactory = repositoryFactory;
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

        public async Task LoadParse(string parse)
        {
            if (Active.IsCurrent == false)
            {
                Active.Dispose();
            }

            var context = new ParseContext(parse, _codes, _timeline, _detrimentalFactory, _beneficialFactory, _actionFactory, _actorItemHelper, _repositoryFactory);
            Active = context;
            RaisePropertyChanged(nameof(ActiveParserName));

            await context.Replay();
        }
    }
}