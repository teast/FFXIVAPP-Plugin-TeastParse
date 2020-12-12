using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.Events;
using FFXIVAPP.Plugin.TeastParse.Models;

namespace FFXIVAPP.Plugin.TeastParse.ViewModels
{
    public class RealTimeViewModel
    {
        private readonly IAppLocalization _localization;
        private readonly RealTimeType _type;

        /// <summary>
        /// Current sort on the party list
        /// </summary>
        private RealTimeSort _sort;

        /// <summary>
        /// Contains all sorting algorithms
        /// </summary>
        private readonly static Dictionary<RealTimeSort, Tuple<Func<RealTimeActorViewModel, object>, bool>> _sortAlgorithms = new Dictionary<RealTimeSort, Tuple<Func<RealTimeActorViewModel, object>, bool>>
        {
            {RealTimeSort.NameAsc, Tuple.Create<Func<RealTimeActorViewModel, object>, bool>((a) => a.Name, false) },
            {RealTimeSort.NameDesc, Tuple.Create<Func<RealTimeActorViewModel, object>, bool>((a) => a.Name, true) },
            {RealTimeSort.DpsAsc, Tuple.Create<Func<RealTimeActorViewModel, object>, bool>((a) => a.DPS, false) },
            {RealTimeSort.DpsDesc, Tuple.Create<Func<RealTimeActorViewModel, object>, bool>((a) => a.DPS, true) },
            {RealTimeSort.PercentAsc, Tuple.Create<Func<RealTimeActorViewModel, object>, bool>((a) => a.PercentOfTimeline, false) },
            {RealTimeSort.PercentDesc, Tuple.Create<Func<RealTimeActorViewModel, object>, bool>((a) => a.PercentOfTimeline, true) },
        };

        public string Title
        {
            get
            {
                switch (_type)
                {
                    case RealTimeType.DPS:
                        return _localization["DPS"];
                    case RealTimeType.DTPS:
                        return _localization["DTPS"];
                    case RealTimeType.HPS:
                        return _localization["HPS"];
                    default:
                        return $"[{_type}]";
                }
            }
        }

        public string LabelName => _localization["Name"];
        public string LabelDPS => _type == RealTimeType.DPS
            ? _localization["DPS"]
            : _type == RealTimeType.DTPS
                ? _localization["DTPS"]
                : _type == RealTimeType.HPS
                    ? _localization["HPS"]
                    : $"[{_type}]";
        public string LabelPercent => _localization["Percent"];

        public SortableObservableCollection<RealTimeActorViewModel> Party { get; set; }

        public void OnSortName()
        {
            _sort = _sort == RealTimeSort.NameAsc ? RealTimeSort.NameDesc : RealTimeSort.NameAsc;
            Party.Sort(_sortAlgorithms[_sort].Item1, _sortAlgorithms[_sort].Item2);
        }

        public void OnSortDPS()
        {
            _sort = _sort == RealTimeSort.DpsDesc ? RealTimeSort.DpsAsc : RealTimeSort.DpsDesc;
            Party.Sort(_sortAlgorithms[_sort].Item1, _sortAlgorithms[_sort].Item2);
        }

        public void OnSortPercent()
        {
            _sort = _sort == RealTimeSort.PercentDesc ? RealTimeSort.PercentAsc : RealTimeSort.PercentDesc;
            Party.Sort(_sortAlgorithms[_sort].Item1, _sortAlgorithms[_sort].Item2);
        }

        public RealTimeViewModel(IAppLocalization localization, RealTimeFocus focus, RealTimeType type, ICurrentParseContext context)
        {
            _localization = localization;
            _type = type;
            _sort = RealTimeSort.NameAsc;
            List<ActorModel> actorList;

            if (focus == RealTimeFocus.Party)
            {
                actorList = context.Actors.GetParty();
                context.Actors.PartyActorAdded += OnActorAdded;
            }
            else if (focus == RealTimeFocus.Alliance)
            {
                actorList = context.Actors.GetAlliance();
                context.Actors.AllianceActorAdded += OnActorAdded;
            }
            else
            {
                actorList = context.Actors.GetAll();
            }

            this.Party = new SortableObservableCollection<RealTimeActorViewModel>(_sortAlgorithms[_sort].Item1, _sortAlgorithms[_sort].Item2, actorList.Select(a => CreateActor(a)));
        }

        private RealTimeActorViewModel CreateActor(ActorModel actor)
        {
            var model = new RealTimeActorViewModel(actor, _type);
            model.PropertyChanged += (s, e) =>
            {
                if (_sort == RealTimeSort.NameAsc || _sort == RealTimeSort.NameDesc)
                    return;

                // Resort if we are sorting on values that can change (dps, percent, etc)
                Party.DoSort();
            };

            return model;
        }

        private void OnActorAdded(object sender, ActorAddedEvent arg) => Party.Add(CreateActor(arg.Actor));

        /// <summary>
        /// All available sorting for the party list
        /// </summary>
        /// <remarks>
        /// Dont forget to update <see cref="_sortAlgorithms" /> if youy add a new sort.
        /// </remarks>
        private enum RealTimeSort
        {
            NameAsc,
            NameDesc,
            DpsAsc,
            DpsDesc,
            PercentAsc,
            PercentDesc
        }
    }

    public enum RealTimeFocus
    {
        Party,
        Alliance,
        All
    }
}