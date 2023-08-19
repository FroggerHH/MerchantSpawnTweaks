using System.Linq;
using Extensions;
using UnityEngine;
using static HeightmapBuilder;
using static MerchantSpawnTweaks.Plugin;

namespace MerchantSpawnTweaks;

public static class Relocator
{
    public static void RPC_RelocateMerchant(long _)
    {
        MoveMerchantToCurrentPos();
    }

    public static Vector2 RandomlyRelocateMerchant()
    {
        var locationInstance = GetHaldors().First().Value;
        if (locationInstance.m_location != null && !locationInstance.m_placed)
        {
            DebugWarning("For first relocation haldor location needs to be explored by someone");
            return Vector3.zero;
        }

        Debug("Finding new place to relocate merchant...");

        var next = merchantPositions.Next(merchantCurrentPosition);
        merchantCurrentPositionConfig.Value = next;
        _self.Config.Reload();
        _self.UpdateConfiguration();

        ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.Everybody, "RelocateMerchant");
        return merchantCurrentPosition;
    }

    public static void MoveMerchantToCurrentPos()
    {
        var position = merchantCurrentPosition.ToV3();

        Debug($"MoveMerchantToCurrentPos, position is {position}");

        var data =
            new HMBuildData(position, 1, 1, false, WorldGenerator.instance);
        instance.Build(data);

        position.y = data.m_baseHeights[0];

        var objects = ZDOMan.instance.m_objectsByID.Select(x => x.Value).Where(x => savedIDs.Contains(x.Get_ID()))
            .ToList();
        Debug(string.Join(", ", objects.Select(x => ZNetScene.instance.GetPrefab(x.GetPrefab()).GetPrefabName())));
        if (objects.Count < 1)
        {
            DebugError("No objects found", false);
            return;
        }

        var haldorLoc = GetHaldorPrefab();
        if (haldorLoc.m_prefab)
            haldorLoc.m_prefab.transform.position =
                Vector3.zero; // 1931579592,1955808584,1496468950,1011581049,1242205269
        if (haldorLoc.m_location) haldorLoc.m_location.transform.position = Vector3.zero;
        savedIDs.Clear();
        foreach (var zdo in objects)
        {
            var zNetView = haldorLoc.m_netViews.Find(x => x.GetPrefabName().GetStableHashCode() == zdo.GetPrefab());
            var prefabPosition = zNetView ? zNetView.transform.position : Vector3.zero;
            var resultPos = position + prefabPosition;

            var newId = resultPos.RoundCords().GetHashCode() + zdo.GetPrefab();
            savedIDs.Add(newId);
            zdo.SetPosition(resultPos);
        }

        lastRelocateDayConfig.Value = EnvMan.instance.GetCurrentDay();
        savedIDsConfig.Value = string.Join(",", savedIDs);
        _self.Config.Reload();
        _self.UpdateConfiguration();

        var locationInstance = ZoneSystem.instance.m_locationInstances.Select(x => x.Value)
            .ToList()
            .Find(x => x.m_location.m_prefabName == HALDOR_LOCATION_NAME);
        locationInstance.m_position = position;
        ZoneSystem.instance.m_locationInstances[new Vector2i(haldorFirstZone)] = locationInstance;


        // Minimap.instance.m_locationPins.Select(x => x.Value).ToList()
        //     .ForEach(x => Minimap.instance.DestroyPinMarker(x));
        // Minimap.instance.m_locationPins.Clear();
        Minimap.instance.m_updateLocationsTimer = -1;
        Minimap.instance.UpdateLocationPins(0);

        _self.Config.Reload();
        _self.UpdateConfiguration();

        Debug($"Merchant relocated to position {position}");
    }
}