// using HarmonyLib;
// using JetBrains.Annotations;
// using static TravelingLocations.Plugin;
//
// namespace TravelingLocations.Patch;
//
// [HarmonyPatch]
// public static class FillClearLists
// {
//     [HarmonyPatch(typeof(ZoneSystem), nameof(ZoneSystem.Start))]
//     [HarmonyWrapSafe, HarmonyPostfix]
//     private static void AwakePostfix()
//     {
//         if (!ZNet.instance.IsServer()) return;
//
//         var MountainClear = new List<string>();
//         var temp = new List<GameObject>();
//         var vegetations = ZoneSystem.instance.m_vegetation.Where(x => x.m_biome == Mountain)
//             .Select(x => x.m_prefab);
//         var locations = ZoneSystem.instance.m_locations.Where(x => x.m_biome == Mountain && x.m_netViews != null && x
//             .m_netViews.Count > 0);
//         var netViews = new List<GameObject>();
//         foreach (var location in locations) netViews.AddRange(location.m_netViews.Select(x => x.gameObject));
//
//         temp.AddRange(vegetations);
//         temp.AddRange(netViews);
//         temp = temp.Distinct().ToList();
//         temp.RemoveAll(x => !x.GetComponentInChildren<Collider>());
//         temp = temp.Distinct().ToList();
//
//         MountainClear = temp.Select(x => x.GetPrefabName()).ToList();
//         MountainClear = MountainClear.Distinct().ToList();
//
//         Debug("MountainClear \n" + MountainClear.Select(x => $"\"{x}\"").GetString());
//     }
// }

