using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using FFXIVAPP.Common.Helpers;
using FFXIVAPP.Common.WPF;
using FFXIVAPP.IPluginInterface;
using FFXIVAPP.Plugin.TeastParse.ViewModels;
using FFXIVAPP.Plugin.TeastParse.Windows;

namespace FFXIVAPP.Plugin.TeastParse
{
    public class Plugin : IPlugin, INotifyPropertyChanged
    {
        public string Copyright => "Copyright © 2020 Niklas Jansson";

        public string Description => "Final Fantasy XIV Battle Parser";

        public string FriendlyName => "TeastParse";

        public string Icon => "Logo.png";

        public string Name => "FFXIVAPP.Plugin.TeastParse";

        public string Notice => "";
        public string Version => AssemblyHelper.Version.ToString();

        //private AppLocalization _localization;
        private Ioc _ioc;
        //private EventSubscriber _eventSubscriber;

        private RealTimeWidget _dpsWidget;
        private RealTimeWidget _dtpsWidget;
        private RealTimeWidget _hpsWidget;

        public TabItem CreateTab()
        {
            var content = new ShellView
            {
                DataContext = _ioc.Instantiate<ShellViewModel>()
            };

            var tabItem = new TabItem
            {
                Header = this.Name,
                Content = content
            };

            var dpsModel = new RealTimeViewModel(_ioc.Get<IAppLocalization>(), RealTimeFocus.Party, RealTimeType.DPS, _ioc.Get<IActorModelCollection>());
            _dpsWidget = new RealTimeWidget()
            {
                DataContext = dpsModel
            };

            var dtpsModel = new RealTimeViewModel(_ioc.Get<IAppLocalization>(), RealTimeFocus.Party, RealTimeType.DTPS, _ioc.Get<IActorModelCollection>());
            _dtpsWidget = new RealTimeWidget()
            {
                DataContext = dtpsModel
            };

            var hpsModel = new RealTimeViewModel(_ioc.Get<IAppLocalization>(), RealTimeFocus.Party, RealTimeType.HPS, _ioc.Get<IActorModelCollection>());
            _hpsWidget = new RealTimeWidget()
            {
                DataContext = hpsModel
            };

            var mainWindow = (Avalonia.Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            _ioc.Get<EventSubscriber>().Subscribe(Host);
            _dpsWidget.Show(mainWindow);
            _dtpsWidget.Show(mainWindow);
            _hpsWidget.Show(mainWindow);

            return tabItem;
        }

        public Dictionary<string, string> Locale
        {
            get
            {
                return new Dictionary<string, string>();
            }
            set { }
        }

        public Exception Trace { get; private set; }

        public IPluginHost Host { get; private set; }
        public MessageBoxResult PopupResult { get; set; }

#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0088

        public void Dispose(bool isUpdating = false)
        {
            _ioc.Get<EventSubscriber>().UnSubscribe(Host);
        }

        public void Initialize(IPluginHost pluginHost)
        {
            // TODO: Update Initialize so it accepts path to the plugin
            Host = pluginHost;


            _ioc = new ParserIoc();
        }
    }
}