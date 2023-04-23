using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus;
using DSharpPlus.VoiceNext;

namespace DiscordBotYoutube
{
    public class YoutubeBot : ApplicationCommandModule
    {
        private readonly IYoutubeService _youtubeService;

        public YoutubeBot(IYoutubeService youtubeService)
        {
            _youtubeService = youtubeService;
        }

        [SlashCommand("join", "Join the user's voice channel or the specified channel")]
        public async Task JoinChannel(InteractionContext context, [Option("channel", "The voice channel to join")] DiscordChannel channel = null)
        {
            // Get the audio channel
            var guildMember = await context.Guild.GetMemberAsync(context.User.Id);
            channel ??= guildMember?.VoiceState?.Channel;

            if (channel == null)
            {
                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("You need to be in a voice channel or provide a voice channel as an argument."));
                return;
            }

            // Connect to the channel
            var nextVoice = context.Client.GetVoiceNext();
            var connection = nextVoice.GetConnection(context.Guild);

            if (connection != null)
            {
                connection.Disconnect();
            }

            connection = await nextVoice.ConnectAsync(channel);
            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent($"Joined {channel.Name}"));
        }

        [SlashCommand("queue", "Queue a song using the provided YouTube URL")]
        public async Task QueueSongAsync(InteractionContext ctx, [Option("url", "The YouTube URL of the song to queue")] string url)
        {
            var video = await _youtubeService.GetVideoAsync(url);
            if (video != null)
            {
                _youtubeService.EnqueueSong(video);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent($"Song queued: {url}"));
                return;
            }

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent($"Song not found: {url}"));
        }


        [SlashCommand("play", "Play the queued songs")]
        public async Task PlaySongAsync(InteractionContext ctx)
        {
            if (_youtubeService.GetSongs().Count > 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                while (_youtubeService.GetSongs().Count > 0)
                {
                    var video = _youtubeService.DequeueSong();

                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                        .WithContent($"Playing: {video.Title}"));

                    await _youtubeService.PlayAsync(video.Id);
                }

                await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                    .WithContent($"No more songs queued"));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent($"No songs in the queue"));
            }
        }

        [SlashCommand("skip", "Skip the current song")]
        public async Task SkipSongAsync(InteractionContext ctx)
        {
            if (_youtubeService.GetSongs().Count > 0)
            {
                _youtubeService.Skip();

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("Skipping the current song..."));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("No songs in the queue to skip."));
            }
        }

        [SlashCommand("stop", "Stop the current song")]
        public async Task StopSongAsync(InteractionContext ctx)
        {
            _youtubeService.Stop();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                .WithContent("Stopping the current song..."));

        }

        [SlashCommand("playlist", "Show the current playlist")]
        public async Task ShowPlaylistAsync(InteractionContext ctx, [Option("amount", "The amount of up coming songs")] long amountToShow = 5)
        {
            if (_youtubeService.GetSongs().Count > 0)
            {
                var currentPlaylist = _youtubeService.ShowPlaylist(amountToShow);
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent(currentPlaylist));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("No songs in the queue."));
            }
        }
    }
}
