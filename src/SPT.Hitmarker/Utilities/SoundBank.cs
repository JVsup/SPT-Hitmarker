using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

namespace SPT.Hitmarker.Utilities;

internal static class SoundBank
{
    private static AudioSource _source;
    private static AudioClip _hitClip;
    private static AudioClip _headshotClip;
    private static AudioClip _killClip;
    private static string _baseSoundsDirectory;
    private static string _cachedHitName;
    private static string _cachedHeadshotName;
    private static string _cachedKillName;

    private static void Ensure()
    {
        if (_source != null)
        {
            return;
        }

        var audioObject = new GameObject("SPT.Hitmarker.Audio");
        _source = audioObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.spatialBlend = 0f;
        _source.volume = Settings.MasterVolume.Value;
        _baseSoundsDirectory = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
            "Sounds");
        LoadClips();
    }

    public static void Reload()
    {
        if (_source == null)
        {
            Ensure();
            return;
        }

        LoadClips();
    }

    private static void LoadClips()
    {
        string hitName = Settings.HitSoundFile.Value?.Trim() ?? string.Empty;
        string headshotName = Settings.HeadshotSoundFile.Value?.Trim() ?? string.Empty;
        string killName = Settings.KillSoundFile.Value?.Trim() ?? string.Empty;

        if (!string.Equals(hitName, _cachedHitName, StringComparison.OrdinalIgnoreCase))
        {
            _hitClip = TryLoadAudio(Path.Combine(_baseSoundsDirectory, hitName));
            _cachedHitName = hitName;
        }

        if (!string.Equals(headshotName, _cachedHeadshotName, StringComparison.OrdinalIgnoreCase))
        {
            _headshotClip = TryLoadAudio(Path.Combine(_baseSoundsDirectory, headshotName));
            _cachedHeadshotName = headshotName;
        }

        if (!string.Equals(killName, _cachedKillName, StringComparison.OrdinalIgnoreCase))
        {
            _killClip = TryLoadAudio(Path.Combine(_baseSoundsDirectory, killName));
            _cachedKillName = killName;
        }
    }

    public static void PlayHit()
    {
        Ensure();
        if (!Settings.PlaySoundOnHit.Value || _hitClip == null)
        {
            return;
        }

        _source.volume = Settings.MasterVolume.Value * Settings.HitSoundVolume.Value;
        _source.PlayOneShot(_hitClip);
    }

    public static void PlayHeadshot()
    {
        Ensure();
        if (!Settings.PlaySoundOnHeadshot.Value || _headshotClip == null)
        {
            return;
        }

        _source.volume = Settings.MasterVolume.Value * Settings.HeadshotSoundVolume.Value;
        _source.PlayOneShot(_headshotClip);
    }

    public static void PlayKill()
    {
        Ensure();
        if (!Settings.PlaySoundOnKill.Value || _killClip == null)
        {
            return;
        }

        _source.volume = Settings.MasterVolume.Value * Settings.KillSoundVolume.Value;
        _source.PlayOneShot(_killClip);
    }

    private static AudioClip TryLoadAudio(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return null;
            }

            return LoadAudio(path);
        }
        catch
        {
            return null;
        }
    }

    private static AudioClip LoadAudio(string path)
    {
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            return null;
        }

        string extension = Path.GetExtension(path)?.ToLowerInvariant();
        AudioType audioType = extension == ".ogg" ? AudioType.OGGVORBIS : AudioType.WAV;
        string uri = path.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? path
            : "file://" + path.Replace("\\", "/");

        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
        UnityWebRequestAsyncOperation operation = request.SendWebRequest();
        while (!operation.isDone)
        {
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            return null;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
        if (clip != null)
        {
            clip.name = Path.GetFileNameWithoutExtension(path);
        }

        return clip;
    }
}
