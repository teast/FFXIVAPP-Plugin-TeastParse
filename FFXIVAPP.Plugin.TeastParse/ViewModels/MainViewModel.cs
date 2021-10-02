using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.Events;
using FFXIVAPP.Plugin.TeastParse.Factories;
using FFXIVAPP.Plugin.TeastParse.Models;
using FFXIVAPP.Plugin.TeastParse.Repositories;

namespace FFXIVAPP.Plugin.TeastParse.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IAppLocalization _localization;
        private readonly List<ChatCodes> _codes;
        private readonly IDetrimentalFactory _detrimentalFactory;
        private readonly IBeneficialFactory _beneficialFactory;
        private readonly IActionFactory _actionFactory;
        private readonly IRepositoryFactory _repositoryFactory;
        private IParseContext _active = null;

        public MainViewModel(IAppLocalization localization, ICurrentParseContext current,
            List<ChatCodes> codes, IDetrimentalFactory detrimentalFactory, IBeneficialFactory beneficialFactory,
            IActionFactory actionFactory, IRepositoryFactory repositoryFactory
        )
        {
            _localization = localization;
            Current = current;
            _codes = codes;
            _detrimentalFactory = detrimentalFactory;
            _beneficialFactory = beneficialFactory;
            _actionFactory = actionFactory;
            _repositoryFactory = repositoryFactory;
            Timeline = new SortableObservableCollection<TimelineViewModel>(sort => sort.StartUtc);
            Party = new SortableObservableCollection<RealTimeActorViewModel>(t => t.Name, false, Active.Actors.GetParty().Select(_ => CreateActor(_)));
            Alliance = new SortableObservableCollection<RealTimeActorViewModel>(t => t.Name, false, Active.Actors.GetAlliance().Select(_ => CreateActor(_)));
            Monster = new SortableObservableCollection<RealTimeActorViewModel>(t => t.Name, false, Active.Actors.GetMonster().Select(_ => CreateActor(_)));

            Active.Timeline.CurrentTimelineChange += OnCurrentTimelineChange;
            Active.Actors.PartyActorAdded += OnPartyActorAdded;
            Active.Actors.AllianceActorAdded += OnAllianceActorAdded;
            Active.Actors.MonsterActorAdded += OnMonsterActorAdded;
            OnCurrentTimelineChange(null, null);
        }

        private void OnCurrentTimelineChange(object sender, TimelineChangeEvent e)
        {
            Timeline.Clear();
            foreach (var item in Active.Timeline)
            {
                var timeline = Timeline.FirstOrDefault(x => x.Index == item.Index);
                if (timeline == null)
                {
                    timeline = new TimelineViewModel(item);
                    Timeline.Add(timeline);
                }
                else
                {
                    timeline.UpdateValues();
                }
            }
        }

        private bool _loadingParse;
        public bool LoadingParse
        {
            get => _loadingParse;
            set => Set(() => _loadingParse = value);
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
        public string LabelTimeline => _localization["Timeline"];
        public string LabelPartyList => _localization["Party list"];
        public string LabelAllianceList => _localization["Alliance list"];
        public string LabelMonsterList => _localization["Monster list"];

        public string ActiveParserName => Active.Name;

        public SortableObservableCollection<TimelineViewModel> Timeline { get; }
        public SortableObservableCollection<RealTimeActorViewModel> Party { get; }
        public SortableObservableCollection<RealTimeActorViewModel> Alliance { get; }
        public SortableObservableCollection<RealTimeActorViewModel> Monster { get; }
        private int _timelineSelected;
        public int TimelineSelected
        {
            get => _timelineSelected;
            set => Set(() => _timelineSelected = value);
        }

        public async Task LoadParse(string parse)
        {
            LoadingParse = true;
            if (Active.IsCurrent == false)
            {
                Active.Dispose();
            }

            Party.Clear();
            Alliance.Clear();
            Monster.Clear();
            Timeline.Clear();
            Active.Actors.PartyActorAdded -= OnPartyActorAdded;
            Active.Actors.AllianceActorAdded -= OnAllianceActorAdded;
            Active.Actors.MonsterActorAdded -= OnMonsterActorAdded;

            var context = new ParseContext(parse, _codes, _detrimentalFactory, _beneficialFactory, _actionFactory, _repositoryFactory);
            Active = context;
            Active.Actors.PartyActorAdded += OnPartyActorAdded;
            Active.Actors.AllianceActorAdded += OnAllianceActorAdded;
            Active.Actors.MonsterActorAdded += OnMonsterActorAdded;

            Active.Actors.GetParty().ForEach(_ => Party.Add(CreateActor(_)));
            Active.Actors.GetAlliance().ForEach(_ => Alliance.Add(CreateActor(_)));
            Active.Actors.GetMonster().ForEach(_ => Monster.Add(CreateActor(_)));
            OnCurrentTimelineChange(null, null);
            RaisePropertyChanged(nameof(ActiveParserName));

            await context.Replay();
            LoadingParse = false;
        }

        private RealTimeActorViewModel CreateActor(ActorModel actor)
        {
            var model = new RealTimeActorViewModel(actor, RealTimeType.DPS, () => Active.Actors.GetAllActions(actor));
            //model.PropertyChanged += (s, e) =>
            //{
            //};

            return model;
        }

        private void OnPartyActorAdded(object sender, ActorAddedEvent arg) => Party.Add(CreateActor(arg.Actor));
        private void OnAllianceActorAdded(object sender, ActorAddedEvent arg) => Alliance.Add(CreateActor(arg.Actor));
        private void OnMonsterActorAdded(object sender, ActorAddedEvent arg) => Monster.Add(CreateActor(arg.Actor));
    }
}