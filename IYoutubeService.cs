using YoutubeExplode.Videos.Streams;
using YoutubeExplode.Videos;

namespace DiscordBotYoutube
{
    public interface IYoutubeService
    {
        Queue<Video> GetSongs();
        void EnqueueSong(Video video);
        Video DequeueSong();

        Task<Video> GetVideoAsync(string videoId);
        Task<StreamManifest> GetStreamManifestAsync(string videoId);
        Task<Stream> GetStreamAsync(AudioOnlyStreamInfo audioStreamInfo);

        Task PlayAsync(string videoId);

        void Skip();

        void Stop();

        string ShowPlaylist(long amountToShow = 5);
    }
}
