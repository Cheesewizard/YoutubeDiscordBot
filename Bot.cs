using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;

namespace DiscordBotYoutube
{
    public class Bot : IBot
    {
        private readonly DiscordClient _client;
        private readonly SlashCommandsExtension _slashCommands;

        public Bot(DiscordClient client, SlashCommandsExtension slashCommands)
        {
            _client = client;
            _slashCommands = slashCommands;
        }

        public async Task StartAsync()
        {
            // Enable VoiceNext
            _client.UseVoiceNext();

            _client.Ready += Client_Ready;
            _client.MessageCreated += Client_MessageCreated;

            await _client.ConnectAsync();

            RegisterSlashCommands();
        }

        private void RegisterSlashCommands()
        {
            _slashCommands.RegisterCommands<YoutubeBot>();

            // Set up your command handlers
            _slashCommands.SlashCommandExecuted += async (s, e) =>
            {
                await e.Context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            };

            _slashCommands.SlashCommandErrored += async (s, e) =>
            {
                await e.Context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                    new DiscordInteractionResponseBuilder()
                        .WithContent("An error occurred while executing the command.")
                        .AsEphemeral(true));
            };
        }

        private async Task Client_Ready(DiscordClient sender, ReadyEventArgs e)
        {
            Console.WriteLine("Bot is connected and ready!");
        }

        private async Task Client_MessageCreated(DiscordClient sender, MessageCreateEventArgs e)
        {
            
        }
    }
}