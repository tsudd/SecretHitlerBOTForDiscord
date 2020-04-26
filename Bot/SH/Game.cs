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
    public class Game
    {
        public class GameOver : Exception { public GameOver() { } }

        public async Task TheEnd(string message)
        {
            string info = "Roles:\n";
            for (int j = 1; j <= PlayerCount; j++)
            {
                info += $"{PlayerName(j)} - {Users[j].GetRole()}\n";
            }
            await Mailing(info).ConfigureAwait(false);
            await Mailing(message).ConfigureAwait(false);
            throw new GameOver();
        }

        private readonly bool DEBUG;

        enum Roles
        {
            Liberal,
            Fascist,
            Hitler
        }

        struct Player
        {
            public Roles Role;
            public readonly ulong UserId;
            public readonly ulong ChatId;
            public bool Alive;

            public Player(ulong userid, ulong chatid)
            {
                Role = Roles.Liberal;
                Alive = true;
                UserId = userid;
                ChatId = chatid;
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

        private string PlayerName(int num)
        {
            return ($"#{num} {Guild.GetUser(Users[num].UserId).Nickname}");
        }

        private Emoji CardEmoji(Card law, int i)
        {
            switch (law)
            {
                case Card.Red:
                    return VoteCardsR[i];
                case Card.Blue:
                    return VoteCardsB[i];
                default:
                    return new Emoji("⚠️");
            }
        }

        private SocketGuild Guild;
        private Player[] Users;
        private int PlayerCount;
        private int PlayersAlive
        {
            get
            {
                var ans = new List<int>();
                for(int i = 1; i <= PlayerCount; i++)
                {
                    if (Users[i].Alive)
                    {
                        ans.Add(i);
                    }
                }
                return ans.Count();
            }
        }
        private int CurPres = 0, CurChanc = 0, PrevPres = 0, PrevChanc = 0;
        private int FascLaws = 0, LibLaws = 0;
        private int SkipsCount = 0;
        private readonly Emoji[] NumbersEmoji;
        private readonly Emoji[] VoteNeinJa;
        private readonly Emoji[] VoteCardsR, VoteCardsB;
        private Deck LawsPile;

        private async Task DoFascFeature()
        {
            async Task CheckTeam()
            {
                await Mailing("President must check 🔍 the team of a player.").ConfigureAwait(false);
                int check = await GetPresChoice(i => 
                    (Users[i].Alive && i != CurPres))
                    .ConfigureAwait(false);
                await Mailing($"President checks the team of {PlayerName(check)}.").ConfigureAwait(false);
                await Guild.GetTextChannel(Users[CurPres].ChatId)
                        .SendMessageAsync($"Player {PlayerName(check)} is in {(Users[check].Role == Roles.Liberal ? "Liberal" + VoteCardsB[0].Name : "Fascist" + VoteCardsR[0].Name)} team!")
                        .ConfigureAwait(false);
            }
            async Task Execute()
            {
                await Mailing("President must kill ☠️ a player.").ConfigureAwait(false);
                int kill = await GetPresChoice(i => 
                    (Users[i].Alive && i != CurPres && !(Users[CurPres].Role == Roles.Fascist && Users[i].Role == Roles.Hitler)))
                    .ConfigureAwait(false);
                await Mailing($"President kills {PlayerName(kill)}.").ConfigureAwait(false);
                Users[kill].Alive = false;
                if (Users[kill].Role == Roles.Hitler)
                {
                    await TheEnd("🕊️ Liberals win! 🕊️\n Happy Christmas to all! Hitler's dead.").ConfigureAwait(false);
                }
            }

            switch (FascLaws)
            {
                case 1:
                    {
                        if (PlayerCount > 8)
                        {
                            await CheckTeam().ConfigureAwait(false);
                        }
                        return;
                    }

                case 2:
                    {
                        if (PlayerCount > 6)
                        {
                            await CheckTeam().ConfigureAwait(false);
                        }
                        return;
                    }
                case 3:
                    {
                        if (PlayerCount < 7)
                        {
                            await Mailing("President will see 👀 next three cards in random order.").ConfigureAwait(false);
                            var next = LawsPile.ShowThreeCards();
                            if (DEBUG)
                            {
                                Console.Write("Next three law cards: ");
                                for (int i = 0; i < 3; i++)
                                {
                                    Console.Write($"{(char)next[i]}");
                                }
                                Console.WriteLine();
                            }
                            DamnMethods.Shuffle(next);
                            var str = new char[3];
                            for(int i = 0; i < 3; i++)
                            {
                                str[i] = (char)next[i];
                            }
                            if (DEBUG)
                            {
                                Console.WriteLine($"{PlayerName(CurPres)} sees {new string(str)}.");
                            }
                            string NextLawsMessage = "Next laws:";
                            for (int i = 0; i < 3; i++)
                            {
                                NextLawsMessage += ' ';
                                switch (next[i])
                                {
                                    case Card.Red:
                                        NextLawsMessage += VoteCardsR[0].Name;
                                        break;
                                    case Card.Blue:
                                        NextLawsMessage += VoteCardsB[0].Name;
                                        break;
                                }
                            }
                            await Guild.GetTextChannel(Users[CurPres].ChatId)
                                    .SendMessageAsync(NextLawsMessage)
                                    .ConfigureAwait(false);
                        }
                        if (PlayerCount > 6)
                        {
                            await Mailing("President must choose next president 👔 out of turn.").ConfigureAwait(false);
                            PrevPres = 0;
                            PrevChanc = 0;
                            int check = await GetPresChoice(i => 
                                (Users[i].Alive /* && i != CurPres */))
                                .ConfigureAwait(false);
                            await DoPres(check).ConfigureAwait(false);
                        }
                        return;
                    }
                case 4:
                    {
                        await Execute().ConfigureAwait(false);
                        return;
                    }
                case 5:
                    {
                        await Execute().ConfigureAwait(false);
                        return;
                    }
            }
        }

        delegate bool CanChoose(int i);
        private async Task<int> GetPresChoice(CanChoose function)
        {
            var can = new List<int>();
            for (int i = 1; i <= PlayerCount; i++)
            {
                if (function(i))
                {
                    can.Add(i);
                }
            }
            while (true)
            {
                try
                {
                    int choice = 0;
                    Console.WriteLine($"President #{CurPres} must make a choice:");
                    string PossiblePlayers = "Choose the player!";
                    for (int i = 0; i < can.Count; i++)
                    {
                        PossiblePlayers += '\n';
                        PossiblePlayers += PlayerName(can[i]);
                    }
                    var ChoiceMessage = await Guild.GetTextChannel(Users[CurPres].ChatId)
                                                .SendMessageAsync($"{PossiblePlayers}")
                                                .ConfigureAwait(false);

                    for (int i = 0; i < can.Count; i++)
                    {
                        await ChoiceMessage.AddReactionAsync(NumbersEmoji[can[i]])
                                .ConfigureAwait(false);
                    }
                    do
                    {
                        for (int i = 0; i < can.Count; i++)
                        {
                            var arr = (await ChoiceMessage.GetReactionUsersAsync(NumbersEmoji[can[i]], 2)
                                               .FlattenAsync().ConfigureAwait(false)).ToArray();
                            if (arr.Length > 1)
                            {
                                choice = can[i];
                                Console.WriteLine($"President has chosen #{choice}.");
                                break;
                            }
                        }
                    }
                    while (choice == 0);
                    return choice;
                }
                catch
                {
                    Console.WriteLine($"Choice error. Meh.");
                }
            }
        }

        private async Task<Card[]> Discard(int num, Card[] cardSet)
        {

            Console.Write($"{PlayerName(num)} must discard a law: ");
            if (DEBUG)
            {
                for (int i = 0; i < cardSet.Length; i++)
                {
                    Console.Write($"{(char)cardSet[i]}");
                }
            }
            Console.WriteLine();

            var ChoiceMessage = await Guild.GetTextChannel(Users[num].ChatId)
                                        .SendMessageAsync("Choose the law to discard!")
                                        .ConfigureAwait(false);

            for (int i = 0; i < cardSet.Length; i++)
            {
                await ChoiceMessage.AddReactionAsync(CardEmoji(cardSet[i], i))
                        .ConfigureAwait(false);
            }
            int choice = -1;
            do
            {
                for (int i = 0; i < cardSet.Length; i++)
                {
                    var arr = (await ChoiceMessage.GetReactionUsersAsync(CardEmoji(cardSet[i], i), 2)
                                       .FlattenAsync().ConfigureAwait(false)).ToArray();
                    if (arr.Length > 1)
                    {
                        choice = i;
                        break;
                    }
                }
            }
            while (choice == -1);
            if (DEBUG)
            {
                Console.WriteLine($"{PlayerName(num)} has discarded {(char)cardSet[choice]} law.");
            }
            LawsPile.PushOneCard(cardSet[choice]);
            var rtrn = new Card[cardSet.Length - 1];
            for (int i = 0; i < cardSet.Length; i++)
            {
                if (i < choice)
                {
                    rtrn[i] = cardSet[i];
                }
                if (i > choice)
                {
                    rtrn[i-1] = cardSet[i];
                }
            }
            return rtrn;
        }

        private async Task AcceptLaw(Card law)
        {
            Console.WriteLine($"A new one law is {(char)law}.");
            switch (law)
            {
                case Card.Red:
                    FascLaws++;
                    await Mailing($"Fascist {VoteCardsR[0].Name} law has passed").ConfigureAwait(false);
                    break;
                case Card.Blue:
                    LibLaws++;
                    await Mailing($"Liberal {VoteCardsB[0].Name} law has passed").ConfigureAwait(false);
                    break;
            }
            string field = "Current field:\n";
            for (int i = 1; i <= FascLaws; i++)
            {
                field += VoteCardsR[0].Name;
                field += ' ';
            }
            for (int i = FascLaws + 1; i <= 6; i++)
            {
                switch (i)
                {
                    case 1:
                        if (PlayerCount > 8) field += "🔍";
                        else field += "▫️";
                        break;
                    case 2:
                        if (PlayerCount > 6) field += "🔍";
                        else field += "▫️";
                        break;
                    case 3:
                        if (PlayerCount > 6) field += "👔";
                        else field += "📜";
                        break;
                    case 4:
                        field += "🖊️";
                        break;
                    case 5:
                        field += "🖋️";
                        break;
                    case 6:
                        field += "🚩";
                        break;
                }
                field += ' ';
            }
            field += '\n';
            for (int i = 1; i <= LibLaws; i++)
            {
                field += VoteCardsB[0].Name;
                field += ' ';
            }
            for (int i = LibLaws + 1; i <= 5; i++)
            {
                switch (i)
                {
                    case 5:
                        field += "🏳️‍🌈";
                        break;
                    default:
                        field += "▫️";
                        break;
                }
                field += ' ';
            }
            await Mailing($"{field}").ConfigureAwait(false);
            if (law == Card.Red && FascLaws == 3)
            {
                await Mailing("Be careful! Hitler should not become сhancellor❗").ConfigureAwait(false);
            }
            if (law == Card.Red && FascLaws == 5)
            {
                await Mailing("Veto became available!").ConfigureAwait(false);
            }
            if (FascLaws == 6)
            {
                await TheEnd("🦅 Fascists win! 🦅\n Glory to Arstotzka!").ConfigureAwait(false);
            }
            if (LibLaws == 5)
            {
                await TheEnd("🕊️ Liberals win! 🕊️\n Liberté, Égalité, Fraternité!").ConfigureAwait(false);
            }
        }
        
        private async Task SkipPres()
        {
            Console.WriteLine($"President {PlayerName(CurPres)} has been skipped.");
            SkipsCount++;
            await Mailing($"Skips: {SkipsCount}/3").ConfigureAwait(false);
            if (SkipsCount == 3)
            {
                SkipsCount = 0;
                PrevPres = 0;
                PrevChanc = 0;
                await Mailing($"The people demand to approve the law!").ConfigureAwait(false);
                await AcceptLaw(LawsPile.DropOneCard()).ConfigureAwait(false);
            }
        }

        private async Task<bool> AskNeinJa(int num)
        {
            var ChoiceMessage = await Guild.GetTextChannel(Users[num].ChatId)
                                        .SendMessageAsync("Vote Nein or Ja!")
                                        .ConfigureAwait(false);
            for (int i = 0; i < VoteNeinJa.Length; i++)
            {
                await ChoiceMessage.AddReactionAsync(VoteNeinJa[i]).ConfigureAwait(false);
            }
            int choice = -1;
            do
            {
                for (int i = 0; i < VoteNeinJa.Length; i++)
                {
                    var arr = (await ChoiceMessage.GetReactionUsersAsync(VoteNeinJa[i], 2)
                                       .FlattenAsync().ConfigureAwait(false)).ToArray();
                    if (arr.Length > 1)
                    {
                        choice = i;
                        break;
                    }
                }
            }
            while (choice == -1);
            Console.WriteLine($"{PlayerName(num)} just voted {(DEBUG ? (choice == 1 ? "Ja" : "Nein") : "")}.");
            return (choice == 1);
        }

        private async Task DoPres(int num)
        {
            if (!Users[num].Alive)
            {
                return;
            }
            Console.WriteLine($"Now {PlayerName(num)} is president.");
            await Mailing($"New president is {PlayerName(num)}").ConfigureAwait(false);
            CurPres = num;
            CurChanc = await GetPresChoice(i =>
                (Users[i].Alive && i != CurPres && (i != PrevPres || PlayersAlive <= 5) && (i != PrevChanc || PlayersAlive <= 4)))
                .ConfigureAwait(false);
            await Mailing($"{PlayerName(CurPres)} has chosen {PlayerName(CurChanc)} to be a chancellor.").ConfigureAwait(false);

            // ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ
            int VotesAgainst = 0;
            var VotePres = new List< Task<bool> >();
            for (int i = 1; i <= PlayerCount; i++)
            {
                if (Users[i].Alive)
                {
                    VotePres.Add(AskNeinJa(i));
                }
            }
            //Task.WaitAll(VotePres.ToArray());
            string votes = "Votes:\n";
            for (int i = 1, p = 0; i <= PlayerCount; i++)
            {
                if (Users[i].Alive)
                {
                    if (await VotePres[p] == false)
                    {
                        VotesAgainst++;
                        votes += $"{PlayerName(i)} - Nein!\n";
                    }
                    else
                    {
                        votes += $"{PlayerName(i)} - Ja!\n";
                    }
                    p++;
                }
            }
            await Mailing($"{votes}").ConfigureAwait(false);
            // ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ ПЕРЕДЕЛЫВАЙ

            if (VotesAgainst * 2 >= PlayersAlive)
            {
                Console.WriteLine($"Pair {PlayerName(CurPres)} & {PlayerName(CurChanc)} has been skipped.");
                await Mailing("👨‍⚖️ Отклонено! 👨‍⚖️").ConfigureAwait(false);
                await SkipPres().ConfigureAwait(false);
                return;
            }
            await Mailing("👨‍⚖️ При-ня-то! 👨‍⚖️").ConfigureAwait(false);
            if (FascLaws >= 3)
            {
                if (Users[CurChanc].Role == Roles.Hitler)
                {
                    await TheEnd("🐍 Fascists win! 🐍\n 卐 Heil Hitler! (∩ ͡° ͜ʖ ͡°)⊃━ 卐").ConfigureAwait(false);
                } 
                else
                {
                    await Mailing($"{PlayerName(CurChanc)} is not Hitler❗").ConfigureAwait(false);
                }
            }
            await Guild.GetUser(Users[CurPres].UserId).ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
            var tmp = await Discard(CurPres, LawsPile.DropThreeCards()).ConfigureAwait(false);
            await Guild.GetUser(Users[CurChanc].UserId).ModifyAsync(x => x.Mute = true).ConfigureAwait(false);
            tmp = await Discard(CurChanc, tmp).ConfigureAwait(false);
            await Guild.GetUser(Users[CurPres].UserId).ModifyAsync(x => x.Mute = false).ConfigureAwait(false);
            await Guild.GetUser(Users[CurChanc].UserId).ModifyAsync(x => x.Mute = false).ConfigureAwait(false);
            if (FascLaws == 5)
            {
                if (DEBUG)
                {
                    Console.WriteLine($"The last card is {(char)tmp[0]}.");
                }
                Console.WriteLine($"It's veto time!");
                await Guild.GetTextChannel(Users[CurChanc].ChatId)
                        .SendMessageAsync("Do you want to ACCEPT a law?")
                        .ConfigureAwait(false);
                if (await AskNeinJa(CurChanc).ConfigureAwait(false) == false)
                {
                    Console.WriteLine($"Chancellor is against the law!");
                    await Guild.GetTextChannel(Users[CurPres].ChatId)
                            .SendMessageAsync("Do you want to ACCEPT a law?")
                            .ConfigureAwait(false);
                    if (await AskNeinJa(CurPres).ConfigureAwait(false) == false)
                    {
                        Console.WriteLine($"President is against the law!");
                        await Mailing("Veto!").ConfigureAwait(false);
                        LawsPile.PushOneCard(tmp[0]);
                        await SkipPres().ConfigureAwait(false);
                        return;
                    }
                }
            }
            await AcceptLaw(tmp[0]).ConfigureAwait(false);
            SkipsCount = 0;
            PrevPres = CurPres;
            PrevChanc = CurChanc;
            if (tmp[0] == Card.Red) 
            {
                await DoFascFeature().ConfigureAwait(false);
            }
        }

        private async Task GiveRoles()
        {
            Console.WriteLine("Start giving roles.");

            int RandomLiberal()
            {
                var rnd = new Random();
                int i;
                do
                {
                    i = rnd.Next(1, PlayerCount + 1);
                }
                while (Users[i].Role != Roles.Liberal);
                return i;
            }

            Users[RandomLiberal()].Role = Roles.Hitler;
            Users[RandomLiberal()].Role = Roles.Fascist;
            if (PlayerCount > 6)
            {
                Users[RandomLiberal()].Role = Roles.Fascist;
                if (PlayerCount > 8)
                {
                    Users[RandomLiberal()].Role = Roles.Fascist;
                }
            }

            string PlayersList = "Players: \n";
            for (int i = 1; i <= PlayerCount; i++)
            {
                PlayersList += ($"{PlayerName(i)}\n");
            }
            Console.WriteLine(PlayersList);
            await Mailing(PlayersList).ConfigureAwait(false);
            Console.WriteLine("Stop mailing...");
            for (int i = 1; i <= PlayerCount; i++)
            {
                await Guild.GetTextChannel(Users[i].ChatId)
                        .SendMessageAsync($"Good luck, {PlayerName(i)}!")
                        .ConfigureAwait(false);
                await Guild.GetTextChannel(Users[i].ChatId)
                        .SendMessageAsync(String.Format("Your secret role is {0} {1} {0}", 
                        (Users[i].Role == Roles.Liberal ? VoteCardsB[0].Name : VoteCardsR[0].Name), Users[i].GetRole()))
                        .ConfigureAwait(false);
                if (Users[i].Role == Roles.Fascist || (PlayerCount <= 6 && Users[i].Role == Roles.Hitler))
                {
                    string info = "";
                    info += "Fascists: \n";
                    for (int j = 1; j <= PlayerCount; j++)
                    {
                        if (Users[j].Role == Roles.Fascist)
                        {
                            info += $"{PlayerName(j)}\n";
                        }
                    }
                    info += "Hitler - ";
                    for (int j = 1; j <= PlayerCount; j++)
                    {
                        if (Users[j].Role == Roles.Hitler)
                        {
                            info += $"{PlayerName(j)}.";
                        }
                    }
                    await Guild.GetTextChannel(Users[i].ChatId)
                            .SendMessageAsync(info)
                            .ConfigureAwait(false);
                }
                if (DEBUG)
                {
                    Console.WriteLine($"Player {PlayerName(i)} is {Users[i].GetRole()}");
                }
            }
        }

        public async Task Play(IUser[] users, SocketTextChannel[] channels)
        {
            PlayerCount = users.Length - 1;
            if (PlayerCount < 5)
            {
                Console.WriteLine("The game was canceled! Not enough players!");
                await channels[0]
                    .SendMessageAsync("Not enough players. Minimum is 5.")
                    .ConfigureAwait(false);
                return;
            }
            if (PlayerCount == 9)
            {
                Console.WriteLine("The game was canceled! 9 players! Bad!");
                await channels[0]
                    .SendMessageAsync("9 players is very bad.")
                    .ConfigureAwait(false);
                return;
            }
            Console.WriteLine($"Game guild is {Guild.Name}. Amount of players - {PlayerCount}.");
            Users = new Player[users.Length];
            for (int i = 0; i <= PlayerCount; i++)
            {
                Users[i] = new Player(users[i].Id, channels[i].Id);
                Console.WriteLine($"Player {PlayerName(i)} added to the game.");
            }

            await GiveRoles().ConfigureAwait(false);

            LawsPile = new Deck();
            if (PlayerCount == 6)
            {
                LawsPile = new Deck(10, 6);
                await AcceptLaw(Card.Red).ConfigureAwait(false);
            }
            if (DEBUG)
            {
                Console.WriteLine($"Deck: {LawsPile.DeckRB}.");
            }

            int PresQueue = 1;
            Console.WriteLine("Game loop has started");
            while (true)
            {
                await DoPres(PresQueue).ConfigureAwait(false);
                PresQueue++;
                if (PresQueue > PlayerCount)
                {
                    PresQueue = 1;
                }
            }
        }

        private async Task Mailing(string message)
        {
            /*
            await Guild.GetTextChannel(Users[0].ChatId)
                    .SendMessageAsync(message)
                    .ConfigureAwait(false);
            */
            var a = new Task[PlayerCount];
            for (int i = 1; i <= PlayerCount; i++)
            {
                a[i - 1] = Guild.GetTextChannel(Users[i].ChatId).SendMessageAsync(message);
            }
            for (int i = 1; i <= PlayerCount; i++)
            {
                await a[i - 1].ConfigureAwait(false);
            }
            //Task.WaitAll(a);
        }

        public Game(SocketGuild guild, bool logs = false)
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

            VoteNeinJa = new Emoji[2];
            VoteNeinJa[0] = new Emoji("❌");
            VoteNeinJa[1] = new Emoji("✅");

            VoteCardsR = new Emoji[3];
            VoteCardsB = new Emoji[3];
            VoteCardsR[0] = new Emoji("🟥");
            VoteCardsR[1] = new Emoji("🔴");
            VoteCardsR[2] = new Emoji("❤️");
            VoteCardsB[0] = new Emoji("🟦");
            VoteCardsB[1] = new Emoji("🔵");
            VoteCardsB[2] = new Emoji("💙");

            Guild = guild;

            DEBUG = logs;
        }

    }
}
