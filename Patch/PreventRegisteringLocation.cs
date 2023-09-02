using System.Collections.Generic;
using System.Linq;
using Extensions;
using Extensions.Valheim;
using HarmonyLib;
using UnityEngine;
using static TravelingLocations.Plugin;
using static ZoneSystem;
using static ZNetScene;

namespace TravelingLocations.Patch;

[HarmonyPatch]
public static class PreventRegisteringLocation
{
    [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.RegisterLocation))]
    [HarmonyPrefix]
    [HarmonyWrapSafe]
    private static bool ZoneSystem_RegisterLocation(ZoneSystem __instance, ZoneLocation location,
        Vector3 pos, bool generated)
    {
        var locationsNames = locationsConfig.GetAllLocationsNames();
        if (!locationsNames.Contains(location.m_prefabName)) return true;

        var posNoY = pos.ToV2().RoundCords().ToSimpleVector2();
        if (!locationsNames.Contains(location.m_prefabName))
            locationsConfig.locations.Add(new()
            {
                clearAreaAfterRelocating = true,
                name = location.m_prefabName,
                positions = new() { posNoY }
            });
        else locationsConfig.GetLocationConfig(location.m_prefabName).positions.TryAdd(posNoY);
        UpdatePositionsFile();
        _self.Config.Reload();
        _self.UpdateConfiguration();

        if (ZoneSystem.instance.GetGeneratedLocationsByName(location.m_prefabName).Length <= 0)
        {
            Debug("Allowing haldor to spawn");
            return true;
        }

        Debug("Prevented haldor to spawn");
        return false;
    }
}