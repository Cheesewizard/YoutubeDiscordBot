namespace DiscordBotYoutube
{
    public interface IAudioPlayer
    {
        Task PlayAsync(Stream audioStream, CancellationTokenSource cancellationToken);
        void Skip();
        void Stop();
    }
}
