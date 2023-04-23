using YoutubeExplode;
using YoutubeExplode.Videos;
using NAudio.Wave;
using DSharpPlus.SlashCommands;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.VoiceNext;
using System.Threading;

namespace DiscordBot
{
    internal class Youtube : ApplicationCommandModule
    {
        private static Queue<Video> songQueue = new();
        private static YoutubeClient youtube = new();
        private static CancellationTokenSource tokenSource = new();

        public Youtube()
        {
            youtube = new YoutubeClient();
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
            var video = await youtube.Videos.GetAsync(url);
            if (video != null)
            {
                songQueue.Enqueue(video);
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
            if (songQueue.Count > 0)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

                while (songQueue.Count > 0)
                {
                    var video = songQueue.Dequeue();
                    await ctx.FollowUpAsync(new DiscordFollowupMessageBuilder()
                        .WithContent($"Playing: {video.Id}"));

                    tokenSource = new CancellationTokenSource();
                    await Play(video.Id, tokenSource.Token);
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
            if (songQueue.Count > 0)
            {
                Skip();
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("Skipping the current song..."));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                    .WithContent("No songs in the queue to skip."));
            }
        }

        private async Task Play(string videoId, CancellationToken cancellationToken)
        {
            try
            {
                var video = await youtube.Videos.GetAsync(videoId);
                var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoId);

                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).FirstOrDefault();

                if (audioStreamInfo != null)
                {
                    var stream = await youtube.Videos.Streams.GetAsync(audioStreamInfo);
                    var tempFilePath = Path.GetTempFileName();
                    await using (var fileStream = File.OpenWrite(tempFilePath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }

                    using var mediaFoundationReader = new MediaFoundationReader(tempFilePath);
                    using var pcmStream = WaveFormatConversionStream.CreatePcmStream(mediaFoundationReader);
                    using var blockAlignedStream = new BlockAlignReductionStream(pcmStream);

                    var player = new WaveOutEvent();
                    player.Init(blockAlignedStream);
                    player.Play();

                    player.PlaybackStopped += (sender, args) =>
                    {
                        player.Dispose();
                        blockAlignedStream.Dispose();
                        File.Delete(tempFilePath);
                    };

                    while (player.PlaybackState == PlaybackState.Playing)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        await Task.Delay(1000); // Wait for a second before checking again
                    }
                }
            }
            catch (OperationCanceledException exception)
            {
                Console.WriteLine("Cancelled playing current song");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error playing song, {exception.Message}");
            }
        }

        private async void Queue(string videoUrl)
        {
            var video = await youtube.Videos.GetAsync(videoUrl);
            songQueue.Enqueue(video);
        }

        private void Skip()
        {
            tokenSource.Cancel();
            tokenSource = new CancellationTokenSource();
        }

    }
}
