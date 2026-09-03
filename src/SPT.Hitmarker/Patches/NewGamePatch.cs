using System.Reflection;
using Comfort.Common;
using EFT;
using SPT.Hitmarker.Features;
using SPT.Reflection.Patching;

namespace SPT.Hitmarker.Patches;

internal sealed class NewGamePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(GameWorld).GetMethod(
            nameof(GameWorld.OnGameStarted),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    }

    [PatchPrefix]
    private static void PatchPrefix()
    {
        GameWorld gameWorld = Singleton<GameWorld>.Instance;
        if (gameWorld == null)
        {
            return;
        }

        gameWorld.gameObject.AddComponent<HitmarkerController>();
    }
}
