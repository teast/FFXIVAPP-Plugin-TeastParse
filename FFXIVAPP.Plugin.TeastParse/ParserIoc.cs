using System.Collections.Generic;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.Factories;
using FFXIVAPP.Plugin.TeastParse.Models;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using FFXIVAPP.Plugin.TeastParse.ViewModels;

namespace FFXIVAPP.Plugin.TeastParse
{
    public class ParserIoc : Ioc
    {
        public ParserIoc()
        {
            this.Singelton<Ioc>(() => this);
            this.Singelton<List<ChatCodes>>(() => ResourceReader.ChatCodes());
            this.Singelton<IAppLocalization>(() => new AppLocalization());
            this.Singelton<IRepositoryFactory, RepositoryFactory>();
            this.Singelton<IParseClock, ParseClockReal>();
            this.Singelton<ICurrentParseContext, CurrentParseContext>();
            this.Singelton<IActorItemHelper, ActorItemHelper>();
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