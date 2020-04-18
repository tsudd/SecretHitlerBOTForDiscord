using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace Bot.Modules //this is code where we are writing our commands
{
    public class Commands : ModuleBase<SocketCommandContext>
    {

        [Command("hello")]//test command 
        public async Task Hello()
        {
            await ReplyAsync("Hello buddy").ConfigureAwait(false); //an example how BOT replies
        }

        [Command("saymyname")]
        public async Task SayMyName()
        {
            SocketUser user = Context.User;
            await ReplyAsync($"Hello, {user.Username} ({user.Id})").ConfigureAwait(false);
        }

        [Command("getchannelid")]
        public async Task GetChannelId()
        {
            var ch = Context.Channel;
            await ReplyAsync($"Heil, {ch.Name} ({ch.Id})").ConfigureAwait(false);
        }

        [Command("sieg")]
        public async Task Sieg()
        {
            var channels = Context.Guild.TextChannels;
            foreach(var i in channels)
            {
                try
                {
                    var chnl = Context.Guild.GetChannel(i.Id) as IMessageChannel;
                    int x = 1;
                    if (chnl.Name.Equals("player" + x.ToString()))
                    {
                        await chnl.SendMessageAsync("Heil!");
                    }
                }
                catch
                {
                    Console.WriteLine($"Whoops! I thought that {i.Name} is text channel!");
                }
            }
        }

        [Command("react")]
        public async Task ReactWithEmoteAsync()
        {
            var emoji = new Emoji("👍");
            await Context.Message.AddReactionAsync(emoji);
        }

        [Command("start")]
        public async Task Start()
        {
            var StartVoteMessage = await Context.Channel
                .SendMessageAsync("The game is about to start. Press 👍 to take part");
            await StartVoteMessage.AddReactionAsync(new Emoji("👍"));
            if (StartVoteMessage == null)
            {
                await Context.Channel.SendMessageAsync("Fuck this shit");
            }
            await Task.Delay(10000);
            var arr = await StartVoteMessage
                .GetReactionUsersAsync(new Emoji("👍"), 11)
                .FlattenAsync();
            //var arr = await StartVoteMessage.GetReactionUsersAsync(new Emoji("👍"), 11).FlattenAsync();

            await Context.Channel.SendMessageAsync("Players:");
            var it = arr.GetEnumerator();
            while (it.MoveNext())
            {
                var i = it.Current;
                if (!i.IsBot)
                {
                    await Context.Channel.SendMessageAsync($"{i.Username} is in the game!");
                }
            }
            it.Reset();

            ...
        }

        [Command("break")]
        public async Task Break()
        {
            
        }

    }
}
