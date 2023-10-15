using System.Diagnostics.CodeAnalysis;

namespace TravelingLocations;

[Serializable]
public class LocationsConfig
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    internal static Dictionary<Biome, List<string>> ClearBiomeDictionary = new()
    {
        {
            BlackForest, new List<string>
            {
                "rock4_forest", "rock4_copper", "MineRock_Tin", "Rock_4", "Rock_3", "FirTree",
                "Pinetree_01", "FirTree_small", "FirTree_oldLog",
                "stubbe", "shrub_2", "BlueberryBush", "Pickable_Tin",
                "Pickable_SeedCarrot", "Spawner_GreydwarfNest", "Greydwarf_Root",
                "StatueSeed", "stone_wall_2x1", "TreasureChest_blackforest", "wood_door",
                "stone_arch", "stone_wall_1x1", "wood_stair", "wood_floor", "wood_pole", "wood_beam",
                "wood_roof", "wood_roof_top", "piece_chair", "barrell", "Pickable_Mushroom_yellow",
                "TreasureChest_trollcave",
                "Pickable_ForestCryptRemains01", "Pickable_ForestCryptRemains02",
                "Pickable_ForestCryptRemains03", "stone_floor_2x2", "stone_stair", "Beehive",
                "piece_table", "wood_wall_roof", "wood_floor_1x1", "wood_stepladder", "stone_wall_4x2",
                "CastleKit_groundtorch"
            }
        },
        {
            Meadows, new List<string>
            {
                "Beech1", "Birch1", "Birch2", "Oak1", "Beech_small1", "Beech_small2", "stubbe", "FirTree_oldLog",
                "Bush01",
                "shrub_2", "RaspberryBush", "Pickable_Dandelion", "Pickable_Flint", "Pickable_Mushroom",
                "Pickable_Stone",
                "Pickable_Branch", "TreasureChest_meadows", "vines", "wood_floor", "goblin_bed",
                "wood_door", "wood_roof_45", "wood_wall_roof_top_45", "woodwall", "wood_beam_45", "wood_wall_roof_45",
                "wood_wall_half", "wood_roof_top_45", "wood_pole_log", "wood_wall_log_4x0.5", "wood_roof",
                "wood_beam_26",
                "wood_pole", "wood_stepladder", "wood_floor_1x1", "wood_pole2", "wood_stair", "wood_wall_roof_top",
                "wood_wall_roof", "wood_fence", "wood_roof_top", "Rock_7", "TreasureChest_meadows_buried",
                "Pickable_ForestCryptRemains01", "Pickable_ForestCryptRemains02"
            }
        },
        {
            Mistlands, new List<string>
            {
                "HugeRoot1", "SwampTree2_darkland", "Rock_4", "Rock_3", "Pinetree_01", "FirTree_small_dead", "stubbe",
                "Skull1", "Skull2", "GlowingMushroom", "cliff_mistlands1", "cliff_mistlands2", "rock_mistlands1",
                "rock_mistlands2", "giant_skull", "giant_helmet1", "giant_helmet2", "giant_sword1", "giant_sword2",
                "giant_ribs", "YggdrasilRoot", "YggaShoot1", "YggaShoot2", "YggaShoot3", "YggaShoot_small1",
                "RaspberryBush", "Pickable_Mushroom_Magecap", "Pickable_Mushroom_JotunPuffs", "blackmarble_floor_large",
                "blackmarble_2x2_enforced", "blackmarble_2x2x2", "blackmarble_2x2x1", "blackmarble_stair",
                "blackmarble_slope_1x2", "blackmarble_slope_inverted_1x2", "blackmarble_base_1", "vines",
                "blackmarble_1x1", "blackmarble_head01", "blackmarble_head02", "dvergrprops_curtain",
                "dvergrprops_lantern", "dvergrprops_wood_pole", "piece_dvergr_wood_door", "dvergrprops_wood_beam",
                "piece_dvergr_metal_wall_2x2", "blackmarble_floor_triangle", "dvergrprops_chair", "dverger_guardstone",
                "Pickable_DvergrLantern", "dvergrprops_table", "dvergrprops_stool", "Pickable_DvergrStein",
                "dvergrtown_stair_corner_wood_left", "dvergrprops_wood_floor", "dvergrprops_banner",
                "blackmarble_base_2", "blackmarble_head_big01", "blackmarble_tip", "blackmarble_column_3",
                "blackmarble_head_big02", "blackmarble_floor", "blackmarble_column_1", "blackmarble_column_2",
                "blackmarble_post01", "dverger_demister", "dvergrprops_crate", "dvergrprops_wood_stakewall",
                "dvergrprops_bed", "dvergrprops_crate_long", "dverger_demister_broken", "dvergrprops_wood_stair",
                "wood_floor_1x1", "dvergrprops_barrel", "dverger_demister_large", "dvergrprops_wood_wall",
                "trader_wagon_destructable", "giant_brain", "dvergrprops_hooknchain", "dvergrtown_wood_crane",
                "blackmarble_basecorner", "blackmarble_arch", "iron_floor_2x2", "blackmarble_outcorner",
                "blackmarble_out_1", "iron_wall_2x2", "blackmarble_stair_corner", "dvergrtown_wood_beam",
                "dvergrtown_wood_support", "CreepProp_hanging01", "CreepProp_wall01", "dverger_demister_ruins",
                "CreepProp_entrance1", "CreepProp_entrance2", "woodwall", "piece_blackmarble_bench",
                "dungeon_queen_door", "dvergrprops_shelf"
            }
        },
        {
            Plains, new List<string>
            {
                "rock4_heath", "HeathRockPillar", "rock2_heath", "Rock_4_plains", "Birch1_aut", "Birch2_aut",
                "Bush02_en", "CloudberryBush", "Bush01_heath", "shrub_2_heath", "HugeStone1", "rockformation1",
                "rock_a", "Pickable_Barley", "RockFinger", "RockThumb", "RockFingerBroken", "CastleKit_groundtorch",
                "goblinking_totemholder", "wood_wall_roof", "wood_roof", "stone_wall_2x1", "TreasureChest_heath",
                "stone_wall_1x1", "stone_arch", "stone_stair", "stone_floor", "stone_wall_4x2", "stone_pillar",
                "goblin_woodwall_1m", "goblin_pole_small", "goblin_fence", "goblin_banner", "goblin_roof_45d_corner",
                "goblin_roof_45d", "goblin_totempole", "wood_stepladder", "goblin_woodwall_2m_ribs", "highstone",
                "widestone", "TreasureChest_plains_stone", "Rock_3", "Pickable_TarBig", "TarLiquid", "Pickable_Tar",
                "lox_ribs"
            }
        },
        {
            Swamp, new List<string>
            {
                "Rock_4", "mudpile_beacon", "FirTree_small_dead", "SwampTree1", "SwampTree2", "SwampTree2_log",
                "stubbe", "StatueEvil", "FirTree_oldLog", "shrub_2_heath", "Pickable_BogIronOre", "Pickable_SeedTurnip",
                "BonePileSpawner", "TreasureChest_swamp", "Spawner_DraugrPile", "stone_wall_2x1", "stone_wall_1x1",
                "stone_stair", "CastleKit_groundtorch_green", "sunken_crypt_gate", "GuckSack", "GuckSack_small",
                "wood_door", "wood_wall_half", "wood_pole_log_4", "wood_floor", "wood_beam", "woodwall", "wood_pole2",
                "wood_pole", "wood_stepladder", "wood_floor_1x1", "piece_table", "wood_roof_45", "wood_wall_roof_45",
                "TreasureChest_blackforest", "wood_pole_log", "goblin_bed", "wood_stair"
            }
        },
        {
            Mountain, new List<string>
            {
                "rock3_mountain", "rock3_silver", "silvervein", "rock1_mountain", "rock2_mountain", "FirTree_small",
                "FirTree", "MineRock_Obsidian", "stone_wall_2x1", "stone_wall_1x1", "wood_stepladder", "wood_floor",
                "stone_arch", "TreasureChest_mountains", "stone_floor_2x2", "stone_stair", "wood_floor_1x1",
                "stone_floor", "BonePileSpawner", "dragoneggcup", "Pickable_DragonEgg", "marker01", "marker02",
                "wood_wall_log_4x0.5", "wood_wall_roof", "wood_roof", "wood_wall_log", "wood_door", "wood_pole_log",
                "piece_chair02", "piece_table", "wood_stack", "wood_beam_26", "wood_roof_top", "wood_wall_roof_top",
                "wood_roof_45", "wood_wall_roof_45", "wood_roof_top_45", "wood_wall_roof_top_45",
                "Pickable_MountainRemains01_buried", "MountainGraveStone01", "caverock_ice_stalagmite",
                "caverock_ice_stalagtite", "rock3_ice", "ice_rock1", "MountainKit_brazier_blue"
            }
        }
    };

    public List<LocationConfig> locations = new();

    public LocationsConfig() { SetupBasic(); }

    internal void SetupBasic()
    {
        if (!locations.Any(x => x.name.StartsWith("Vendor_BlackForest")))
            locations.Add(new LocationConfig
            {
                name = "Vendor_BlackForest-GeneratePositions",
                clearAreaAfterRelocating = true
            });
        if (!locations.Any(x => x.name.StartsWith("Hildir_camp")))
            locations.Add(new LocationConfig
            {
                name = "Hildir_camp-GeneratePositions",
                clearAreaAfterRelocating = true
            });
    }

    public List<string> GetAllLocationsNames() { return locations.Select(x => x.name).ToList(); }

    public LocationConfig GetLocationConfig(string name) { return locations.Find(x => x.name == name); }

    public bool GetLocationConfig(string name, out LocationConfig locationConfig)
    {
        locationConfig = locations.Find(x => x.name == name);
        return locationConfig;
    }

    public void GeneratePositionsIfNeeded()
    {
        var smtChanged = false;
        foreach (var configLocation in locations)
        {
            if (!configLocation || !configLocation.name.Contains("-GeneratePositions")) continue;

            configLocation.name = configLocation.name.Replace("-GeneratePositions", string.Empty);
            configLocation.positions = ZoneSystem.instance.CreateValidPlacesForLocation(configLocation.name, 15)
                .Select(x => x.ToV2().ToSimpleVector2()).ToList();
            smtChanged = true;
        }

        if (smtChanged) UpdatePositionsFile();
    }

    [Serializable]
    public class LocationConfig
    {
        public string name = string.Empty;
        public bool clearAreaAfterRelocating;
        public List<SimpleVector2> positions = new();

        public static implicit operator bool(LocationConfig locationConfig) { return locationConfig != null; }
    }
}