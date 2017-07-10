﻿using Discord;
using Discord.Net;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TitanBot.Commands;
using TitanBot.Storage;
using TitanBot.Dependencies;
using TitanBot.DiscordHandlers;
using TitanBot.Downloader;
using TitanBot.Formatter;
using TitanBot.Logging;
using TitanBot.Scheduling;
using TitanBot.Settings;
using TitanBot.TypeReaders;
using TitanBot.Util;
using TitanBot.TextResource;

namespace TitanBot
{
    public class BotClient : IDisposable
    {
        public DiscordSocketClient DiscordClient { get; private set; }
        public ILogger Logger { get; private set; }
        public IDatabase Database { get; private set; }
        public IDependencyFactory DependencyFactory { get; private set; }
        public IScheduler Scheduler { get; private set; }
        public ICommandService CommandService { get; private set; }
        public ITypeReaderCollection TypeReaders { get; private set; }
        public ISettingsManager SettingsManager { get; private set; }
        public ITextResourceManager TextResourceManager { get; private set; }
        public GlobalSetting GlobalSettings => SettingsManager.GlobalSettings;
        public IReadOnlyList<ulong> Owners => GlobalSettings.Owners.Concat(new ulong[] { DiscordClient.GetApplicationInfoAsync().Result.Owner.Id })
                                                                   .ToList().AsReadOnly();
        public Task UntilOffline => Task.Run(async () => { while (DiscordClient.LoginState != LoginState.LoggedOut) { await Task.Delay(10); } });
        private List<DiscordHandlerBase> Handlers { get; } = new List<DiscordHandlerBase>();
        public Type[] DefaultCommands => Assembly.GetAssembly(GetType()).GetTypes().Where(t => t.IsSubclassOf(typeof(Command))).ToArray();

        private ManualResetEvent readyEvent = new ManualResetEvent(false);

        public BotClient() : this(null, null) { }

        public BotClient(Action<IDependencyFactory> mapper) : this(null, mapper) { }

        public BotClient(IDependencyFactory factory, Action<IDependencyFactory> mapper)
        { 
            mapper = mapper ?? (f => { });
            DependencyFactory = factory ?? new DependencyFactory();
            
            MapDefaults();
            mapper(DependencyFactory);

            Logger = DependencyFactory.GetOrStore<ILogger>();
            DiscordClient = DependencyFactory.GetOrStore<DiscordSocketClient>();
            TypeReaders = DependencyFactory.GetOrStore<ITypeReaderCollection>();
            Database = DependencyFactory.GetOrStore<IDatabase>();
            SettingsManager = DependencyFactory.GetOrStore<ISettingsManager>();
            Scheduler = DependencyFactory.GetOrStore<IScheduler>();
            CommandService = DependencyFactory.GetOrStore<ICommandService>();
            DependencyFactory.GetOrStore<IDownloader>();
            TextResourceManager = DependencyFactory.GetOrStore<ITextResourceManager>();

            SubscribeEvents();

            InstallHandlers(Assembly.GetExecutingAssembly());
        }

        public void MapDefaults()
        {
            DependencyFactory.Store(this);
            DependencyFactory.TryMap<ILogger, Logger>();
            DependencyFactory.TryMap<IDatabase, Database>();
            DependencyFactory.TryMap<IScheduler, Scheduler>();
            DependencyFactory.TryMap<ICommandService, CommandService>();
            DependencyFactory.TryMap<IReplier, Replier>();
            DependencyFactory.TryMap<ICommandContext, CommandContext>();
            DependencyFactory.TryMap<ITypeReaderCollection, TypeReaderCollection>();
            DependencyFactory.TryMap<IPermissionManager, PermissionManager>();
            DependencyFactory.TryMap<ISettingsManager, SettingsManager>();
            DependencyFactory.TryMap<IDownloader, CachedDownloader>();
            DependencyFactory.TryMap<IEditableSettingGroup, EditableSettingGroup>();
            DependencyFactory.TryMap(typeof(IEditableSettingBuilder<>), typeof(EditableSettingBuilder<>));
            DependencyFactory.TryMap<ITextResourceManager, TextResourceManager>();
        }

        public void InstallHandlers(Assembly assembly)
        {
            var handlers = assembly.GetTypes()
                                   .Where(t => t.IsSubclassOf(typeof(DiscordHandlerBase)));
            foreach (var handler in handlers)
            {
                if (DependencyFactory.TryConstruct(handler, out object obj))
                    Handlers.Add((DiscordHandlerBase)obj);
            }
        }

        private void SubscribeEvents()
        {
            DiscordClient.Ready += () => Task.Run(() => readyEvent.Set());
            DiscordClient.Log += m => Logger.LogAsync(DiscordUtil.ToLoggable(m));
        }

        public Task StartAsync()
            => StartAsync(x => GlobalSettings.Token);
        public Task StartAsync(string token)
            => StartAsync(x => token);
        public async Task StartAsync(Func<string, string> tokenInput)
        {
            var token = tokenInput(GlobalSettings.Token) ?? GlobalSettings.Token;
            token = string.IsNullOrWhiteSpace(token) ? GlobalSettings.Token : token;
            GlobalSettings.Token = token;
            if (DiscordClient.LoginState != LoginState.LoggedOut)
                return;
            try
            {
                await DiscordClient.LoginAsync(TokenType.Bot, token);
            }
            catch (HttpException ex)
            {
                if (ex.HttpCode == HttpStatusCode.Unauthorized)
                    GlobalSettings.Token = null;
                throw;
            }
            await DiscordClient.StartAsync();
            readyEvent.WaitOne();
            readyEvent.Reset();

            //await Scheduler.StartAsync();
        }

        public async Task StopAsync()
        {
            await Scheduler.StopAsync();
            await DiscordClient.LogoutAsync();
        }

        public void Dispose()
        {
            Database?.Dispose();
            DiscordClient?.Dispose();
            DependencyFactory?.Dispose();
        }
    }
}