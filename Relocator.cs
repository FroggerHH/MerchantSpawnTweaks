using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using UnityEngine;
using static ZoneSystem;
using static HeightmapBuilder;
using static MerchantSpawnTweaks.Plugin;

namespace MerchantSpawnTweaks;

public static class Relocator
{
    private static bool calculationsFinished = false;
    private static IEnumerable<ZDO> objects = new List<ZDO>();

    
    
    
    
    
    
    
    
    
    
    
    public static void RandomlyRelocateLocations(bool sendMapPing = false)
    {
        _self.StopAllCoroutines();
        _self.StartCoroutine(RandomlyRelocateLocations_IEnumerator(sendMapPing));
    }

    private static IEnumerator RandomlyRelocateLocations_IEnumerator(bool sendMapPing = false)
    {
        calculationsFinished = true;
        foreach (var locationName in locationsToMove.ToList())
        {
            yield return new WaitUntil(() => calculationsFinished = true);
            var locationTuple = ZoneSystem.instance.GetGeneratedLocationsByName(locationName).FirstOrDefault();
            if (locationTuple.Item2.m_location != null && !locationTuple.Item2.m_placed)
            {
                var msg = $"For first relocation {locationName} location needs to be explored by someone.";
                DebugWarning(msg);
                if (Console.IsVisible()) Console.instance.AddString($"<color=yellow>{msg}</color>");
                continue;
            }

            Debug($"Finding new place to relocate {locationName}...");
            var newPosition = locationsPositions[locationName].Random();
            var position = newPosition.ToVector2().ToV3();
            var data = new HMBuildData(position, 1, 1, false, WorldGenerator.instance);
            HeightmapBuilder.instance.Build(data);
            position.y = data.m_baseHeights[0];
            
            Task.Run(() => { MoveIt(sendMapPing, locationTuple, locationName, position, newPosition); });
        }
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    

    private static void MoveIt(bool sendMapPing, (Vector2i, LocationInstance) locationTuple,
        string locationName, Vector3 position, SimpleVector2 newPosition)
    {
        calculationsFinished = false;
        DateTime now = DateTime.Now;
        GetLocationRelatedObjects(locationTuple.Item2);
        if (objects.Count() <= 0) DebugError("No objects found", false);
        else
        {
            var haldorLoc = ZoneSystem.instance.GetLocation(locationName);
            haldorLoc.m_prefab.transform.position = Vector3.zero;
            haldorLoc.m_prefab.transform.rotation = Quaternion.identity;
            foreach (var zdo in objects)
            {
                if (zdo == null) continue;
                var zNetView =
                    haldorLoc.m_netViews.Find(x => x.GetPrefabName().GetStableHashCode() == zdo.GetPrefab());
                var prefabPosition = zNetView ? zNetView.transform.position : Vector3.zero;
                var resultPos = position + prefabPosition;
                zdo.SetPosition(resultPos);
            }

            lastRelocateDayConfig.Value = EnvMan.instance.GetCurrentDay();
            _self.Config.Reload();
            _self.UpdateConfiguration();
            var zoneIndex = locationTuple.Item1;
            locationTuple.Item2.m_position = position;
            ZoneSystem.instance.m_locationInstances[zoneIndex] = locationTuple.Item2;

            _self.Config.Reload();
            _self.UpdateConfiguration();

            Debug($"{locationName} relocated to position {position}");
        }

        if (sendMapPing) Chat.instance.SendPing(newPosition.ToVector2().ToV3());
        var time = (DateTime.Now - now).TotalSeconds;
        Debug($"Finished search for {locationName} related objects, took {time} seconds.");

        var finishedMsg = $"Relocating {locationName} finished.";
        Debug(finishedMsg);
        if (Console.IsVisible()) Console.instance.AddString($"<color=green>{finishedMsg}</color>");
        Minimap.instance.ForceUpdateLocationPins();
        calculationsFinished = true;
    }


    internal static void GetLocationRelatedObjects(LocationInstance location)
    {
        try
        {
            var zdos = ZDOMan.instance.m_objectsByID.Select(x => x.Value);
            zdos = zdos.ToList().Where(zdo =>
            {
                var netViewMatch = location.m_location.m_netViews.Any(view =>
                    view.gameObject.GetPrefabName().GetStableHashCode() == zdo.GetPrefab());

                var locProxyMatch = zdo.GetInt(ZDOVars.s_location) ==
                                    location.m_location.m_prefabName.GetStableHashCode();

                return netViewMatch || locProxyMatch;
            });

            zdos = zdos.ToList().Where(zdo =>
            {
                var areaMatch = Utils.DistanceXZ(location.m_position, zdo.GetPosition()) <=
                                Mathf.Max(location.m_location.m_exteriorRadius, location.m_location.m_interiorRadius);

                return areaMatch;
            });
            Debug($"location related objects is {zdos.GetString()}");

            objects = zdos.ToList();
        }
        catch (Exception e)
        {
            DebugError($"GetLocationRelatedObjects Error: {e.Message}", false);
            calculationsFinished = true;
        }
    }
}