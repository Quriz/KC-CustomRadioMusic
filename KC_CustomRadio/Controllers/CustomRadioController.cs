using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using MelonLoader;
using UnityEngine;
using UnityEngine.Networking;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using MonoBehaviour = Photon.MonoBehaviour;

namespace Quriz.CustomRadio;

public class CustomRadioController : MonoBehaviour
{
    private static readonly MelonLogger.Instance Logger = Melon<Mod>.Logger;
    
    private const string AddSongsKey = "CustomRadio_addSongs";
    private const string IsPlayingKey = "CustomRadio_isPlaying";
    private const string CurrentSongKey = "CustomRadio_currentSong";
    private const string RemoveSongKey = "CustomRadio_removeSong";
    private const string UpdateTimestampKey = "CustomRadio_updateTimestamp";
    
    public event EventHandler<string> OnSongChanged;
        
    private Radio _radio;
    private RadioUI _ui;

    private AudioSource _audioSource;
    private Coroutine _playSongFileCoroutine;
        
    private readonly List<string> _playlist = [];
    private string _currentSong;

    private void Awake()
    {
        _radio = GetComponent<Radio>();
        _ui = RadioUI.CreateForRadio(this);
        
        _audioSource = GameServer.restaurantM.CurrentRestaurant.SoundRegion.GetComponent<AudioSource>();
        _audioSource.loop = false;
        _audioSource.playOnAwake = false;

        SongsDownloader.OnDownloadProgress += OnDownloadProgress;
        SongsDownloader.OnDownloadStatusChanged += OnDownloadStatusChanged;
    }

    private void OnDestroy()
    {
        SongsDownloader.OnDownloadProgress -= OnDownloadProgress;
        SongsDownloader.OnDownloadStatusChanged -= OnDownloadStatusChanged;
        Destroy(_ui.gameObject);
    }

    public void AddSongs(string url)
    {
        SetCustomProperties(new Hashtable
        {
            { AddSongsKey, url },
        });
    }

    public void Play(string fileName)
    {
        SetCustomProperties(new Hashtable
        {
            { CurrentSongKey, fileName },
            { UpdateTimestampKey, DateTime.Now.ToBinary() },
        });
    }

    public void ContinuePlaying()
    {
        SetCustomProperties(new Hashtable
        {
            { IsPlayingKey, true },
            { UpdateTimestampKey, DateTime.Now.ToBinary() },
        });
    }

    public void PausePlaying(string fileName)
    {
        if (fileName != _currentSong)
            return;
        
        SetCustomProperties(new Hashtable
        {
            { IsPlayingKey, false }
        });
    }

    public void NextSong()
    {
        var currentSong = (string)PhotonNetwork.room.CustomProperties[CurrentSongKey];
        var nextSongIndex = (_playlist.IndexOf(currentSong) + 1) % _playlist.Count;
        SetCustomProperties(new Hashtable 
        { 
            { CurrentSongKey, _playlist[nextSongIndex] },
            { UpdateTimestampKey, DateTime.Now.ToBinary() },
        });
    }

    public void RemoveSongFromPlaylist(string fileName)
    {
        SetCustomProperties(new Hashtable 
        { 
            { RemoveSongKey, fileName },
        });
    }
        
    /// <summary>
    /// When room properties have changed.
    /// Gets called from <see cref="Room.SetCustomProperties"/>
    /// </summary>
    private void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
    {
        var updateTimestampBinary = (long)(propertiesThatChanged[UpdateTimestampKey] ?? DateTime.Now.ToBinary());
        var updateTimestamp = DateTime.FromBinary(updateTimestampBinary);
        var timeDifference = (float)(DateTime.Now - updateTimestamp).TotalSeconds;
        
        // Add songs to playlist
        if (propertiesThatChanged.TryGetValue(AddSongsKey, out var urlValue))
        {
            SongsDownloader.Enqueue((string)urlValue);
        }
        
        // Play/Pause
        if (propertiesThatChanged.TryGetValue(IsPlayingKey, out var isPlayingValue))
        {
            var isPlaying = (bool)isPlayingValue;
            if (isPlaying)
            {
                // Continue playing
                _audioSource.time += timeDifference;
                _audioSource.Play();
                _radio.ToggleRadio(true);
            }
            else
            {
                _audioSource.Pause();
                _radio.ToggleRadio(false);
            }
        }
        
        // Play song
        if (propertiesThatChanged.TryGetValue(CurrentSongKey, out var currentSongValue))
        {
            var currentSongFileName = (string)currentSongValue;
            PlayFile(currentSongFileName, updateTimestamp);
            _radio.ToggleRadio(true);
            
            OnSongChanged?.Invoke(this, currentSongFileName);
        }
        
        // Remove song from playlist
        if (propertiesThatChanged.TryGetValue(RemoveSongKey, out var removeSongValue))
        {
            var songFileName = (string)removeSongValue;
            
            if (_currentSong == songFileName)
                PausePlaying(songFileName);
            
            _playlist.Remove(songFileName);
            _ui.UpdatePlaylist(_playlist);
        }
    }

    private void OnDownloadProgress(object sender, SongsDownloader.DownloadProgress args)
    {
        var isFirstSong = _playlist.Count == 0;
        var fileName = Path.GetFileName(args.filePath);

        // Add to playlist
        if (!_playlist.Contains(fileName))
            _playlist.Add(fileName);
        
        // Update UI
        _ui.UpdatePlaylist(_playlist);
        _ui.SetLoadingText($"Loading songs {args.idx}/{args.count}");
        
        // Automatically start playing the first song if the master client has downloaded it
        if (isFirstSong && PhotonNetwork.isMasterClient)
            Play(fileName);
    }

    private void OnDownloadStatusChanged(object sender, SongsDownloader.DownloadStatus args)
    {
        if (args.isDownloading)
            _ui.SetLoadingText("Loading songs...");
        
        _ui.SetDownloading(args.isDownloading);
    }

    private void PlayFile(string fileName, DateTime startTime)
    {
        if (_playSongFileCoroutine != null)
            StopCoroutine(_playSongFileCoroutine);

        _currentSong = fileName;
        var uri = new Uri(SongsDownloader.GetFullPath(fileName));
        _playSongFileCoroutine = StartCoroutine(DoPlayFile(uri, startTime));
    }
    
    private IEnumerator DoPlayFile(Uri uri, DateTime startTime)
    {
        while (!File.Exists(uri.LocalPath))
        {
            yield return new WaitForSeconds(1f);
        }
        
        using var request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.OGGVORBIS);
        yield return request.SendWebRequest();

        if (request.responseCode == 200)
        {
            var clip = DownloadHandlerAudioClip.GetContent(request);
            clip.name = Path.GetFileNameWithoutExtension(uri.LocalPath);

            _audioSource.Stop();
            var timeDifference = (float)(DateTime.Now - startTime).TotalSeconds;
            _audioSource.clip = clip;
            _audioSource.time = timeDifference;
            _audioSource.Play();

            Logger.Msg("Playing audio clip: " + uri);
        }
        else
        {
            Logger.Error($"Error loading audio clip '{uri}': {request.error}");
        }

        if (!PhotonNetwork.isMasterClient)
            yield break;

        // Wait until end of song reached
        var clipLength = _audioSource.clip.length - 0.1f;
        while (_audioSource.time < clipLength)
            yield return null;
        
        // Play next song
        NextSong();
    }

    public void OpenUI()
    {
        _ui.Open();
    }
        
    private void SetCustomProperties(Hashtable properties) => PhotonNetwork.room.SetCustomProperties(properties);
}