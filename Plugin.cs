using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace MerchantSpawnTweaks;

[BepInPlugin(ModGUID, ModName, ModVersion)]
internal class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        _self = this;

        Config.SaveOnConfigSet = false;
        //modEnabledConfig = config("General", "Enabled", modEnabled, "Enable this mod");
        relocateIntervalConfig = config("Merchant", "RelocateInterval", relocateInterval,
            "Number of days before merchant relocates. Sit to 0 to disable relocation.");
        lastRelocateDayConfig = config("Merchant", "LastRelocateDay", lastRelocateDay,
            "Number of days before merchant relocates. Sit to 0 to disable relocation.");
        merchantCurrentPositionConfig = config("Merchant", "MerchantPosition", merchantCurrentPosition,
            new ConfigDescription(
                "Current merchant position.", null, new ConfigurationManagerAttributes
                {
                    Browsable = false
                }));
        merchantPositionsConfig = config("Merchant", "All merchant positions", string.Empty,
            "Example:1 1,5 5,66 88  It means haldor can spawn on coordinats x:1 z:1, x:5 z:5 and x:66 z:88");

        savedIDsConfig = config("Do not touch", "savedIDs", string.Empty,
            new ConfigDescription("Do not touch", null, new ConfigurationManagerAttributes
            {
                Browsable = false
            }));
        haldorFirstZoneConfig = config("Do not touch", "Haldor first zone", haldorFirstZone,
            new ConfigDescription("Do not touch", null, new ConfigurationManagerAttributes
            {
                Browsable = false
            }));

        SetupWatcher();
        Config.ConfigReloaded += (_, _) =>
        {
            Debug("Config reloaded.");
            UpdateConfiguration();
        };
        Config.SaveOnConfigSet = true;
        Config.Save();

        //if (!modEnabledConfig.Value)
        //    return;

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGUID);
    }

    internal static KeyValuePair<Vector2i, ZoneSystem.LocationInstance> FindHaldorLocation()
    {
        return ZoneSystem.instance.m_locationInstances.ToList()
            .Find(x => x.Value.m_location.m_prefabName == HALDOR_LOCATION_NAME);
    }


    internal static List<KeyValuePair<Vector2i, ZoneSystem.LocationInstance>> GetHaldors()
    {
        return ZoneSystem.instance.m_locationInstances
            .Where(x => x.Value.m_location.m_prefabName == HALDOR_LOCATION_NAME)
            .ToList();
    }

    internal static ZoneSystem.ZoneLocation GetHaldorPrefab()
    {
        return ZoneSystem.instance.GetLocation(HALDOR_LOCATION_NAME);
    }

    internal static void PrintPingsNames()
    {
        Minimap.instance.m_locationPins.ToList().Select(x => x.Value).ToList()
            .ForEach(x => Debug(x.m_name));
    }

    #region values

    internal const string ModName = "MerchantSpawnTweaks", ModVersion = "1.0.0", ModGUID = "com.Frogger." + ModName;
    internal static Plugin _self;
    internal const string HALDOR_LOCATION_NAME = "Vendor_BlackForest";

    #endregion


    #region tools

    public static void Debug(object msg)
    {
        _self.Logger.LogInfo(msg);
    }

    public static void DebugError(object msg, bool showWriteToDev = true)
    {
        if (showWriteToDev) msg += "Write to the developer and moderator if this happens often.";

        _self.Logger.LogError(msg);
    }

    public static void DebugWarning(object msg, bool showWriteToDev = false)
    {
        if (showWriteToDev) msg += "Write to the developer and moderator if this happens often.";

        _self.Logger.LogWarning(msg);
    }

    #endregion

    #region ConfigSettings

    #region tools

    private static readonly string ConfigFileName = $"{ModGUID}.cfg";
    private DateTime LastConfigChange;

    public static readonly ConfigSync configSync = new(ModName)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

    public static ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
        bool synchronizedSetting = true)
    {
        var configEntry = _self.Config.Bind(group, name, value, description);

        var syncedConfigEntry = configSync.AddConfigEntry(configEntry);
        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

        return configEntry;
    }

    private ConfigEntry<T> config<T>(string group, string name, T value, string description,
        bool synchronizedSetting = true)
    {
        return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
    }

    private void SetupWatcher()
    {
        FileSystemWatcher fileSystemWatcher = new(Paths.ConfigPath, ConfigFileName);
        fileSystemWatcher.Changed += ConfigChanged;
        fileSystemWatcher.IncludeSubdirectories = true;
        fileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        fileSystemWatcher.EnableRaisingEvents = true;
    }

    private void ConfigChanged(object sender, FileSystemEventArgs e)
    {
        if ((DateTime.Now - LastConfigChange).TotalSeconds <= 2) return;

        LastConfigChange = DateTime.Now;
        try
        {
            Config.Reload();
        }
        catch
        {
            DebugError("Can't reload Config");
        }
    }

    internal void ConfigChanged()
    {
        ConfigChanged(null, null);
    }

    #endregion

    #region configs

    //public static ConfigEntry<bool> modEnabledConfig;
    public static ConfigEntry<int> relocateIntervalConfig;
    public static ConfigEntry<int> lastRelocateDayConfig;
    public static ConfigEntry<Vector2> merchantCurrentPositionConfig;
    public static ConfigEntry<string> merchantPositionsConfig;
    public static ConfigEntry<string> savedIDsConfig;
    public static ConfigEntry<Vector2> haldorFirstZoneConfig;

    //public static bool modEnabled;
    public static int relocateInterval = 1;
    public static int lastRelocateDay;
    public static Vector2 merchantCurrentPosition = Vector3.zero;
    public static List<int> savedIDs = new();
    public static List<Vector2> merchantPositions = new();
    public static Vector2 haldorFirstZone = Vector2.zero;

    #endregion

    internal void UpdateConfiguration()
    {
        try
        {
            // modEnabled = modEnabledConfig.Value;
            relocateInterval = relocateIntervalConfig.Value;
            lastRelocateDay = lastRelocateDayConfig.Value;
            merchantCurrentPosition = merchantCurrentPositionConfig.Value;
            haldorFirstZone = haldorFirstZoneConfig.Value;
            var strings = savedIDsConfig.Value.Split(new char[1] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (strings.Length > 0) savedIDs = strings.Select(x => int.Parse(x)).ToList();

            // 1 1, 5 5, 66 88
            SetMerchantPositionsFromString(merchantPositionsConfig.Value);
            Debug("Configuration Received");
        }
        catch (Exception e)
        {
            DebugError(e.Message, false);
        }
    }

    internal static void SetMerchantPositionsFromString(string str)
    {
        merchantPositions = new List<Vector2>();
        var strings = str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        if (strings.Length < 1) return;
        foreach (var vectorString in strings)
        {
            var coords = vectorString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (coords.Length < 2) return;

            var vector2 = new Vector2();
            vector2.x = int.Parse(coords[0]);
            vector2.y = int.Parse(coords[1]);
            merchantPositions.Add(vector2);
        }
    }

    internal static void SetMerchantPositionsConfig()
    {
        merchantPositionsConfig.Value = string.Join(",", merchantPositions.Select(x => $"{x.x} {x.y}"));
        _self.Config.Reload();
        _self.UpdateConfiguration();
    }

    #endregion
}