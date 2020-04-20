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
            await ReplyAsync("Heil in 3 sec...").ConfigureAwait(false);
            await Task.Delay(3 * 1000).ConfigureAwait(false);
            foreach (var i in channels)
            {
                await i.SendMessageAsync("Heil").ConfigureAwait(false);
            }
            await ReplyAsync("Hitler in 3 sec...").ConfigureAwait(false);
            await Task.Delay(3 * 1000).ConfigureAwait(false);
            foreach (var i in channels)
            {
                var NoWarningsPlease = i.SendMessageAsync("Hitler").ConfigureAwait(false);
            }
        }

        [Command("react")]
        public async Task ReactWithEmoteAsync()
        {
            var emoji = new Emoji("👍");
            await Context.Message.AddReactionAsync(emoji).ConfigureAwait(false);
        }

        [Command("start")]
        public async Task Start()
        {
            var StartVoteMessage = await Context.Channel
                .SendMessageAsync("The game will start in 42 seconds. Press 👍 to take part")
                .ConfigureAwait(false);
            await StartVoteMessage.AddReactionAsync(new Emoji("👍"))
                .ConfigureAwait(false);
            await Task.Delay(7*1000) //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!1
                .ConfigureAwait(false); 
            var arr = (await StartVoteMessage
                .GetReactionUsersAsync(new Emoji("👍"), 11)
                .FlattenAsync()
                .ConfigureAwait(false))
                .ToArray();
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
            await Context.Channel.SendMessageAsync("Players:").ConfigureAwait(false);
            foreach (var i in arr)
            {
                if (!i.IsBot)
                {
                    await Context.Channel.SendMessageAsync($"{i.Username} is in the game!").ConfigureAwait(false);
                }
            }
            //---------------------------------------------------------------------------------------------------------------------
            var temparr = new IUser[1 + (arr.Length - 1) * 3];
            int p = 0;
            foreach (var i in arr)
            {
                if (i.IsBot)
                {
                    temparr[0] = i;
                }
                else
                {
                    p++;
                    temparr[p] = i;
                    p++;
                    temparr[p] = i;
                    p++;
                    temparr[p] = i;
                }
            }
            arr = temparr;
            Console.WriteLine($"Total players: {arr.Length - 1}");
            //---------------------------------------------------------------------------------------------------------------------
            SocketTextChannel[] chnls = new SocketTextChannel[arr.Length];
            chnls[0] = Context.Channel as SocketTextChannel;
            var channels = Context.Guild.TextChannels;
            foreach (var chnl in channels)
            {
                try
                {
                    if (chnl.Name.StartsWith("player")) 
                    {
                        int num = Convert.ToInt32(chnl.Name.Substring(6));
                        if (0 < num && num < arr.Length)
                        {
                            chnls[num] = chnl;
                            await chnl.SendMessageAsync("Heil!").ConfigureAwait(false);
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
                await SecretHitlerGame.Play(chnls).ConfigureAwait(false);
                await Context.Channel.SendMessageAsync("Well played!").ConfigureAwait(false);
            }
            catch
            {
                Console.WriteLine($"Critical error. Exit game.");
                await Context.Channel.SendMessageAsync("Critical error. Exit game.").ConfigureAwait(false);
            }

        }

        [Command("break")]
        public async Task Break()
        {
            await ReplyAsync("S U C C").ConfigureAwait(false);
        }

        [Command("numbers")]
        public async Task Numbers()
        {
            var NumbersEmoji = new Emoji[11];
            NumbersEmoji[0] = new Emoji("\u0030\uFE0F\u20E3");
            NumbersEmoji[1] = new Emoji("\u0031\uFE0F\u20E3");
            NumbersEmoji[2] = new Emoji("\u0032\uFE0F\u20E3");
            NumbersEmoji[3] = new Emoji("\u0033\uFE0F\u20E3");
            NumbersEmoji[4] = new Emoji("\u0034\uFE0F\u20E3");
            NumbersEmoji[5] = new Emoji("\u0035\uFE0F\u20E3");
            NumbersEmoji[6] = new Emoji("\u0036\uFE0F\u20E3");
            NumbersEmoji[7] = new Emoji("\u0037\uFE0F\u20E3");
            NumbersEmoji[8] = new Emoji("\u0038\uFE0F\u20E3");
            NumbersEmoji[9] = new Emoji("\u0039\uFE0F\u20E3");
            NumbersEmoji[10] = new Emoji("🔟");
            for (int i = 0; i <= 10; i++)
            {
                try
                {
                    await Context.Message.AddReactionAsync(NumbersEmoji[i]).ConfigureAwait(false);
                } catch { };
            }
        }
    }
}
