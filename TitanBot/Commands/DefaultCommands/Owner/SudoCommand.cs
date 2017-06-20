﻿using Discord;
using System.Threading.Tasks;
using TitanBot.Commands.Models;
using TitanBot.Dependencies;
using TitanBot.Util;

namespace TitanBot.Commands.DefaultCommands.Owner
{
    [RequireOwner]
    class SudoCommand : Command
    {
        private IDependencyFactory Factory { get; }

        public SudoCommand(IDependencyFactory factory)
        {
            Factory = factory;
        }

        [Call]
        async Task SudoAsync(IUser user, [Dense]string command)
        {
            var spoofMessage = new SudoMessage(Message);
            spoofMessage.Author = user;
            spoofMessage.Content = command;
            await ReplyAsync($"Executing `{command}` as {user}", ReplyType.Success);
            await CommandService.ParseAndExecute(spoofMessage);
        }
    }
}
