using System.Collections.Generic;
using Extensions;
using HarmonyLib;
using UnityEngine;
using static MerchantSpawnTweaks.Plugin;
using static ZoneSystem;

namespace MerchantSpawnTweaks.Patch;

[HarmonyPatch]
public static class PreventRegisteringLocation
{
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.RegisterLocation))]
    [HarmonyPrefix, HarmonyWrapSafe]
    private static bool ZoneSystem_RegisterLocation(ZoneSystem __instance, ZoneLocation location,
        Vector3 pos, bool generated)
    {
        if (!locationsToMove.Contains(location.m_prefabName)) return true;

        var posNoY = pos.ToV2().RoundCords().ToSimpleVector2();
        if (!locationsPositions.ContainsKey(location.m_prefabName))
            locationsPositions.Add(location.m_prefabName, new() { posNoY });
        else locationsPositions[location.m_prefabName].TryAdd(posNoY);
        UpdatePositionsFile();
        _self.Config.Reload();
        _self.UpdateConfiguration();

        if (instance.GetGeneratedLocationsByName(location.m_prefabName).Length <= 0)
        {
            Debug("Allowing haldor to spawn");
            return true;
        }

        Debug("Prevented haldor to spawn");
        return false;
    }
}