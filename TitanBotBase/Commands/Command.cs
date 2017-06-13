﻿using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanBotBase.Database;
using TitanBotBase.Database.Tables;
using TitanBotBase.Dependencies;
using TitanBotBase.Formatter;
using TitanBotBase.Logger;
using TitanBotBase.Scheduler;
using TitanBotBase.Settings;
using TitanBotBase.Util;

namespace TitanBotBase.Commands
{
    public abstract class Command
    {
        private static ConcurrentDictionary<Type, object> _commandLocks = new ConcurrentDictionary<Type, object>();
        private static ConcurrentDictionary<Type, ConcurrentDictionary<ulong?, object>> _guildLocks = new ConcurrentDictionary<Type, ConcurrentDictionary<ulong?, object>>();
        private static ConcurrentDictionary<Type, ConcurrentDictionary<ulong, object>> _channelLocks = new ConcurrentDictionary<Type, ConcurrentDictionary<ulong, object>>();
        private bool HasReplied;
        private IUserMessage awaitMessage;
        private ICommandContext Context { get; set; }

        protected BotClient Bot { get; set; }
        protected ILogger Logger { get; private set; }
        protected ICommandService CommandService { get; private set; }
        protected IUserMessage Message => Context.Message;
        protected DiscordSocketClient Client => Context.Client;
        protected IUser Author => Context.Author;
        protected IMessageChannel Channel => Context.Channel;
        protected SocketSelfUser BotUser => Client.CurrentUser;
        protected IGuild Guild => Context.Guild;
        protected IGuildUser GuildAuthor => Author as IGuildUser;
        protected IGuildChannel GuildChannel => Channel as IGuildChannel;
        protected IGuildUser GuildBotUser => Guild?.GetCurrentUserAsync().Result;
        protected ISettingsManager SettingsManager { get; private set; }
        protected GuildSettings GuildData { get; private set; }
        protected IDatabase Database { get; private set; }
        protected IScheduler Scheduler { get; private set; }
        protected IReplier Replier { get; private set; }
        protected OutputFormatter Formatter { get; private set; }
        protected string[] AcceptedPrefixes => new string[] { BotUser.Mention, BotUser.Username, CommandService.DefaultPrefix, GuildData?.Prefix }.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
        protected object GlobalCommandLock => _commandLocks.GetOrAdd(GetType(), new object());
        protected object GuildCommandLock => _guildLocks.GetOrAdd(GetType(), new ConcurrentDictionary<ulong?, object>()).GetOrAdd(Context.Guild?.Id, new object());
        protected object ChannelCommandLock => _channelLocks.GetOrAdd(GetType(), new ConcurrentDictionary<ulong, object>()).GetOrAdd(Context.Channel.Id, new object());
        protected object InstanceCommandLock { get; } = new object();

        protected string Prefix { get; private set; }
        protected string CommandName { get; private set; }

        public static int TotalCommands { get; private set; } = 0;

        public Command()
        {
            TotalCommands++;
        }

        internal void Install(ICommandContext context, IDependencyFactory factory)
        {
            Context = context;
            Logger = factory.Get<ILogger>();
            Database = factory.Get<IDatabase>();
            Bot = factory.Get<BotClient>();
            CommandService = factory.Get<ICommandService>();
            Scheduler = factory.Get<IScheduler>();
            Replier = factory.WithInstance(this).Construct<IReplier>();
            SettingsManager = factory.Get<ISettingsManager>();
            var userData = Database.AddOrGet(context.Author.Id, () => new UserSetting()).Result;
            Formatter = factory.WithInstance(userData.AltFormat)
                               .WithInstance(context)
                               .Construct<OutputFormatter>();
            if (Guild != null)
                GuildData = Database.Query(conn => conn.GetTable<GuildSettings>().FindById(Guild.Id));
            Prefix = context.Prefix;
            CommandName = context.CommandText;
        }

        private void RegisterReply()
        {
            lock (InstanceCommandLock)
            {
                HasReplied = true;
                if (awaitMessage != null)
                {
                    var temp = awaitMessage;
                    awaitMessage = null;
                    temp.DeleteAsync().Wait();
                }
            }
        }

        protected Task<IUserMessage> ReplyAsync(IMessageChannel channel, string message, ReplyType replyType, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            RegisterReply();
            return Replier.ReplyAsync(channel, Author, message, replyType, isTTS: isTTS, embed: embed, options: options);
        }
        protected IUserMessage Reply(IMessageChannel channel, string message, ReplyType replyType, bool isTTS = false, Embed embed = null, RequestOptions options = null)
        {
            RegisterReply();
            return Replier.Reply(channel, Author, message, replyType, isTTS: isTTS, embed: embed, options: options);
        }

        protected Task<IUserMessage> ReplyAsync(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => ReplyAsync(Channel, message, ReplyType.None, isTTS, embed, options);
        
        protected Task<IUserMessage> ReplyAsync(string message, ReplyType replyType, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => ReplyAsync(Channel, message, replyType, isTTS, embed, options);
        
        protected Task<IUserMessage> ReplyAsync(IMessageChannel channel, string message, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => ReplyAsync(channel, message, ReplyType.None, isTTS, embed, options);
        
        protected Task<IUserMessage> ReplyAsync(IUser user, string message, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => ReplyAsync(user.GetDMChannelAsync().Result, message, ReplyType.None, isTTS, embed, options);
        
        protected Task<IUserMessage> ReplyAsync(IUser user, string message, ReplyType replyType, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => ReplyAsync(user.GetDMChannelAsync().Result, message, replyType, isTTS, embed, options);

        protected IUserMessage Reply(string message, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => Reply(Channel, message, ReplyType.None, isTTS, embed, options);
        
        protected IUserMessage Reply(string message, ReplyType replyType, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => Reply(Channel, message, replyType, isTTS, embed, options);
        
        protected IUserMessage Reply(IMessageChannel channel, string message, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => Reply(channel, message, ReplyType.None, isTTS, embed, options);        
        
        protected IUserMessage Reply(IUser user, string message, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => Reply(user.GetDMChannelAsync().Result, message, ReplyType.None, isTTS, embed, options);
        
        protected IUserMessage Reply(IUser user, string message, ReplyType replyType, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => Reply(user.GetDMChannelAsync().Result, message, replyType, isTTS, embed, options);
    }
}
