using System.Diagnostics;
using System.Threading.Tasks;
using static HeightmapBuilder;

namespace TravelingLocations;

public static class Relocator
{
    private static bool busy;

    public static void RandomlyRelocateLocations(bool sendMapPing = false)
    {
        RandomlyRelocateLocationsTask(sendMapPing);
    }

    private static async Task RandomlyRelocateLocationsTask(bool sendMapPing = false)
    {
        busy = true;
        var watch = Stopwatch.StartNew();
        foreach (var locationName in locationsConfig.GetAllLocationsNames())
        {
            var (zoneID, locationInstance) =
                ZoneSystem.instance.GetGeneratedLocationsByName(locationName).FirstOrDefault();
            if (locationInstance.m_location != null && !locationInstance.m_placed)
            {
                var msg = $"For first relocation {locationName} location needs to be explored by someone.";
                DebugWarning(msg);
                if (Console.IsVisible()) Console.instance.AddString($"<color=yellow>{msg}</color>");
                continue;
            }

            var zdos = await Task.Run(() => GetLocationRelatedObjects(locationInstance));

            if (zdos.Count <= 0)
            {
                DebugError("No objects found", false);
            } else
            {
                Debug($"Finding new place to relocate {locationName}...");
                var locationConfig = locationsConfig.GetLocationConfig(locationName);
                var newPosition = locationConfig.positions.Random();
                var position = newPosition.ToVector2().ToV3();
                var data = new HMBuildData(position, 1, 1, false, WorldGenerator.instance);
                HeightmapBuilder.instance.Build(data);
                position.y = data.m_baseHeights[0];

                var haldorLoc = ZoneSystem.instance.GetLocation(locationName);
                haldorLoc.m_prefab.transform.position = Vector3.zero;
                haldorLoc.m_prefab.transform.rotation = Quaternion.identity;
                foreach (var zdo in zdos)
                {
                    if (zdo == null || !zdo.IsValid()) continue;
                    var prefabPosition = await Task.Run(() =>
                    {
                        var zNetView = haldorLoc.m_netViews.Find(x =>
                            x.GetPrefabName().GetStableHashCode() == zdo.GetPrefab());
                        return zNetView ? zNetView.transform.position : Vector3.zero;
                    });

                    var resultPos = position + prefabPosition;
                    zdo.SetPosition(resultPos);
                }

                LastRelocateDayConfig.Value = EnvMan.instance.GetCurrentDay();
                GetPlugin().Config.Reload();
                OnConfigurationChanged?.Invoke();
                locationInstance.m_position = position;
                ZoneSystem.instance.m_locationInstances[zoneID] = locationInstance;
                GetPlugin().Config.Reload();
                OnConfigurationChanged?.Invoke();
                Debug($"{locationName} relocated to position {position}");
                if (sendMapPing) Chat.instance.SendPing(newPosition.ToVector2().ToV3());

                var finishedMsg = $"Relocating {locationName} finished.";

                Debug(finishedMsg);
                Minimap.instance.ForceUpdateLocationPins();
            }
        }

        watch.Stop();
        Debug($"Finished relocating, took {watch.Elapsed.TotalSeconds} seconds.");
        busy = false;
    }

    internal static List<ZDO> GetLocationRelatedObjects(LocationInstance location)
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
            return zdos.ToList();
        }
        catch (Exception e)
        {
            DebugError($"GetLocationRelatedObjects Error: {e.Message}", false);
        }

        return new List<ZDO>();
    }
}