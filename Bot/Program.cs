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
        static void Main() => new Program().RunBotAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client; //structure variables
        private CommandService _commands;
        private IServiceProvider _services;

        public async Task RunBotAsync() //runs bot on the server and it's methods in different threads
        {
            var configProvider = new ConfigProvider();

            _client = new DiscordSocketClient(); //construct client object (i think client its a discord app on the computer
            _commands = new CommandService(); //construct object for commands

            _services = new ServiceCollection() //construct object which associate client and commands
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string token = (string)configProvider.GetSetting(Options.Token); //unic token of our bot(don't show anyone!)

            _client.Log += _client_Log;

            await RegisterCommandsAsync();//get info about commands

            await _client.LoginAsync(TokenType.Bot, token);//connect our bot

            await _client.StartAsync();//starts all methods

            await Task.Delay(-1); //makes BOT work forever
        }

        private Task _client_Log(LogMessage arg)//puts info about work process of the program in the console
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()//working with chat and makes commands work
        {
            _client.MessageReceived += HandleCommandAsync;//if somebody has typed something in the chat
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);//sync modules where we are describing commands
        }

        private async Task HandleCommandAsync(SocketMessage arg)//helps BOT to process a commands when spmething was typed in the chat
        {
            var message = arg as SocketUserMessage; //gets message from the chat
            var context = new SocketCommandContext(_client, message); //gets context (not really sure what this is)
            //if (message.Author.IsBot) return; //doesn't allow BOT answer his own messages

            int argPos = 0;//maybe position of the char in message
            if (message.HasStringPrefix("/", ref argPos))//looking for command-char in our case this is a '/' like minecraft
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);//runs a command in this context
                if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);//if something goes wrong writes a discribtion of the error in the console
            }
        }
    }
}
