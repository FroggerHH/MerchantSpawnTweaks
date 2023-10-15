namespace TravelingLocations.Patch;

[HarmonyPatch]
public static class LoadFromFileOnStart
{
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
    [HarmonyPostfix] [HarmonyWrapSafe]
    private static void ZoneSystemStart() { LoadPositionsFromFile(); }
}