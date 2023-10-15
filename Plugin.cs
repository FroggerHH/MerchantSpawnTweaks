using System.IO;
using BepInEx;
using BepInEx.Configuration;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace TravelingLocations;

[BepInPlugin(ModGuid, ModName, ModVersion)]
internal class Plugin : BaseUnityPlugin
{
    internal const string
        ModAuthor = "Frogger",
        ModName = "TravelingLocations",
        ModVersion = "2.1.0",
        ModGuid = $"com.{ModAuthor}.{ModName}";

    public static ConfigEntry<int> RelocateIntervalConfig;
    public static ConfigEntry<int> LastRelocateDayConfig;
    public static int RelocateInterval = 1;
    public static int LastRelocateDay;
    public static LocationsConfig locationsConfig = new();

    private static readonly string SecondConfigFileName = $"{ModName.Replace("Frogger.", string.Empty)}.config.yml";

    private void Awake()
    {
        CreateMod(this, ModName, ModAuthor, ModVersion, ModGuid, pathAll: true);
        OnConfigurationChanged += UpdateConfiguration;
        RelocateIntervalConfig = config("General", "RelocateInterval", RelocateInterval,
            "Number of days before merchant relocates. Sit to 0 to disable relocation.");
        LastRelocateDayConfig = config("General", "LastRelocateDay", LastRelocateDay,
            "Number of days before merchant relocates. Sit to 0 to disable relocation.");

        SetupWatcher();
    }

    private void SetupWatcher()
    {
        FileSystemWatcher positionsWatcher = new(Paths.ConfigPath, SecondConfigFileName);
        positionsWatcher.Changed += LoadPositionsFromFile;
        positionsWatcher.IncludeSubdirectories = true;
        positionsWatcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
        positionsWatcher.EnableRaisingEvents = true;

        void LoadPositionsFromFile(object sender, FileSystemEventArgs e) { Plugin.LoadPositionsFromFile(); }
    }

    private void UpdateConfiguration()
    {
        RelocateInterval = RelocateIntervalConfig.Value;
        LastRelocateDay = LastRelocateDayConfig.Value;

        LoadPositionsFromFile();

        if (ZoneSystem.instance && Game.instance)
            foreach (var loc in locationsConfig.locations.Select(x => x.name))
            {
                var location = ZoneSystem.instance.GetLocation(loc);
                if (location == null) DebugError($"Could not find location with name {loc}");
                else if (location.m_unique == false) DebugError($"Location {loc} needs to be unique.");
            }

        Debug("Configuration Received");
    }

    internal static void UpdatePositionsFile()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var path = Path.Combine(Paths.ConfigPath, SecondConfigFileName);
        using (StreamWriter writer = new(path, false))
            writer.Write(serializer.Serialize(locationsConfig.locations));
    }

    public static void LoadPositionsFromFile()
    {
        List<LocationsConfig.LocationConfig> Recreate(string path, IDeserializer deserializer)
        {
            string str1;
            locationsConfig.SetupBasic();
            UpdatePositionsFile();
            using (StreamReader reader = new(path))
            {
                str1 = reader.ReadToEnd();
            }

            return deserializer.Deserialize<List<LocationsConfig.LocationConfig>>(str1);
        }

        var deserializer = new DeserializerBuilder().WithNamingConvention(CamelCaseNamingConvention.Instance).Build();
        var str = string.Empty;
        var path = Path.Combine(Paths.ConfigPath, SecondConfigFileName);
        if (!File.Exists(path)) UpdatePositionsFile();
        using (StreamReader reader = new(path))
        {
            str = reader.ReadToEnd();
        }

        List<LocationsConfig.LocationConfig> deserialize;
        if (!str.IsGood()) deserialize = Recreate(path, deserializer);
        try
        {
            deserialize = deserializer.Deserialize<List<LocationsConfig.LocationConfig>>(str);
        }
        catch (Exception e)
        {
            deserialize = Recreate(path, deserializer);
        }

        locationsConfig.locations = deserialize;
        locationsConfig.GeneratePositionsIfNeeded();

        Debug("Locations positions updated");
    }
}