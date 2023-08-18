﻿using System.Collections.Generic;
using System.Linq;
using Extensions;
using UnityEngine;
using static HeightmapBuilder;
using static MerchantSpawnTweaks.Plugin;
using Enumerable = System.Linq.Enumerable;

namespace MerchantSpawnTweaks;

public static class Relocator
{
    public static void RPC_RelocateMerchant(long _)
    {
        MoveMerchantToCurrentPos();
    }

    public static Vector2 RandomlyRelocateMerchant()
    {
        var locationInstance = Enumerable.First(GetHaldors()).Value;
        if (locationInstance.m_location != null && !locationInstance.m_placed)
        {
            DebugWarning("For first relocation haldor location needs to be explored by someone");
            return Vector3.zero;
        }

        Debug("Finding new place to relocate merchant...");

        var next = merchantPositions.Next(merchantCurrentPosition);
        Debug($"Next pos is {next}");
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
        if (haldorLoc.m_prefab) haldorLoc.m_prefab.transform.position = Vector3.zero; // 1931579592,1955808584,1496468950,1011581049,1242205269
        if (haldorLoc.m_location) haldorLoc.m_location.transform.position = Vector3.zero;
        savedIDs.Clear();
        foreach (var zdo in objects)
        {
            var zNetView = haldorLoc.m_netViews.Find(x => x.GetPrefabName().GetStableHashCode() == zdo.GetPrefab());
            var prefabPosition = zNetView ? zNetView.transform.position : Vector3.zero;
            var resultPos = position + prefabPosition;

            var newId = resultPos.RoundCords().GetHashCode() + zdo.GetPrefab();
            Debug($"New id is {newId}");
            savedIDs.Add(newId);
            zdo.SetPosition(resultPos);
        }

        var value = string.Join(",", savedIDs);
        savedIDsConfig.Value = value;
        _self.Config.Reload();
        _self.UpdateConfiguration();

        var locationInstance = Enumerable
            .ToList(Enumerable.Select(ZoneSystem.instance.m_locationInstances, x => x.Value))
            .Find(x => x.m_location.m_prefabName == HALDOR_LOCATION_NAME);
        locationInstance.m_position = position;
        ZoneSystem.instance.m_locationInstances[new Vector2i(haldorFirstZone)] = locationInstance;

        Minimap.instance.UpdateLocationPins(Time.deltaTime);

        _self.Config.Reload();
        _self.UpdateConfiguration();

        Debug($"Merchant relocated to position {position}");
    }
}