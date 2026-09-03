using BepInEx;
using BepInEx.Logging;
using SPT.Hitmarker.Patches;
using SPT.Hitmarker.Utilities;

namespace SPT.Hitmarker;

[BepInPlugin(PluginMetadata.Guid, PluginMetadata.Name, PluginMetadata.Version)]
[BepInDependency("com.SPT.custom", "4.1.3")]
public sealed class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource Log { get; private set; }

    private void Awake()
    {
        Log = Logger;
        Settings.Initialize(Config);
        PatchManager.EnablePatches();
    }
}

internal static class PluginMetadata
{
    public const string Guid = "com.jvsup.hitmarker";
    public const string Name = "Hitmarker";
    public const string Version = "4.1.0";
}
