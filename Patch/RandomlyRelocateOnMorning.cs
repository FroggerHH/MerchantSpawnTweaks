using HarmonyLib;
using static MerchantSpawnTweaks.Plugin;

namespace MerchantSpawnTweaks.Patch;

[HarmonyPatch(typeof(EnvMan))]
public static class RandomlyRelocateOnMorning
{
    [HarmonyPatch(nameof(EnvMan.OnMorning))]
    private static void Prefix(EnvMan __instance)
    {
        var day = (int)(__instance.m_totalSeconds / __instance.m_dayLengthSec);
        if (relocateInterval > 0 && day - lastRelocateDay >= relocateInterval)
        {
            Debug("Morning, relocating merchant...");
            Relocator.RandomlyRelocateMerchant();
        }
        else
        {
            Debug("Morning.");
        }
    }
}