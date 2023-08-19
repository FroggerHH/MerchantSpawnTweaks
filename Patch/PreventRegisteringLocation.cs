using System.Linq;
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
    [HarmonyPrefix]
    private static bool ZoneSystem_RegisterLocation(ZoneSystem __instance, ZoneLocation location,
        Vector3 pos, bool generated)
    {
        if (location.m_prefabName != HALDOR_LOCATION_NAME) return true;

        location.m_iconAlways = true;

        var posNoY = pos.ToV2().RoundCords();
        if (merchantCurrentPosition == Vector2.zero || GetHaldors().Count <= 0)
        {
            if (merchantPositions.Count > 0)
            {
                merchantCurrentPositionConfig.Value = merchantPositions.First();
            }
            else
            {
                merchantPositions.TryAdd(posNoY);
                SetMerchantPositionsConfig();
                merchantCurrentPositionConfig.Value = posNoY;
                var vector2I = instance.GetZone(pos);
                haldorFirstZoneConfig.Value = new Vector2(vector2I.x, vector2I.y);

                _self.Config.Reload();
                _self.UpdateConfiguration();
            }


            Debug("Alowing haldor to spawn");

            return true;
        }

        Debug("Prevented haldor to spawn");
        merchantPositions.TryAdd(posNoY);
        SetMerchantPositionsConfig();
        
        return false;
    }
}