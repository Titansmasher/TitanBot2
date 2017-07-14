﻿using Discord;
using System;
using System.Linq;
using System.Threading.Tasks;
using TitanBot.Formatting;

namespace TitanBot.Commands
{
    class ReplyContext : IReplyContext
    {
        private ITextResourceCollection TextResource { get; }
        private ValueFormatter Formatter { get; }
        private IUser User { get; }
        private IMessageChannel Channel { get; }
        private string Message { get; set; } = "";
        private Embed Embed { get; set; }
        private bool IsTTS { get; set; }
        private Func<Exception, Task> Handler { get; set; }
        private RequestOptions Options { get; set; }

        public event OnSendEventHandler OnSend;
        
        public ReplyContext(IMessageChannel channel, IUser user, ITextResourceCollection textResource, ValueFormatter formatter)
        {
            User = user;
            Channel = channel;
            TextResource = textResource;
            Formatter = formatter;
            OnSend += (s, m) => Task.CompletedTask;
        }

        private IReplyContext InlineAction(Action action)
        {
            action();
            return this;
        }

        public IUserMessage Send()
            => SendAsync().Result;

        public async Task<IUserMessage> SendAsync()
        {
            if (string.IsNullOrWhiteSpace(Message) && Embed == null)
                throw new InvalidOperationException("Unable to send a message without an embed or message");

            IUserMessage msg = null;

            try
            {
                msg = await Channel.SendMessageAsync(Message, IsTTS, Embed, Options);
            }
            catch (Exception ex)
            {
                await (Handler ?? (e => Task.CompletedTask))(ex);
            }
            await OnSend(this, msg);
            return msg;
        }

        public IReplyContext WithEmbed(Embed embed)
            => InlineAction(() => Embed = embed);

        public IReplyContext WithHandler(Func<Exception, Task> handler)
            => InlineAction(() => Handler = handler);

        public IReplyContext WithMessage(string message)
            => InlineAction(() => Message = TextResource.GetResource(message));

        public IReplyContext WithMessage(string message, ReplyType replyType)
            => InlineAction(() => Message = TextResource.GetResource(message, replyType));

        public IReplyContext WithMessage(string message, params object[] values)
            => InlineAction(() => Message = TextResource.Format(message, values.Select(v => Formatter.Beautify(v)).ToArray()));

        public IReplyContext WithMessage(string message, ReplyType replyType, params object[] values)
            => InlineAction(() => Message = TextResource.Format(message, replyType, values.Select(v => Formatter.Beautify(v)).ToArray()));

        public IReplyContext WithRequestOptions(RequestOptions options)
            => InlineAction(() => Options = options);

        public IReplyContext WithTTS(bool tts)
            => InlineAction(() => IsTTS = tts);
    }
}
