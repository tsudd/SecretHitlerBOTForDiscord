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

        [Command("sieg")]
        public async Task Sieg()
        {
            var channels = Context.Guild.TextChannels;
            foreach (var i in channels)
            {
                await i.SendMessageAsync("Heil").ConfigureAwait(false);
            }
            foreach (var i in channels)
            {
                _ = i.SendMessageAsync("Hitler").ConfigureAwait(false);
            }
        }

        [Command("test")]
        public async Task Test()
        {
            await Context.Channel.SendMessageAsync("TEEEST").ConfigureAwait(false);


        }

        [Command("clear")]
        public async Task Clear(string prefix = "player")
        {
            var AllChannels = Context.Guild.Channels.ToArray();
            foreach (var i in AllChannels)
            {
                try 
                {
                    if (i.Name.StartsWith(prefix))
                    {
                        await i.DeleteAsync().ConfigureAwait(false);
                    }
                }
                catch { }
            }
            var AllRoles = Context.Guild.Roles.ToArray();
            foreach (var i in AllRoles)
            {
                try
                {
                    if (i.Name.StartsWith(prefix))
                    {
                        await i.DeleteAsync().ConfigureAwait(false);
                    }
                }
                catch { }
            }
        }

        [Command("create")]
        public async Task Create(int players, string prefix = "player")
        {
            await Clear(prefix).ConfigureAwait(false);
            for (int i = 1; i <= players; i++)
            {
                await Context.Guild.CreateTextChannelAsync(prefix + i.ToString()).ConfigureAwait(false);
            }
        }

        [Command("play")]
        public async Task PlaySecretHitler(int delay = 30)
        {
            await Create(10).ConfigureAwait(false);
            await 
                (await Context.Channel.SendMessageAsync($"/_game {delay}").ConfigureAwait(false))
                .DeleteAsync().ConfigureAwait(false);
        }

        [Command("_game")]
        public async Task Game(int delay = 42, string s = null, int TestMultiplier = 1)
        {
            //await Context.Client.StopAsync().ConfigureAwait(false);
            //await Context.Client.StartAsync().ConfigureAwait(false);

            const string prefix = "player";

            var StartVoteMessage = await Context.Channel
                .SendMessageAsync($"The game will start in {delay} seconds. Press 👍 to take part")
                .ConfigureAwait(false);
            await StartVoteMessage.AddReactionAsync(new Emoji("👍"))
                .ConfigureAwait(false);
            await Task.Delay(delay * 1000)
                .ConfigureAwait(false);
            var arr = (await StartVoteMessage
                .GetReactionUsersAsync(new Emoji("👍"), 11)
                .FlattenAsync()
                .ConfigureAwait(false))
                .ToArray();
            ulong BotId = Context.Client.Rest.CurrentUser.Id;
            for (int i = 1; i < arr.Length; i++)
            {
                if (arr[i].Id == BotId)
                {
                    SH.DamnMethods.Swap(ref arr[i], ref arr[0]);
                }
            }
            if (arr[0].Id != BotId)
            {
                Console.WriteLine("Bot is missing!");
                await Context.Channel.SendMessageAsync("Where am I?..").ConfigureAwait(false);
                return;
            }
            Console.WriteLine("Players:");
            foreach (var i in arr)
            {
                if (i.Id != BotId)
                {
                    Console.WriteLine($"{i.Username} is in the game!");
                }
            }

            // --- Игра за множество игроков при тестировании --- 
            var temparr = new IUser[1 + (arr.Length - 1) * TestMultiplier];
            int p = 0;
            foreach (var i in arr)
            {
                if (i.Id == BotId)
                {
                    temparr[0] = i;
                }
                else
                {
                    for(int j = 0; j < TestMultiplier; j++)
                    {
                        p++;
                        temparr[p] = i;
                    }
                }
            }
            arr = temparr;
            // --- Игра за множество игроков при тестировании --- 

            SH.DamnMethods.Shuffle(arr, 1, arr.Length);

            SocketTextChannel GetTextChannelByName(string name)
            {
                var AllChannels = Context.Guild.TextChannels.ToArray();
                foreach (var i in AllChannels)
                {
                    if (i.Name.Equals(name))
                    {
                        return i;
                    }
                }
                return null;
            }
            for (int i = arr.Length; i <= 10; i++)
            {
                var chat = GetTextChannelByName(prefix + i.ToString());
                if (chat != null)
                {
                    await chat.DeleteAsync().ConfigureAwait(false);
                }
            }

            var chats = new SocketTextChannel[arr.Length];
            var rnd = new Random();
            for (int i = 1; i < arr.Length; i++)
            {
                var role = await Context.Guild.CreateRoleAsync(prefix + i.ToString(), null, 
                    new Color(rnd.Next(256), rnd.Next(256), rnd.Next(256)), false, null)
                    .ConfigureAwait(false);
                await Context.Guild.GetUser(arr[i].Id).AddRoleAsync(role).ConfigureAwait(false);

                chats[i] = GetTextChannelByName(prefix + i.ToString());
                if (chats[i] == null)
                {
                    Console.WriteLine($"Not enough reserved seats!");
                    await Context.Channel.SendMessageAsync($"Please, use command /create {arr.Length - 1}").ConfigureAwait(false);
                    return;
                }

                await chats[i].AddPermissionOverwriteAsync(
                    Context.Guild.EveryoneRole, 
                    OverwritePermissions.DenyAll(chats[i]))
                    .ConfigureAwait(false);
                await chats[i].AddPermissionOverwriteAsync(role, OverwritePermissions.DenyAll(chats[i]).Modify(
                    viewChannel: PermValue.Allow,
                    readMessageHistory: PermValue.Allow,
                    //sendMessages: PermValue.Allow,
                    addReactions: PermValue.Allow))
                    .ConfigureAwait(false);
            }

            Console.WriteLine($"Total players: {arr.Length - 1}");
            var chnls = new SocketTextChannel[arr.Length];
            chnls[0] = Context.Channel as SocketTextChannel;
            for (int i = 1; i < arr.Length; i++)
            {
                chnls[i] = Context.Guild.GetTextChannel(chats[i].Id);
            }

            try
            {
                Console.WriteLine($"TEST0");
                bool logs = (s != null ? true : false);
                Console.WriteLine($"There will be {(logs ? "" : "NO")} secret logs.");
                var SecretHitlerGame = new SH.Game(Context.Guild, logs);
                Console.WriteLine($"TEST13");
                await SecretHitlerGame.Play(arr, chnls).ConfigureAwait(false);
                Console.WriteLine($"TEST42");
            }
            catch (SH.Game.GameOver)
            {
                Console.WriteLine($"Game is over.");
                await Context.Channel.SendMessageAsync("Well played!").ConfigureAwait(false);
                await Task.Delay(7 * 1000).ConfigureAwait(false);
                await Clear(prefix).ConfigureAwait(false); // Требуется улучшение: не удаляются роли
                await Context.Channel.SendMessageAsync("Another one? ( ͡° ͜ʖ ͡°)").ConfigureAwait(false);
            }
            catch
            {
                Console.WriteLine($"Critical error. Exit game.");
                await Context.Channel.SendMessageAsync("Oops! Something went wrong!..").ConfigureAwait(false);
            }
            
        }

        [Command("break")]
        public async Task Break(string s = null)
        {
            if (s != null)
            {
                await ReplyAsync("Extra T H I C C").ConfigureAwait(false);
            }
            await ReplyAsync("S U C C").ConfigureAwait(false);
        }

    }
}
