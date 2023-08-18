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

        if (merchantCurrentPosition == Vector2.zero || GetHaldors().Count <= 0)
        {
            if (merchantPositions.Count > 0)
            {
                merchantCurrentPositionConfig.Value = merchantPositions.First();
            }
            else
            {
                //Good
                var posNoY = pos.ToV2().RoundCords();
                Debug($"Registering haldor, position is {pos}, writing {posNoY}");
                merchantPositions.Add(posNoY);
                SetMerchantPositionsConfig();
                merchantCurrentPositionConfig.Value = posNoY;
                var vector2I = instance.GetZone(pos);
                haldorFirstZoneConfig.Value = new Vector2(vector2I.x, vector2I.y);

                _self.Config.Reload();
                _self.UpdateConfiguration();
                //Good
            }


            Debug("Alowing haldor to spawn");

            return true;
        }

        Debug("Prevented haldor to spawn");
        return false;
    }
}