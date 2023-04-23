using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBotYoutube
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Discord Youtube Bot");

            var serviceProvider = ConfigureServices();
            var bot = serviceProvider.GetService<Bot>();

            if (bot != null)
            {
                await bot.StartAsync();
            }

            // Keep the application running until terminated
            await Task.Delay(-1);
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton(new DiscordConfiguration
            {
                Token = Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"),
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.AllUnprivileged | DiscordIntents.All
            });

            services.AddSingleton(provider =>
            {
                var config = provider.GetService<DiscordConfiguration>();
                return new DiscordClient(config);
            });

            services.AddSingleton(provider =>
            {
                var client = provider.GetService<DiscordClient>();
                return client.UseSlashCommands(new SlashCommandsConfiguration
                {
                    Services = provider
                });
            });

            services.AddSingleton<YoutubeBot>();
            services.AddSingleton<Bot>();
            services.AddSingleton<IYoutubeService, YoutubeService>();

            return services.BuildServiceProvider();
        }
    }
}