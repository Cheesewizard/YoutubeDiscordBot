using NAudio.Wave;

namespace DiscordBotYoutube
{
    public class AudioPlayer : IAudioPlayer
    {
        private CancellationTokenSource? tokenSource;
        public async Task PlayAsync(Stream audioStream, CancellationTokenSource cancellationToken)
        {
            tokenSource = cancellationToken;

            using var audioStreamReader = new WaveFormatConversionStream(new WaveFormat(48000, 16, 2), new Mp3FileReader(audioStream));
            using var player = new WaveOutEvent();
            player.Init(audioStreamReader);
            player.Play();

            player.PlaybackStopped += (sender, args) =>
            {
                player.Dispose();
                audioStreamReader.Dispose();
            };

            while (player.PlaybackState == PlaybackState.Playing)
            {
                cancellationToken.Token.ThrowIfCancellationRequested();
                await Task.Delay(1000); // Wait for a second before checking again
            }
        }

        public void Skip()
        {
            tokenSource?.Cancel();
        }

        public void Stop()
        {
            tokenSource?.Cancel();
        }
    }

}
