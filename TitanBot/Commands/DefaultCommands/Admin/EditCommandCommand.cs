﻿using Discord;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitanBot.Commands.DefaultCommands.Admin
{
    [Description("Used to allow people with varying roles or permissions to use different commands.")]
    [Notes("To work out just what permission id you need, give the [permission calculator](https://discordapi.com/permissions.html) a try!")]
    [DefaultPermission(8)]
    [RequireContext(ContextType.Guild)]
    public class EditCommandCommand : Command
    {
        IPermissionManager PermissionManager { get; }
        ICommandContext Context { get; }

        public EditCommandCommand(IPermissionManager permissions, ICommandContext context)
        {
            PermissionManager = permissions;
            Context = context;
        }

        IEnumerable<CallInfo> FindCalls(string[] cmds)
            =>  CommandService.CommandList.SelectMany(c => c.Calls)
                                       .Select(c => (Call: c, Path: c.PermissionKey.Split('.')))
                                       .Where(c => cmds.Count(t => c.Path.Zip(t.Split('.'), (p, v) => p.ToLower() == v.ToLower()).All(a => a)) > 0)
                                       .Select(c => c.Call);

        [Call("SetRole")]
        [Usage("Sets a list of roles required to use each command supplied")]
        async Task SetRoleAsync(string[] cmds, SocketRole[] roles = null)
        {
            var validCalls = FindCalls(cmds);
            
            if (validCalls == null || validCalls.Count() == 0)
            {
                await ReplyAsync("There were no commands that matched those calls.", ReplyType.Error);
                return;
            }

            PermissionManager.SetPermissions(Context, validCalls.ToArray(), null, roles.Select(r => r.Id).ToArray(), null);
            
            await ReplyAsync($"Roles set successfully for {validCalls.Select(c => c.Parent).Distinct().Count()} command(s)!", ReplyType.Success);
        }

        [Call("SetPerm")]
        [Usage("Sets a permission required to use each command supplied")]
        async Task SetPermAsync(string[] cmds, ulong permission)
        {
            var validCalls = FindCalls(cmds);

            if (validCalls == null || validCalls.Count() == 0)
            {
                await ReplyAsync("There were no commands that matched those calls.", ReplyType.Error);
                return;
            }

            PermissionManager.SetPermissions(Context, validCalls.ToArray(), permission, null, null);

            await ReplyAsync($"Permissions set successfully for {validCalls.Select(c => c.Parent).Distinct().Count()} command(s)!", ReplyType.Success);
        }

        [Call("Reset")]
        [Usage("Resets the roles and permissions required to use each command supplied")]
        async Task ResetCommandAsync(string[] cmds)
        {
            var validCalls = FindCalls(cmds);

            if (validCalls == null || validCalls.Count() == 0)
            {
                await ReplyAsync("There were no commands that matched those calls.", ReplyType.Error);
                return;
            }

            PermissionManager.ResetPermissions(Context, validCalls.ToArray());

            await ReplyAsync($"Permissions reset successfully for {validCalls.Select(c => c.Parent).Distinct().Count()} command(s)!", ReplyType.Success);
        }

        [Call("Blacklist")]
        [Usage("Prevents anyone with permissions below the override permissions from using the command in the given channel")]
        async Task BlackListCommandAsync(string[] cmds, IMessageChannel[] channels)
        {
            var validCalls = FindCalls(cmds);

            if (validCalls == null || validCalls.Count() == 0)
            {
                await ReplyAsync("There were no commands that matched those calls.", ReplyType.Error);
                return;
            }

            PermissionManager.SetPermissions(Context, validCalls.ToArray(), null, null, channels.Select(c => c.Id).ToArray());

            await ReplyAsync($"Blacklisted {validCalls.Select(c => c.Parent).Distinct().Count()} call(s) from {channels.Length} channel(s)!", ReplyType.Success);
        }
    }
}