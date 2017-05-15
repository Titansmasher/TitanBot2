﻿using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TitanBot2.Services.CommandService;
using TitanBot2.TypeReaders;

namespace TitanBot2.Commands.GuildSpecific.Singularity
{
    public class RemoveCommand : Command
    {
        private ulong[] _roleOrder = new ulong[]
        {
            307806171798962177,
            307805963295916032,
            307806447402614785,
            307806702151925760,
            307807022903197698,
            312177555379585024,
        };

        public RemoveCommand(TitanbotCmdContext context, TypeReaderCollection readers) : base(context, readers)
        {
            GuildRestrictions = new ulong[] { 307803032534646785 };

            Calls.AddNew(a => Remove((SocketGuildUser)a[0], (string)a[1]))
                 .WithArgTypes(typeof(SocketGuildUser), typeof(string))
                 .WithItemAsParams(1);

            Description = "Demotes a user down the ranks";
        }
    
        private async Task Remove(SocketGuildUser user, string reason)
        {
            var callingUser = Context.User as SocketGuildUser;
            if (callingUser == null)
            {
                await ReplyAsync("An error occured, please try again later.", ReplyType.Error);
                return;
            }

            var userTopRole = callingUser.Roles.OrderBy(r => r.Position).FirstOrDefault(r => _roleOrder.Contains(r.Id));
            var targetTopRole = user.Roles.OrderBy(r => r.Position).FirstOrDefault(r => _roleOrder.Contains(r.Id));

            if (userTopRole == null)
            {
                await ReplyAsync("You do not have a member rank here!", ReplyType.Error);
                return;
            }

            if (targetTopRole == null)
            {
                await ReplyAsync("They do not have a member rank here! Please give them one of the member roles to use this command on them.", ReplyType.Error);
                return;
            }

            if (userTopRole.Position <= targetTopRole.Position)
            {
                await ReplyAsync("You cannot remove someone who is above or equal to you", ReplyType.Error);
                return;
            }

            await user.RemoveRolesAsync(user.Roles.Where(r => _roleOrder.Contains(r.Id)));

            await TrySend(user.Id, "You have been kicked from the clan for the following reason:\n```" + reason + "```\nWe hope you enjoyed your stay and wish you luck in your future clans!");

            await ReplyAsync($"Removed {user.Mention} from the clan.", ReplyType.Success);
        }
    }
}
