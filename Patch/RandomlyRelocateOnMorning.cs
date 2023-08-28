using HarmonyLib;
using JetBrains.Annotations;
using static TravelingLocations.Plugin;

namespace TravelingLocations.Patch;

[HarmonyPatch(typeof(EnvMan))]
public static class RandomlyRelocateOnMorning
{
    [HarmonyPatch(nameof(EnvMan.OnMorning))]
    [HarmonyWrapSafe]
    private static void Prefix([NotNull] EnvMan __instance)
    {
        if (!ZNet.instance.IsServer()) return;
        var day = __instance.GetCurrentDay();
        if (relocateInterval > 0 && day - lastRelocateDay >= relocateInterval)
        {
            Debug("Morning, relocating locations...");
            Relocator.RandomlyRelocateLocations();
        }
        else
        {
            Debug("Morning.");
        }
    }
}