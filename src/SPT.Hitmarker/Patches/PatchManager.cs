using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SPT.Reflection.Patching;

namespace SPT.Hitmarker.Patches;

internal static class PatchManager
{
    public static void EnablePatches()
    {
        foreach (Type patchType in GetAllPatches())
        {
            ((ModulePatch)Activator.CreateInstance(patchType)).Enable();
        }
    }

    private static IEnumerable<Type> GetAllPatches()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type.BaseType == typeof(ModulePatch)
                           && type.GetCustomAttribute<DisablePatchAttribute>() == null);
    }
}

[AttributeUsage(AttributeTargets.Class)]
internal sealed class DisablePatchAttribute : Attribute
{
}
