using static Terminal;

namespace TravelingLocations;

public static class TerminalCommands
{
    [HarmonyPatch(typeof(Terminal), nameof(InitTerminal))]
    [HarmonyWrapSafe]
    internal class AddChatCommands
    {
        private static void Postfix()
        {
            new ConsoleCommand("randomlyrelocatelocs", "",
                args =>
                {
                    RunCommand(args =>
                    {
                        if (!IsAdmin) throw new ConsoleCommandException("You are not an admin on this server.");

                        Relocator.RandomlyRelocateLocations(true);

                        args.Context.AddString(
                            "<color=yellow>Wait for it, it is a long process. Do not spam command</color>");
                    }, args);
                }, true);
            new ConsoleCommand("addPos",
                "[Location name] Player position will be added to list of valid positions for merchant to spawn",
                args =>
                {
                    RunCommand(args =>
                    {
                        if (!IsAdmin) throw new ConsoleCommandException("You are not an admin on this server.");

                        if (args.Args.Length == 1)
                            throw new ConsoleCommandException("First argument must be a location name (string)");

                        var locName = args[1];

                        var newPos =
                            (Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2f +
                             Vector3.up).RoundCords().ToV2().ToSimpleVector2();

                        if (locationsConfig.GetAllLocationsNames().Contains(locName))
                            locationsConfig.GetLocationConfig(locName).positions.TryAdd(newPos);
                        else
                            locationsConfig.locations.Add(new LocationsConfig.LocationConfig
                            {
                                clearAreaAfterRelocating = true,
                                name = locName,
                                positions = new List<SimpleVector2> { newPos }
                            });

                        args.Context.AddString($"Done, position {newPos} added");
                        UpdatePositionsFile();
                    }, args);
                }, true);

            new ConsoleCommand("createRandomPositions",
                "Adds [count] random positions to the [Location name] to relocate",
                args =>
                {
                    RunCommand(args =>
                    {
                        if (!ZoneSystem.instance)
                            throw new ConsoleCommandException("Command cannot be executed in game menu");

                        if (!IsAdmin) throw new ConsoleCommandException("You are not an admin on this server.");

                        if (args.Args.Length < 2)
                            throw new ConsoleCommandException("First argument must be a number");
                        if (!int.TryParse(args.Args[1], out var count))
                            throw new ConsoleCommandException($"{args.Args[1]} is not valid a number");
                        if (args.Args.Length < 3)
                            throw new ConsoleCommandException("First argument must be a location name (string)");

                        var locationName = args[2];
                        var location = locationsConfig.GetLocationConfig(locationName);
                        if (!location)
                            locationsConfig.locations.Add(
                                new LocationsConfig.LocationConfig { name = locationName });

                        var placesForLocation = ZoneSystem.instance.CreateValidPlacesForLocation(locationName, count);
                        foreach (var newPos in placesForLocation)
                            location.positions.Add(newPos.ToV2().ToSimpleVector2());
                        location.positions._Distinct();

                        UpdatePositionsFile();

                        args.Context.AddString("Done.");
                    }, args);
                }, true);
        }
    }
}