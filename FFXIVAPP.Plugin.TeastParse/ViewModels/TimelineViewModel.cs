using System;
using FFXIVAPP.Plugin.TeastParse.Models;

namespace FFXIVAPP.Plugin.TeastParse.ViewModels
{
    public class TimelineViewModel : ViewModelBase
    {
        private string _name;
        private DateTime _startUtc;
        private DateTime? _endUtc;

        public int Index { get;}

        public string Name
        { 
            get => _name;
            set => Set(() => _name = value);
        }

        public DateTime StartUtc
        { 
            get => _startUtc;
            set => Set(() => _startUtc = value);
        }

        public DateTime? EndUtc
        { 
            get => _endUtc;
            set => Set(() => _endUtc = value);
        }

        private readonly TimelineModel _model;

        public TimelineViewModel(TimelineModel timeline)
        {
            _model = timeline;
            Index = _model.Index;
            UpdateValues();
        }

        public void UpdateValues()
        {
            if (Name != _model.Name)
                Name = _model.Name;
            if (StartUtc != _model.StartUtc)
                StartUtc = _model.StartUtc;
            if (EndUtc != _model.EndUtc)
                EndUtc = _model.EndUtc;
        }
    }
}