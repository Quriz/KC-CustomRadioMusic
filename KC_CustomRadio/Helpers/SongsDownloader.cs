using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MelonLoader;
using YoutubeDLSharp;
using YoutubeDLSharp.Options;

namespace Quriz.CustomRadio;

public static class SongsDownloader
{
    private static readonly MelonLogger.Instance Logger = Melon<Mod>.Logger;
    
    public record DownloadProgress(int idx, int count, string filePath);
    public record DownloadStatus(bool isDownloading);

    public static event EventHandler<DownloadProgress> OnDownloadProgress;
    public static event EventHandler<DownloadStatus> OnDownloadStatusChanged;
        
    private static readonly string RootFolder = Path.Combine(Directory.GetCurrentDirectory(), "CustomRadio");
    private static readonly string PlaylistFolder = Path.Combine(RootFolder, "playlist");
        
    private static readonly string FFmpegPath = Path.Combine(RootFolder, YoutubeDLSharp.Utils.FfmpegBinaryName);
    private static readonly string YoutubeDLPath = Path.Combine(RootFolder, YoutubeDLSharp.Utils.YtDlpBinaryName);
        
    private static bool IsFFmpegInstalled => File.Exists(FFmpegPath);
    private static bool IsYtDlpInstalled => File.Exists(YoutubeDLPath);
        
    private static readonly Regex SongIndexRegex = new Regex(@"^\[download\] Downloading item (\d+) of (\d+)");
    private static readonly Regex SongOutputRegex = new Regex(@"^outfile: ""([^""]+)""$");

    private static readonly YoutubeDL YoutubeDL = new YoutubeDL
    {
        FFmpegPath = FFmpegPath,
        YoutubeDLPath = YoutubeDLPath,
        OutputFolder = PlaylistFolder,
        OutputFileTemplate = "%(title)s.%(ext)s"
    };
        
    private static readonly Dictionary<string, string[]> DownloadedUrls = new();
    
    private static readonly ConcurrentQueue<string> DownloadQueue = new();
    private static readonly SemaphoreSlim QueueSemaphore = new(0);

    public static async void Init()
    {
        try
        {
            Directory.CreateDirectory(RootFolder);
            Directory.CreateDirectory(PlaylistFolder);

            if (!IsFFmpegInstalled)
            {
                await YoutubeDLSharp.Utils.DownloadFFmpeg(RootFolder);
                Logger.Msg("FFmpeg was downloaded successfully!");
            }

            if (!IsYtDlpInstalled)
            {
                await YoutubeDLSharp.Utils.DownloadYtDlp(RootFolder);
                Logger.Msg("yt-dlp was downloaded successfully!");
            }

        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
        
        ProcessDownloadsAsync();
    }

    public static void Enqueue(string url)
    {
        DownloadQueue.Enqueue(url);
        QueueSemaphore.Release();
    }
    
    private static async void ProcessDownloadsAsync()
    {
        var cancellationToken = Mod.CancellationTokenSource.Token;
        while (!cancellationToken.IsCancellationRequested)
        {
            // Wait for new download task
            if (DownloadQueue.Count == 0)
            {
                OnDownloadStatusChanged?.Invoke(null, new DownloadStatus(false));
                await QueueSemaphore.WaitAsync(cancellationToken);
            }

            // Process download task
            if (DownloadQueue.TryDequeue(out var url))
            {
                OnDownloadStatusChanged?.Invoke(null, new DownloadStatus(true));
                await DownloadSongs(url);
            }
        }
    }

    private static async Task DownloadSongs(string url)
    {
        if (DownloadedUrls.TryGetValue(url, out var songs))
        {
            Logger.Msg($"Songs already downloaded: {url}");
            
            for (var i = 0; i < songs.Length; i++)
                OnDownloadProgress?.Invoke(null, new DownloadProgress(i + 1, songs.Length, songs[i]));
            return;
        }

        var index = 1;
        var count = 1;
        var outputProgress = new Progress<string>(line => {
            var match = SongIndexRegex.Match(line);
            if (match.Success)
            {
                index = int.Parse(match.Groups[1].Value);
                count = int.Parse(match.Groups[2].Value);
            }
                
            match = SongOutputRegex.Match(line);
            if (match.Success)
            {
                var filePath = match.Groups[1].Value;
                OnDownloadProgress?.Invoke(null, new DownloadProgress(index, count, filePath));
                Logger.Msg($"Downloaded \"{Path.GetFileNameWithoutExtension(filePath)}\"");
            }
        });
        
        try
        {
            Logger.Msg($"Downloading \"{url}\"...");
            
            // Can't use mp3 because the engine version the game uses can't load it due to patent issues
            var res = await YoutubeDL.RunAudioPlaylistDownload(url, format: AudioConversionFormat.Vorbis, output: outputProgress, ct: Mod.CancellationTokenSource.Token);
            var files = res.Data.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            DownloadedUrls.Add(url, files);
        }
        catch (Exception e)
        {
            Logger.Error(e);
        }
    }

    public static string GetFullPath(string fileName) => Path.Combine(PlaylistFolder, fileName);

    public static void Cleanup()
    {
        foreach (var file in Directory.GetFiles(PlaylistFolder))
        {
            File.Delete(file);
        }
        QueueSemaphore.Dispose();
    }
}