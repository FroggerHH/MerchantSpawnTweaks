using System;
using System.Collections.Generic;
using Extensions;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using static MerchantSpawnTweaks.Plugin;
using static Terminal;

namespace MerchantSpawnTweaks;

public static class TerminalCommands
{
    private static bool isServer => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
    private static string modName => ModName;


    [HarmonyPatch(typeof(Terminal), nameof(InitTerminal))]
    [HarmonyWrapSafe]
    internal class AddChatCommands
    {
        private static void Postfix()
        {
            new ConsoleCommand("RandomlyRelocateLocations", "The merchant will move to one of the allowed positions",
                args =>
                {
                    try
                    {
                        if (!configSync.IsAdmin && !ZNet.instance.IsServer())
                        {
                            args.Context.AddString("You are not an admin on this server.");
                            return;
                        }

                        Relocator.RandomlyRelocateLocations(true);

                        args.Context.AddString("Done, merchant randomly relocated. New position pinged.");
                    }
                    catch (Exception e)
                    {
                        args.Context.AddString("<color=red>Error: " + e.Message + "</color>");
                    }
                }, true);
            new ConsoleCommand("addMerchantPos",
                "[Location name] Player position will be added to list of valid positions for merchant to spawn",
                args =>
                {
                    try
                    {
                        if (!configSync.IsAdmin && !ZNet.instance.IsServer())
                        {
                            args.Context.AddString("You are not an admin on this server.");
                            return;
                        }

                        if (args.Args.Length == 1)
                            throw new Exception("First argument must be a location name (string)");

                        var locName = args[1];
                        if (!locationsToMove.Contains(locName)) locationsToMove.Add(locName);

                        var newPos =
                            (Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2f +
                             Vector3.up).RoundCords().ToV2().ToSimpleVector2();

                        if (locationsPositions.ContainsKey(locName)) locationsPositions[locName].TryAdd(newPos);
                        else locationsPositions.Add(locName, new() { newPos });

                        args.Context.AddString($"Done, position {newPos} added");
                        UpdatePositionsFile();
                    }
                    catch (Exception e)
                    {
                        args.Context.AddString("<color=red>Error: " + e.Message + "</color>");
                    }
                }, true);

            new ConsoleCommand("createNewRandomPositionsForHaldor",
                "Adds [count] random positions to the [Location name] to relocate",
                args =>
                {
                    try
                    {
                        if (!ZoneSystem.instance)
                            throw new Exception("Command cannot be executed in game menu");

                        if (args.Args.Length < 2 || !int.TryParse(args.Args[1], out var count))
                            throw new Exception("First argument must be a number");
                        if (args.Args.Length < 3)
                            throw new Exception("First argument must be a location name (string)");

                        var location = ZoneSystem.instance.GetLocation(args[2]);
                        var haldorLocationsVanillaCount = location.m_quantity;
                        location.m_quantity = count;
                        var isUnique = location.m_unique;
                        location.m_unique = false;

                        ZoneSystem.instance.GenerateLocations(location);

                        args.Context.AddString(
                            $"Done. Now this is list of all haldor possible positions: {locationsPositions.GetString()}");
                        location.m_quantity = haldorLocationsVanillaCount;
                        location.m_unique = isUnique;
                    }
                    catch (Exception e)
                    {
                        args.Context.AddString("<color=red>Error: " + e.Message + "</color>");
                    }
                }, true);
        }
    }
}