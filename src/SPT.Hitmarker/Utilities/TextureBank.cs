using System.IO;
using System.Reflection;
using UnityEngine;

namespace SPT.Hitmarker.Utilities;

internal static class TextureBank
{
    private static string _baseUiDirectory;
    private static Texture2D _hitmarker;
    private static Texture2D _headshotHitmarker;
    private static Texture2D _killHitmarker;
    private static string _cachedHitName;
    private static string _cachedHeadshotName;
    private static string _cachedKillName;

    private static void EnsureBaseDirectory()
    {
        if (_baseUiDirectory != null)
        {
            return;
        }

        string pluginDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        _baseUiDirectory = Path.Combine(pluginDirectory, "UI");
    }

    private static Texture2D LoadPng(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return null;
            }

            byte[] bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2, TextureFormat.ARGB32, false)
            {
                filterMode = FilterMode.Bilinear
            };
            return texture.LoadImage(bytes) ? texture : null;
        }
        catch
        {
            return null;
        }
    }

    public static Texture2D Hitmarker()
    {
        EnsureBaseDirectory();
        string name = Settings.HitmarkerImageFile.Value?.Trim() ?? "Hitmarker.png";
        if (name != _cachedHitName)
        {
            _hitmarker = LoadPng(Path.Combine(_baseUiDirectory, name));
            _cachedHitName = name;
        }

        if (!_hitmarker)
        {
            _hitmarker = LoadPng(Path.Combine(_baseUiDirectory, "Hitmarker.png"));
        }

        return _hitmarker;
    }

    public static Texture2D HitmarkerHeadshot()
    {
        EnsureBaseDirectory();
        string name = Settings.HitmarkerHeadshotImageFile.Value?.Trim() ?? "Hitmarker_Headshot.png";
        if (name != _cachedHeadshotName)
        {
            _headshotHitmarker = LoadPng(Path.Combine(_baseUiDirectory, name));
            _cachedHeadshotName = name;
        }

        return _headshotHitmarker ?? Hitmarker();
    }

    public static Texture2D HitmarkerKill()
    {
        EnsureBaseDirectory();
        string name = Settings.HitmarkerKillImageFile.Value?.Trim() ?? "Hitmarker_Kill.png";
        if (name != _cachedKillName)
        {
            _killHitmarker = LoadPng(Path.Combine(_baseUiDirectory, name));
            _cachedKillName = name;
        }

        return _killHitmarker ?? Hitmarker();
    }
}
