using HarmonyLib;
using static TravelingLocations.Plugin;

namespace TravelingLocations.Patch;

[HarmonyPatch]
public static class UpdateConfigOnZoneSysStart
{
    [HarmonyPatch(typeof(Game), nameof(Game.Start))]
    [HarmonyPostfix]
    [HarmonyWrapSafe]
    private static void GameStart()
    {
        _self.UpdateConfiguration();
    }
}