using YoutubeExplode.Videos;
using YoutubeExplode;
using System.Text;
using YoutubeExplode.Videos.Streams;

namespace DiscordBotYoutube
{
    internal class YoutubeService : IYoutubeService
    {
        private readonly IAudioPlayer _audioPlayer;
        private readonly Queue<Video> songQueue = new();
        private readonly YoutubeClient _youtube = new();

        public YoutubeService(IAudioPlayer audioPlayer)
        {
            this._audioPlayer = audioPlayer;
        }

        public Queue<Video> GetSongs()
        {
            return songQueue;
        }

        public void EnqueueSong(Video video)
        {
            songQueue.Enqueue(video);
        }

        public Video DequeueSong()
        {
            return songQueue.Dequeue();
        }

        public async Task<Video> GetVideoAsync(string videoId)
        {
            return await _youtube.Videos.GetAsync(videoId);
        }

        public async Task<StreamManifest> GetStreamManifestAsync(string videoId)
        {
            return await _youtube.Videos.Streams.GetManifestAsync(videoId);
        }

        public async Task<Stream> GetStreamAsync(AudioOnlyStreamInfo audioStreamInfo)
        {
            return await _youtube.Videos.Streams.GetAsync(audioStreamInfo);
        }

        public async Task PlayAsync(string videoId)
        {
            try
            {
                var video = await _youtube.Videos.GetAsync(videoId);
                var streamManifest = await _youtube.Videos.Streams.GetManifestAsync(videoId);

                var audioStreamInfo = streamManifest.GetAudioOnlyStreams().OrderByDescending(s => s.Bitrate).FirstOrDefault();

                if (audioStreamInfo != null)
                {
                    var stream = await _youtube.Videos.Streams.GetAsync(audioStreamInfo);
                    await _audioPlayer.PlayAsync(stream, new CancellationTokenSource());
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Cancelled playing current song");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error playing song, {exception.Message}");
            }
        }

        public void Skip()
        {
            _audioPlayer.Skip();
        }

        public void Stop()
        {
            _audioPlayer.Stop();
        }

        public string ShowPlaylist(long amountToShow = 5)
        {
            var songs = new StringBuilder();
            var queueCount = songQueue.Count;
            for (var i = 0; i < amountToShow; i++)
            {
                if (i > queueCount)
                {
                    break;
                }

                songs.AppendLine($"{i + 1}: {songQueue.ToArray()[i].Title}");
            }

            return songs.ToString();
        }
    }
}
