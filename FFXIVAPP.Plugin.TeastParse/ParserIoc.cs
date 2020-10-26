using System;
using System.Collections.Generic;
using FFXIVAPP.Plugin.TeastParse.Actors;
using FFXIVAPP.Plugin.TeastParse.ChatParse;
using FFXIVAPP.Plugin.TeastParse.Repositories;
using FFXIVAPP.Plugin.TeastParse.ViewModels;

namespace FFXIVAPP.Plugin.TeastParse
{
    public class ParserIoc : Ioc
    {
        public ParserIoc()
        {
            var connection = $"Data Source=./parser{DateTime.Now.ToString("yyyyMMddHHmmss")}.db;Version=3;";
            this.Singelton<Ioc>(() => this);
            this.Singelton<List<ChatCodes>>(() => ResourceReader.ChatCodes());
            this.Singelton<IAppLocalization>(() => new AppLocalization("i18n"));
            this.Singelton<IRepository>(() => new Repository(connection));
            this.Singelton<IActorItemHelper, ActorItemHelper>();
            this.Singelton<IActorModelCollection, ActorModelCollection>();
            this.Singelton<IChatFactory, ChatFactory>();
            this.Singelton<ITimelineCollection, TimelineCollection>();
            this.Singelton<EventSubscriber>();
            this.Singelton<MainViewModel>();
            this.Singelton<SettingsViewModel>();
            this.Singelton<AboutViewModel>();
        }
    }
}