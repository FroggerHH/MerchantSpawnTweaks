using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;

namespace TravelingLocations;

[Serializable]
public class LocationsConfig
{
    public List<LocationConfig> locations = new();

    [Serializable]
    public class LocationConfig
    {
        public string name = string.Empty;
        public bool clearAreaAfterRelocating = false;
        public List<string> objectsToClear = new();
        public List<SimpleVector2> positions = new();
    }

    public LocationsConfig()
    {
    }

    public List<string> GetAllLocationsNames() => locations.Select(x => x.name).ToList();
    public LocationConfig GetLocationConfig(string name) => locations.Find(x=> x.name == name);
}