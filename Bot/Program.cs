using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace Bot
{
    class Program //this is code to run and initialize the BOT
    {
        static void Main(string[] args) => new Program().RunBotAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client; //structure veribles
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunBotAsync() //runs bot on the server and it's methods in different threads
        {
            _client = new DiscordSocketClient(); //construct client object (i think client its a discord app on the computer
            _commands = new CommandService(); //construct object for commands

            _services = new ServiceCollection() //consctruct object which associate client and commands
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string token = "NzAwMDU0NDU2Nzg3NjY1MDA3.XpdaZA.vypcxqau51IRrRsiv7y6aSz4WdQ"; //unic token of our bot(don't show anyone! copy here from VK)

            _client.Log += _client_Log;

            await RegisterCommandsAsync();//get info about commands

            await _client.LoginAsync(TokenType.Bot, token);//connect our bot

            await _client.StartAsync();//starts all methods

            await Task.Delay(-1); //makes BOT work forever
        }

        private Task _client_Log(LogMessage arg)//puts info about work procces of the programm in the console
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()//working with chat and makes commands work
        {
            _client.MessageReceived += HandleCommandAsync;//if somebody has typed smth in the chat
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);//sync modules where we are discribing commands
        }

        private async Task HandleCommandAsync(SocketMessage arg)//helps BOT to procces a commands when smth was typed in the chat
        {
            var message = arg as SocketUserMessage; //gets message from the chat
            var context = new SocketCommandContext(_client, message); //gets context (not really sure what this is)
            if (message.Author.IsBot) return; //doesn's allow BOT answer his own messages

            int argPos = 0;//maybe position of the char in message
            if (message.HasStringPrefix("/", ref argPos))//looking for command-char in our case this is a '/' like minecraft
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);//runs a command in this context
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);//if smth goes wrong writes a discribtion of the error in the console
            }
        }
    }
}
