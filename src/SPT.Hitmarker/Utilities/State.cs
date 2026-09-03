using Comfort.Common;
using EFT;

namespace SPT.Hitmarker.Utilities;

internal static class State
{
    public static GameWorld World => Singleton<GameWorld>.Instance;

    public static Player LocalPlayer => World?.MainPlayer;
}
