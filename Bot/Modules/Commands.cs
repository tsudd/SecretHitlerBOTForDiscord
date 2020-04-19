using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
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
                .SendMessageAsync("The game will start in 42 seconds. Press 👍 to take part");
            await StartVoteMessage.AddReactionAsync(new Emoji("👍"));
            await Task.Delay(7*1000); //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1
            var arr = (await StartVoteMessage
                .GetReactionUsersAsync(new Emoji("👍"), 11)
                .FlattenAsync()).ToArray();
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i].IsBot)
                {
                    SH.DamnMethods.Swap(ref arr[i], ref arr[0]);
                }
            }
            if (!arr[0].IsBot)
            {
                Console.WriteLine("Bot is missing!");
                throw new Exception("Bot is not in the game!");
            }
            await Context.Channel.SendMessageAsync("Players:");
            foreach (var i in arr)
            {
                if (!i.IsBot)
                {
                    await Context.Channel.SendMessageAsync($"{i.Username} is in the game!");
                }
            }

            SocketTextChannel[] chnls = new SocketTextChannel[arr.Length];
            chnls[0] = Context.Channel as SocketTextChannel;
            var channels = Context.Guild.TextChannels;
            foreach (var chnl in channels)
            {
                try
                {
                    if (chnl.Name.StartsWith("player")) {
                        int num = Convert.ToInt32(chnl.Name.Substring(6));
                        if (0 < num && num < arr.Length)
                        {
                            chnls[num] = chnl;
                            await chnl.SendMessageAsync("Heil!");
                        }
                    }
                }
                catch
                {
                    Console.WriteLine($"Whoops! I thought that {chnl.Name} is player channel!");
                }
            }

            try
            {
                var SecretHitlerGame = new SH.Game(Context.Guild, arr);
                SecretHitlerGame.Play(chnls);
            }
            catch
            {
                await Context.Channel.SendMessageAsync("Critical error. Exit game.");
            }

        }

        [Command("break")]
        public async Task Break()
        {
            
        }

    }
}
