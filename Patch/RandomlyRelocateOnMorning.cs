using JetBrains.Annotations;

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
        if (RelocateInterval > 0 && day - LastRelocateDay >= RelocateInterval)
        {
            Debug("Morning, relocating locations...");
            Relocator.RandomlyRelocateLocations();
        } else
        {
            Debug("Morning.");
        }
    }
}