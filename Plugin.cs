using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using Extensions;
using HarmonyLib;
using ServerSync;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
        //locationsPositionsConfig = config("Merchant", "All merchant positions", string.Empty,
        //    "Example:1 1,5 5,66 88  It means haldor can spawn on coordinates x:1 z:1, x:5 z:5 and x:66 z:88");

        locationsToMoveConfig = config("Main", "LocationsToMove", "Vendor_BlackForest, Hildir_camp", "");
        SetupWatcher();
        Config.ConfigReloaded += (_, _) => UpdateConfiguration();
        Config.SaveOnConfigSet = true;
        Config.Save();

        //if (!modEnabledConfig.Value)
        //    return;

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), ModGUID);
    }

    #region values

    internal const string ModName = "Frogger.MerchantSpawnTweaks", ModVersion = "1.1.0", ModGUID = "com." + ModName;

    internal static Plugin _self;
    //internal const string HALDOR_LOCATION_NAME = "Vendor_BlackForest";

    #endregion

    //internal static ZoneSystem.ZoneLocation GetHaldorPrefab() => ZoneSystem.instance.GetLocation(HALDOR_LOCATION_NAME);

    #region tools

    public static void Debug(object msg, bool showInConsole = false)
    {
        _self.Logger.LogInfo(msg);
        if (showInConsole && Console.IsVisible()) Console.instance.AddString(msg.ToString());
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
        FileSystemWatcher mainConfigFileSystemWatcher = new(Paths.ConfigPath, ConfigFileName);
        mainConfigFileSystemWatcher.Changed += ConfigChanged;
        mainConfigFileSystemWatcher.IncludeSubdirectories = true;
        mainConfigFileSystemWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        mainConfigFileSystemWatcher.EnableRaisingEvents = true;

        FileSystemWatcher positionsWatcher = new(Paths.ConfigPath, secondConfigFileName);
        positionsWatcher.Changed += LoadPositionsFromFile;
        positionsWatcher.IncludeSubdirectories = true;
        positionsWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        positionsWatcher.EnableRaisingEvents = true;

        void LoadPositionsFromFile(object sender, FileSystemEventArgs e) => Plugin.LoadPositionsFromFile();
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
    //public static ConfigEntry<string> locationsPositionsConfig;

    public static ConfigEntry<string> locationsToMoveConfig;

    //public static bool modEnabled;
    public static int relocateInterval = 1;
    public static int lastRelocateDay;

    public static List<string> locationsToMove = new();

    public static Dictionary<string, List<SimpleVector2>> locationsPositions = new()
    {
        {
            "Vendor_BlackForest", new()
        },
        {
            "Hildir_camp", new()
        }
    };

    #endregion

    internal void UpdateConfiguration()
    {
        try
        {
            relocateInterval = relocateIntervalConfig.Value;
            lastRelocateDay = lastRelocateDayConfig.Value;
            locationsToMove = locationsToMoveConfig.Value
                .Split(new string[1] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();

            LoadPositionsFromFile();


            if (ZoneSystem.instance)
            {
                foreach (var loc in locationsToMove)
                {
                    var where = ZoneSystem.instance.m_locationInstances
                        .Where(x => x.Value.m_location.m_prefabName == loc)
                        .ToDictionary(x => x.Key, y => y.Value);

                    var first = where.First();
                    where.Remove(first.Key);
                    ZoneSystem.instance.m_locationInstances =
                        ZoneSystem.instance.m_locationInstances.Where(x => !where.Contains(x))
                            .ToDictionary(x => x.Key, y => y.Value);
                }
            }

            Debug("Configuration Received");
        }
        catch (Exception e)
        {
            DebugError($"Configuration error: {e.Message}", false);
        }
    }

    // internal static void SetMerchantPositionsFromString()
    // {
    //     var deserializer = new DeserializerBuilder()
    //         .WithNamingConvention(CamelCaseNamingConvention.Instance)
    //         .Build();
    //
    //     locationsPositions = deserializer.Deserialize<Dictionary<string, List<Vector2>>>(str);
    // }

    internal static void UpdatePositionsFile()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var path = Path.Combine(Paths.ConfigPath, secondConfigFileName);
        using (StreamWriter writer = new(path, false)) writer.Write(serializer.Serialize(locationsPositions));
    }

    static string secondConfigFileName = $"{ModName.Replace("Frogger.", string.Empty)}.config.yml";

    internal static void LoadPositionsFromFile()
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var str = string.Empty;
        var path = Path.Combine(Paths.ConfigPath, secondConfigFileName);
        if (!File.Exists(path)) UpdatePositionsFile();
        using (StreamReader reader = new(path)) str = reader.ReadToEnd();
        locationsPositions = deserializer.Deserialize<Dictionary<string, List<SimpleVector2>>>(str);
    }

    #endregion
}