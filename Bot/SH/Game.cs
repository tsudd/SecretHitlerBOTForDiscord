using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
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
        public readonly ulong UserId;
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


        private void DoFascFeature(int num)
        {

        }

        private int GetUserChoice(int num)
        {

            return 0;
        }

        private void DoPres(int num)
        {
            CurPres = num;

        }

        private void GiveRoles(SocketTextChannel[] channels)
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
                string role = Users[i].GetRole();
                Guild.GetTextChannel(Users[i].ChatId).SendMessageAsync($"Good luck, {Guild.GetUser(Users[i].UserId).Nickname}!");
                Guild.GetTextChannel(Users[i].ChatId).SendMessageAsync($"Your secret role is {role}");
                Console.WriteLine($"Player #{i} {Guild.GetUser(Users[i].UserId).Nickname} is {role}");
            }
        }

        public void Play(SocketTextChannel[] channels)
        {
            GiveRoles(channels);
            int PresQueue = 1;
            Console.WriteLine("Game loop has started");
            while(false) // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            {
                DoPres(PresQueue);
                PresQueue++;
            }
        }

        public Game(SocketGuild guild, IUser[] users)
        {
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
