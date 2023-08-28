using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Extensions;
using UnityEngine;
using static ZoneSystem;
using static HeightmapBuilder;
using static TravelingLocations.Plugin;

namespace TravelingLocations;

public static class Relocator
{
    private static bool calculationsFinished;
    private static List<ZDO> objects = new();
    private static List<Task> tasks = new();
    private static bool busy;

    public static void RandomlyRelocateLocations(bool sendMapPing = false)
    {
        //if(busy) return;
        busy = true;
        tasks.ForEach(x => x.Dispose());
        tasks = new List<Task>();
        _self.StopAllCoroutines();
        _self.StartCoroutine(RandomlyRelocateLocations_IEnumerator(sendMapPing));
    }

    private static IEnumerator RandomlyRelocateLocations_IEnumerator(bool sendMapPing = false)
    {
        var now = DateTime.Now;
        calculationsFinished = true;
        foreach (var locationName in locationsPositions.Keys.ToList())
        {
            var locationTuple = ZoneSystem.instance.GetGeneratedLocationsByName(locationName).First();
            if (locationTuple.Item2.m_location != null && !locationTuple.Item2.m_placed)
            {
                var msg = $"For first relocation {locationName} location needs to be explored by someone.";
                DebugWarning(msg);
                if (Console.IsVisible()) Console.instance.AddString($"<color=yellow>{msg}</color>");
                continue;
            }

            calculationsFinished = false;
            tasks.Add(Task.Run(() =>
            {
                Debug("0");
                GetLocationRelatedObjects(locationTuple.Item2);
                Debug("1");
                return Task.CompletedTask;
            }));
            Debug("2");
            yield return new WaitUntil(() => calculationsFinished);
            Debug("3");
            if (objects.Count <= 0)
            {
                DebugError("No objects found", false);
            }
            else
            {
                Debug($"Finding new place to relocate {locationName}...");
                var newPosition = locationsPositions[locationName].Random();
                var position = newPosition.ToVector2().ToV3();
                var data = new HMBuildData(position, 1, 1, false, WorldGenerator.instance);
                HeightmapBuilder.instance.Build(data);
                position.y = data.m_baseHeights[0];

                var haldorLoc = ZoneSystem.instance.GetLocation(locationName);
                haldorLoc.m_prefab.transform.position = Vector3.zero;
                haldorLoc.m_prefab.transform.rotation = Quaternion.identity;
                foreach (var zdo in objects)
                {
                    if (zdo == null) continue;
                    var prefabPosition = Vector3.zero;
                    tasks.Add(Task.Run(() =>
                    {
                        calculationsFinished = false;
                        var zNetView = haldorLoc.m_netViews.Find(x =>
                            x.GetPrefabName().GetStableHashCode() == zdo.GetPrefab());
                        prefabPosition = zNetView ? zNetView.transform.position : Vector3.zero;
                        calculationsFinished = true;
                        return Task.CompletedTask;
                    }));
                    yield return new WaitUntil(() => calculationsFinished = true);
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

                if (sendMapPing) Chat.instance.SendPing(newPosition.ToVector2().ToV3());

                var finishedMsg = $"Relocating {locationName} finished.";

                Debug(finishedMsg);
                if (Console.IsVisible()) Console.instance.AddString($"<color=green>{finishedMsg}</color>");
                Minimap.instance.ForceUpdateLocationPins();
                calculationsFinished = true;
            }
        }

        var time = (DateTime.Now - now).TotalSeconds;
        Debug($"Finished relocating, took {time} seconds.");
        busy = false;
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
            calculationsFinished = true;
        }
        catch (Exception e)
        {
            DebugError($"GetLocationRelatedObjects Error: {e.Message}", false);
            calculationsFinished = true;
        }
    }
}