using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace Bot.Modules //this is code where we are writing our commands
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("hello")]//test command 
        public async Task Hello()
        {
            await ReplyAsync("Hello buddy"); //an example how BOT replies
        }
    }
}
