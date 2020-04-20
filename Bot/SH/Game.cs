using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

namespace Bot.SH
{
    enum Roles
    {
        Liberal,
        Fascist,
        Hitler
    }

    struct Player
    {
        public Roles Role;
        public ulong UserId;
        public ulong ChatId;
        public bool Alive;
        public Player(ulong userid)
        {
            Role = Roles.Liberal;
            Alive = true;
            UserId = userid;
            ChatId = 0;
        }

        public string GetRole()
        {
            switch (Role)
            {
                case Roles.Liberal: return "Liberal";
                case Roles.Fascist: return "Fascist";
                case Roles.Hitler: return "Hitler";
                default: return "N/D";
            }
        }
    }

    public class Game
    {
        private SocketGuild Guild;
        private Player[] Users;
        private int PlayerCount;
        private int CurPres = 0, CurChanc = 0, PrevPres = 0, PrevChanc = 0;
        private int FascLaws = 0, LibLaws = 0;
        private int SkipsCount = 0;
        private readonly Emoji[] NumbersEmoji;

        private async Task DoFascFeature(int num)
        {

        }

        private async Task<int> GetPresChoice()
        {
            Console.WriteLine($"President #{CurPres} must make a choice:");
            var ChoiceMessage = await Guild.GetTextChannel(Users[CurPres].ChatId)
                                        .SendMessageAsync("Choose the player!")
                                        .ConfigureAwait(false);
            for (int i = 1; i <= PlayerCount; i++)
            {
                await ChoiceMessage.AddReactionAsync(NumbersEmoji[i])
                        .ConfigureAwait(false);
            }
            int choice = 0;
            do
            {
                await Task.Delay(3 * 1000).ConfigureAwait(false);
                for (int i = 1; i <= PlayerCount; i++)
                {
                    var arr = (await ChoiceMessage.GetReactionUsersAsync(NumbersEmoji[i], 2)
                                       .FlattenAsync().ConfigureAwait(false)).ToArray();
                    if (arr.Length > 1)
                    {
                        choice = i;
                        break;
                    }
                }
            } 
            while (choice == 0);
            return choice;
        }

        private async Task Mailing(string message)
        {
            for (int i = 0; i <= PlayerCount; i++)
            {
                await Guild.GetTextChannel(Users[i].ChatId)
                        .SendMessageAsync(message)
                        .ConfigureAwait(false);
            }
        }

        private async Task DoPres(int num)
        {
            Console.WriteLine($"Now Player#{num} is president.");
            CurPres = num;
            CurChanc = 0;
            int canc = 0;
            do
            {
                canc = await GetPresChoice().ConfigureAwait(false);
                Console.WriteLine($"President has chosen {canc} to be a chancellor.");
                if (canc == CurPres || canc == PrevPres || canc == PrevChanc || !Users[canc].Alive)
                {
                    canc = 0;
                    await Guild.GetTextChannel(Users[num].ChatId)
                            .SendMessageAsync("Whong choice!")
                            .ConfigureAwait(false);
                }
            }
            while (canc == 0);
            CurChanc = canc;
            // ГОВНО ПЕРЕДЕЛЫВАЙ ГОВНО ПЕРЕДЕЛЫВАЙ ГОВНО ПЕРЕДЕЛЫВАЙ ГОВНО ПЕРЕДЕЛЫВАЙ ГОВНО ПЕРЕДЕЛЫВАЙ
            var VoteNein = await Guild.GetTextChannel(Users[0].ChatId)
                                    .SendMessageAsync(String.Format($"President - {0}){1}, Chancellor - {2}){3}",
                                        num, Guild.GetUser(Users[num].UserId).Nickname, 
                                        canc, Guild.GetUser(Users[canc].UserId).Nickname))
                                    .ConfigureAwait(false);
            await VoteNein.AddReactionAsync(new Emoji("❌"))
                    .ConfigureAwait(false);
            await Task.Delay(10 * 1000).ConfigureAwait(false);
            var arr = (await VoteNein.GetReactionUsersAsync(new Emoji("❌"), 11)
                                       .FlattenAsync()
                                       .ConfigureAwait(false))
                                       .ToArray();
            // ГОВНО ПЕРЕДЕЛЫВАЙ ГОВНО ПЕРЕДЕЛЫВАЙ ГОВНО ПЕРЕДЕЛЫВАЙ ГОВНО ПЕРЕДЕЛЫВАЙ ГОВНО ПЕРЕДЕЛЫВАЙ
            if ((arr.Length - 1) * 2 >= PlayerCount)
            {
                SkipsCount++;
                if (SkipsCount == 3)
                {
                    // Принять случайный закон
                    SkipsCount = 0;
                }
                return;
            }
            await Guild.GetTextChannel(Users[0].ChatId)
                    .SendMessageAsync($"👨‍⚖️ При-ня-то! 👨‍⚖️")
                    .ConfigureAwait(false);

        }

        private async Task GiveRoles(SocketTextChannel[] channels)
        {
            DamnMethods.Shuffle(Users, 1, PlayerCount);
            Users[1].Role = Roles.Hitler;
            Users[2].Role = Roles.Fascist;
            if (PlayerCount > 6)
            {
                Users[3].Role = Roles.Fascist;
                if (PlayerCount > 8)
                {
                    Users[4].Role = Roles.Fascist;
                }
            }
            DamnMethods.Shuffle(Users, 1, PlayerCount);
            for (int i = 0; i <= PlayerCount; i++)
            {
                Users[i].ChatId = channels[i].Id;
            }
            for (int i = 1; i <= PlayerCount; i++)
            {
                await Guild.GetTextChannel(Users[0].ChatId)
                        .SendMessageAsync($"{Guild.GetUser(Users[i].UserId).Nickname}," +
                            $" your game room is {Guild.GetTextChannel(Users[i].ChatId).Name}.")
                        .ConfigureAwait(false);

                string role = Users[i].GetRole();
                await Guild.GetTextChannel(Users[i].ChatId)
                        .SendMessageAsync($"Good luck, {Guild.GetUser(Users[i].UserId).Nickname}!")
                        .ConfigureAwait(false);
                await Guild.GetTextChannel(Users[i].ChatId)
                        .SendMessageAsync($"Your secret role is {role}")
                        .ConfigureAwait(false);
                if (Users[i].Role == Roles.Fascist || (PlayerCount < 7 && Users[i].Role == Roles.Hitler))
                {
                    string info = "";
                    info += "Fascists: ";
                    for (int j = 1; j <= PlayerCount; j++)
                    {
                        if (Users[j].Role == Roles.Fascist)
                        {
                            info += $"#{j} {Guild.GetUser(Users[j].UserId).Nickname}, ";
                        }
                    }
                    //info.[info.LastIndexOf(',')] = ';';
                    info += "\n";
                    info += "Hitler: ";
                    for (int j = 1; j <= PlayerCount; j++)
                    {
                        if (Users[j].Role == Roles.Hitler)
                        {
                            info += $"#{j} {Guild.GetUser(Users[j].UserId).Nickname}. ";
                        }
                    }

                    await Guild.GetTextChannel(Users[i].ChatId)
                            .SendMessageAsync(info)
                            .ConfigureAwait(false);
                }
                Console.WriteLine($"Player #{i} {Guild.GetUser(Users[i].UserId).Nickname} is {role}");
            }
        }

        public async Task Play(SocketTextChannel[] channels)
        {
            await GiveRoles(channels).ConfigureAwait(false);
            int PresQueue = 1;
            Console.WriteLine("Game loop has started");
            while (true)
            {
                await DoPres(PresQueue).ConfigureAwait(false);
                PresQueue++;
                break;
            }
        }

        public Game(SocketGuild guild, IUser[] users)
        {
            NumbersEmoji = new Emoji[11];
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

            Guild = guild;
            PlayerCount = users.Length - 1;
            if (PlayerCount < 5)
            {
                Console.WriteLine("The game was canceled! Not enough players!");
                //throw "Nope.";
            }
            if (PlayerCount == 9)
            {
                Console.WriteLine("The game was canceled! 9 players! Bad!");
                throw new Exception("Nope.");
            }
            Console.WriteLine($"Game guild is {Guild.Name}. Amount of players {PlayerCount}");
            Users = new Player[users.Length];
            for (int i = 0; i <= PlayerCount; i++)
            {
                Users[i] = new Player(users[i].Id);
                Console.WriteLine($"Player {Guild.GetUser(Users[i].UserId).Nickname} added to the game.");
            }
        }
    }


}
