using System.Collections.Generic;
using Extensions;
using HarmonyLib;
using UnityEngine;
using static MerchantSpawnTweaks.Plugin;
using static ZoneSystem;

namespace MerchantSpawnTweaks.Patch;

[HarmonyPatch]
public static class UpdateConfigOnZoneSysStart
{
    [HarmonyPatch(typeof(Game), nameof(Game.Start))]
    [HarmonyPostfix, HarmonyWrapSafe]
    private static void GameStart() => _self.UpdateConfiguration();
}