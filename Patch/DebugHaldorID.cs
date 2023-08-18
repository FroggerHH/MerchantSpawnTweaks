using Extensions;
using HarmonyLib;
using static MerchantSpawnTweaks.Plugin;

namespace MerchantSpawnTweaks.Patch;

[HarmonyPatch]
public static class DebugHaldorID
{
    [HarmonyPatch(typeof(Trader), nameof(Trader.Start))]
    [HarmonyWrapSafe]
    [HarmonyPostfix]
    private static void ZNViewAwake(Trader __instance)
    {
        Debug(
            $"I'm {__instance.GetPrefabName()}, my ID is {__instance.GetComponent<ZNetView>().GetZDO().Get_ID()}");
    }
}
//---------------Old way--------------------
//I'm Haldor, my ID is 29023 //exit
//I'm Haldor, my ID is 29027//kill
//I'm Haldor, my ID is 29027//exit
//I'm Haldor, my ID is 29027//rejoin world
//I'm Haldor, my ID is 93821
//I'm Haldor, my ID is 158615

//---------------New way----------------------
//I'm Haldor, my ID is 1397653704
//I'm Haldor, my ID is 1397653704
//I'm Haldor, my ID is 1397653704 //Changed position: I'm Haldor, my ID is 1570853064