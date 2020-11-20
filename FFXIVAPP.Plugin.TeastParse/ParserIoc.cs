using System;
using System.Collections.Generic;
using System.IO;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.ChatParse;
using FFXIVAPP.Plugin.TeastParse.Factories;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using FFXIVAPP.Plugin.TeastParse.ViewModels;

namespace FFXIVAPP.Plugin.TeastParse
{
    public class ParserIoc : Ioc
    {
        public ParserIoc()
        {
            var database = Path.Combine(Constants.PluginsParsesPath, $"parser{DateTime.Now.ToString("yyyyMMddHHmmss")}.db");
            var connection = $"Data Source={database};Version=3;";
            this.Singelton<Ioc>(() => this);
            this.Singelton<List<ChatCodes>>(() => ResourceReader.ChatCodes());
            this.Singelton<IAppLocalization>(() => new AppLocalization());
            this.Singelton<IRepository>(() => new Repository(connection));
            this.Singelton<IActorItemHelper, ActorItemHelper>();
            this.Singelton<IActorModelCollection, ActorModelCollection>();
            this.Singelton<IChatFacade, ChatFacade>();
            this.Singelton<ITimelineCollection, TimelineCollection>();
            this.Singelton<EventSubscriber>();
            this.Singelton<MainViewModel>();
            this.Singelton<SettingsViewModel>();
            this.Singelton<AboutViewModel>();
            this.Singelton<IDetrimentalFactory, DetrimentalFactory>();
            this.Singelton<IBeneficialFactory, BeneficialFactory>();
            this.Singelton<IActionFactory>(() => new ActionFactory(ResourceReader.Actions()));
        }
    }
}