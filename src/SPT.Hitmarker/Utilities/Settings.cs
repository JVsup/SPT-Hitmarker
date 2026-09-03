using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using UnityEngine;

namespace SPT.Hitmarker.Utilities;

internal enum HitmarkerStyle
{
    Cross,
    X,
    Image
}

internal static class Settings
{
    private const string CategoryHitmarker = "1. Hitmarker";
    private const string CategoryDamageNumbers = "2. Damage Numbers";
    private const string CategoryAudio = "3. Audio";
    private const string CategoryAdvanced = "4. Advanced";

    private static readonly List<ConfigEntryBase> ConfigEntries = new();

    public static ConfigEntry<bool> HitmarkerEnabled { get; private set; }
    public static ConfigEntry<HitmarkerStyle> HitmarkerStyleMode { get; private set; }
    public static ConfigEntry<string> HitmarkerImageFile { get; private set; }
    public static ConfigEntry<string> HitmarkerHeadshotImageFile { get; private set; }
    public static ConfigEntry<string> HitmarkerKillImageFile { get; private set; }
    public static ConfigEntry<Color> HitmarkerKillColor { get; private set; }
    public static ConfigEntry<Color> HitmarkerColor { get; private set; }
    public static ConfigEntry<Color> ArmorHitColor { get; private set; }
    public static ConfigEntry<Color> HeadshotColor { get; private set; }
    public static ConfigEntry<bool> HeadshotPulse { get; private set; }
    public static ConfigEntry<float> HitmarkerSizePx { get; private set; }
    public static ConfigEntry<float> HitmarkerFadeSeconds { get; private set; }
    public static ConfigEntry<float> HitmarkerOpacity { get; private set; }

    public static ConfigEntry<bool> NumbersEnabled { get; private set; }
    public static ConfigEntry<int> NumbersFontSize { get; private set; }
    public static ConfigEntry<float> NumbersLifetimeSeconds { get; private set; }
    public static ConfigEntry<float> NumbersRisePixels { get; private set; }
    public static ConfigEntry<string> NumbersTemplate { get; private set; }
    public static ConfigEntry<bool> NumbersBackdrop { get; private set; }
    public static ConfigEntry<bool> NumbersAtHitPosition { get; private set; }
    public static ConfigEntry<float> NumbersEdgeClampPadding { get; private set; }
    public static ConfigEntry<Color> NumbersColor { get; private set; }

    public static ConfigEntry<float> MasterVolume { get; private set; }
    public static ConfigEntry<bool> PlaySoundOnHit { get; private set; }
    public static ConfigEntry<bool> PlaySoundOnHeadshot { get; private set; }
    public static ConfigEntry<bool> PlaySoundOnKill { get; private set; }
    public static ConfigEntry<float> HitSoundVolume { get; private set; }
    public static ConfigEntry<float> HeadshotSoundVolume { get; private set; }
    public static ConfigEntry<float> KillSoundVolume { get; private set; }
    public static ConfigEntry<string> HitSoundFile { get; private set; }
    public static ConfigEntry<string> HeadshotSoundFile { get; private set; }
    public static ConfigEntry<string> KillSoundFile { get; private set; }

    public static ConfigEntry<bool> DebugLog { get; private set; }

    public static Font Font { get; private set; }

    public static void Initialize(ConfigFile config)
    {
        var range01 = new AcceptableValueRange<float>(0f, 1f);
        var rangeEdge = new AcceptableValueRange<float>(0f, 0.49f);
        int order = 1000;

        ConfigEntries.Add(HitmarkerEnabled = config.Bind(
            CategoryHitmarker,
            "Show Hitmarker",
            true,
            new ConfigDescription(
                "Toggle the hitmarker when you hit an enemy.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HitmarkerStyleMode = config.Bind(
            CategoryHitmarker,
            "Hitmarker Style",
            HitmarkerStyle.Image,
            new ConfigDescription(
                "Cross draws plus, X draws angled lines, Image uses a PNG from the UI folder.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HitmarkerImageFile = config.Bind(
            CategoryHitmarker,
            "Hitmarker: Normal Hit",
            "Hitmarker.png",
            new ConfigDescription(
                "PNG file name inside the UI folder.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HitmarkerHeadshotImageFile = config.Bind(
            CategoryHitmarker,
            "Hitmarker: Headshot Hit",
            "Hitmarker_Headshot.png",
            new ConfigDescription(
                "PNG file for headshot marker.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HitmarkerKillImageFile = config.Bind(
            CategoryHitmarker,
            "Hitmarker: Kill Hit",
            "Hitmarker_Kill.png",
            new ConfigDescription(
                "PNG file for kill marker.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HitmarkerKillColor = config.Bind(
            CategoryHitmarker,
            "Color: Kill",
            new Color(1f, 0f, 0f, 1f),
            new ConfigDescription(
                "Color for kill marker.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HitmarkerColor = config.Bind(
            CategoryHitmarker,
            "Color: Normal Hit",
            new Color(0f, 0.75f, 0f, 1f),
            new ConfigDescription(
                "Color for normal hits.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(ArmorHitColor = config.Bind(
            CategoryHitmarker,
            "Color: Armor Hit",
            new Color(0f, 0.75f, 1f, 1f),
            new ConfigDescription(
                "Color for armor hits.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HeadshotColor = config.Bind(
            CategoryHitmarker,
            "Color: Headshot",
            new Color(1f, 0.25f, 0.25f, 1f),
            new ConfigDescription(
                "Color for headshots.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HeadshotPulse = config.Bind(
            CategoryHitmarker,
            "Headshot Pulse Effect",
            true,
            new ConfigDescription(
                "If the hitmarker briefly scales on headshots.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HitmarkerSizePx = config.Bind(
            CategoryHitmarker,
            "Size",
            12f,
            new ConfigDescription(
                "The size of the hitmarker lines or image in pixels.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HitmarkerFadeSeconds = config.Bind(
            CategoryHitmarker,
            "Visible Time",
            0.5f,
            new ConfigDescription(
                "Duration before the marker fades out in seconds.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HitmarkerOpacity = config.Bind(
            CategoryHitmarker,
            "Opacity",
            1f,
            new ConfigDescription(
                "The opacity for the hitmarker.",
                range01,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(NumbersEnabled = config.Bind(
            CategoryDamageNumbers,
            "Show Damage Numbers",
            false,
            new ConfigDescription(
                "Toggle floating damage numbers next to hitmarker on hits.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(NumbersFontSize = config.Bind(
            CategoryDamageNumbers,
            "Font Size",
            18,
            new ConfigDescription(
                "Font size of the damage text.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(NumbersLifetimeSeconds = config.Bind(
            CategoryDamageNumbers,
            "Visible Time",
            1.25f,
            new ConfigDescription(
                "Lifetime of each damage number in seconds.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(NumbersRisePixels = config.Bind(
            CategoryDamageNumbers,
            "Rise Distance",
            30f,
            new ConfigDescription(
                "Pixels the text moves upward over its lifetime.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(NumbersTemplate = config.Bind(
            CategoryDamageNumbers,
            "Main Text Template",
            "{flesh} ({armor})",
            new ConfigDescription(
                "Tokens: {flesh} body damage, {armor} armor damage, {bp} body part.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(NumbersBackdrop = config.Bind(
            CategoryDamageNumbers,
            "Background Box",
            false,
            new ConfigDescription(
                "Add a semi-transparent box behind the damage numbers for readability.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(NumbersAtHitPosition = config.Bind(
            CategoryDamageNumbers,
            "Show At Hit Position",
            true,
            new ConfigDescription(
                "If true, numbers appear where it hits on the target. If off-screen they clamp to the nearest edge.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(NumbersEdgeClampPadding = config.Bind(
            CategoryDamageNumbers,
            "Edge Clamp Padding",
            0.04f,
            new ConfigDescription(
                "Padding when clamping off-screen hits.",
                rangeEdge,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(NumbersColor = config.Bind(
            CategoryDamageNumbers,
            "Base Text Color",
            new Color(1f, 0.85f, 0.25f, 1f),
            new ConfigDescription(
                "Tag color used for labels such as Ricochet.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(MasterVolume = config.Bind(
            CategoryAudio,
            "Master Volume",
            0.6f,
            new ConfigDescription(
                "Global volume for sounds.",
                range01,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(PlaySoundOnHit = config.Bind(
            CategoryAudio,
            "Play Sound On Hit",
            true,
            new ConfigDescription(
                "Play a sound on hit.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(PlaySoundOnHeadshot = config.Bind(
            CategoryAudio,
            "Play Sound On Headshot",
            true,
            new ConfigDescription(
                "Play a sound on headshot.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(PlaySoundOnKill = config.Bind(
            CategoryAudio,
            "Play Sound On Kill",
            true,
            new ConfigDescription(
                "Play a sound on kill.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HitSoundVolume = config.Bind(
            CategoryAudio,
            "Hit Sound Volume",
            0.6f,
            new ConfigDescription(
                "Volume for hit sound.",
                range01,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HeadshotSoundVolume = config.Bind(
            CategoryAudio,
            "Headshot Sound Volume",
            0.6f,
            new ConfigDescription(
                "Volume for headshot sound.",
                range01,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(KillSoundVolume = config.Bind(
            CategoryAudio,
            "Kill Sound Volume",
            0.6f,
            new ConfigDescription(
                "Volume for kill sound.",
                range01,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HitSoundFile = config.Bind(
            CategoryAudio,
            "Hit Sound File Name",
            "Hitmarker1.wav",
            new ConfigDescription(
                "WAV file in the Sounds folder.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(HeadshotSoundFile = config.Bind(
            CategoryAudio,
            "Headshot Sound File Name",
            "Hitmarker2.wav",
            new ConfigDescription(
                "WAV file in the Sounds folder.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(KillSoundFile = config.Bind(
            CategoryAudio,
            "Kill Sound File Name",
            "Kill.wav",
            new ConfigDescription(
                "WAV file in the Sounds folder.",
                null,
                new ConfigurationManagerAttributes { Order = order-- })));

        ConfigEntries.Add(DebugLog = config.Bind(
            CategoryAdvanced,
            "Debug Log Output",
            false,
            new ConfigDescription(
                "Log hit and kill events to the BepInEx console.",
                null,
                new ConfigurationManagerAttributes { Order = order, IsAdvanced = true })));

        Font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        HitSoundFile.Subscribe(_ => SoundBank.Reload());
        HeadshotSoundFile.Subscribe(_ => SoundBank.Reload());
        KillSoundFile.Subscribe(_ => SoundBank.Reload());
        MasterVolume.Subscribe(_ => SoundBank.Reload());
        HitSoundVolume.Subscribe(_ => SoundBank.Reload());
        HeadshotSoundVolume.Subscribe(_ => SoundBank.Reload());
        KillSoundVolume.Subscribe(_ => SoundBank.Reload());
    }
}

internal static class SettingExtensions
{
    public static void Subscribe<T>(this ConfigEntry<T> configEntry, Action<T> onChange)
    {
        configEntry.SettingChanged += (_, _) => onChange(configEntry.Value);
    }
}
