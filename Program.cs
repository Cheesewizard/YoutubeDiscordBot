using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using DSharpPlus.VoiceNext;

namespace DiscordBot
{
    internal class Program
    {
        private static Youtube youtube = new Youtube();
        private static DiscordClient client;
        private static SlashCommandsExtension slashCommands;

        private static string token => Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN");

        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Discord Youtube Bot");
            await DiscordClient();
            RegisterSlashCommands();

            // Keep the application running until terminated
            await Task.Delay(-1);
        }

        private static async Task DiscordClient()
        {
            var config = new DiscordConfiguration
            {
                Token = token,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.All
            };

            client = new DiscordClient(config);

            // Enable VoiceNext
            client.UseVoiceNext();

            client.Ready += Client_Ready;
            client.MessageCreated += Client_MessageCreated;

            await client.ConnectAsync();
        }


        private static async void RegisterSlashCommands()
        {
            var slashCommandsConfiguration = new SlashCommandsConfiguration
            {
                Services = new ServiceCollection().BuildServiceProvider()
            };

            slashCommands = client.UseSlashCommands(slashCommandsConfiguration);

            // Register your command modules
            slashCommands.RegisterCommands<Youtube>();

            // Set up your command handlers
            slashCommands.SlashCommandExecuted += async (s, e) =>
            {
                await e.Context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            };

            slashCommands.SlashCommandErrored += async (s, e) =>
            {
                await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("An error occurred while executing the command.")
                        .AsEphemeral(true));
            };
        }

        private static async Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            Console.WriteLine("Bot is connected and ready!");
        }

        private static async Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Message.Content.StartsWith("philip."))
            {
                // Process your text-based commands here if needed
            }
        }
    }
}















//        private static async Task DiscordClient()
//        {
//            var config = new DiscordSocketConfig
//            {
//                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
//            };

//            client = new DiscordSocketClient(config);

//            await client.LoginAsync(TokenType.Bot, "MTA5OTA2OTU0MjYxMDc3NjEyNA.GZ4AXO.FFwi0SR-4Qo2qsQYFzcHHl3vYGnVwIwDui1zKo");
//            await client.StartAsync();

//            client.Ready += Ready;
//            client.MessageReceived += HandleCommandAsync;
//        }

//        private static Task Ready()
//        {
//            Console.WriteLine("Bot is connected and ready!");
//            return Task.CompletedTask;
//        }

//        private static async Task DiscordCommands()
//        {
//            var commandsConfig = new CommandServiceConfig { Prefix = "philip." };

//            commands = new CommandService(commandsConfig);
//            await commands.AddModuleAsync<Youtube>(null);
//            await commands.AddModuleAsync<MyModule>(null);
//            commands.CommandExecuted += CommandExecutedAsync;
//        }

//        private static async Task HandleCommandAsync(SocketMessage messageParam)
//        {
//            var message = messageParam as SocketUserMessage;
//            if (message == null) return;

//            int argPos = 0;
//            if (!(message.HasStringPrefix("philip.", ref argPos) || message.HasMentionPrefix(client.CurrentUser, ref argPos)) || message.Author.IsBot) return;

//            var context = new SocketCommandContext(client, message);
//            var result = await commands.ExecuteAsync(context: context, argPos: argPos, services: null);

//            if (!result.IsSuccess)
//            {
//                Console.WriteLine(result.ErrorReason);
//            }
//        }

//        private static async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
//        {
//            // If the command was successful, we don't need to log anything
//            if (result.IsSuccess) return;

//            // Otherwise, log the error
//            await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
//        }

//    }
//}
