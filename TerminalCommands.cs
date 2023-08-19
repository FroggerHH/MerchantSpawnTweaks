using System;
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
            new ConsoleCommand("RandomlyRelocateMerchant", "The merchant will move to one of the allowed positions",
                args =>
                {
                    try
                    {
                        if (!configSync.IsAdmin && !ZNet.instance.IsServer())
                        {
                            args.Context.AddString("You are not an admin on this server.");
                            return;
                        }

                        var newPos = Relocator.RandomlyRelocateMerchant();
                        if (newPos == Vector2.zero)
                        {
                            args.Context.AddString(
                                "<color=yellow>For first relocation haldor location needs to be explored by someone.</color>");
                            return;
                        }

                        args.Context.AddString("Done, merchant randomly relocated. New position pinged.");
                        Chat.instance.SendPing(newPos.ToV3());
                    }
                    catch (Exception e)
                    {
                        args.Context.AddString("<color=red>Error: " + e.Message + "</color>");
                    }
                }, true);
            new ConsoleCommand("addMerchantPos",
                "Player position will be added to list of valid positions for merchant to spawn",
                args =>
                {
                    try
                    {
                        if (!configSync.IsAdmin && !ZNet.instance.IsServer())
                        {
                            args.Context.AddString("You are not an admin on this server.");
                            return;
                        }

                        var newPos =
                            (Player.m_localPlayer.transform.position + Player.m_localPlayer.transform.forward * 2f +
                             Vector3.up).RoundCords().ToV2();
                        if (merchantPositions.Contains(newPos))
                        {
                            args.Context.AddString($"Position {newPos} already exists");
                            return;
                        }

                        args.Context.AddString($"Done, position {newPos} added");
                        merchantPositions.TryAdd(newPos);
                        SetMerchantPositionsConfig();
                    }
                    catch (Exception e)
                    {
                        args.Context.AddString("<color=red>Error: " + e.Message + "</color>");
                    }
                }, true);

            new ConsoleCommand("createNewRandomPositionsForHaldor",
                "Adds [count] random positions to the Haldor to relocate",
                args =>
                {
                    try
                    {
                        if (!ZoneSystem.instance)
                            throw new Exception("Command cannot be executed in game menu");

                        if (args.Args.Length == 1 || !int.TryParse(args.Args[1], out var count))
                            throw new Exception("First argument must be a number");

                        var haldorPrefab = GetHaldorPrefab();
                        var haldorLocationsVanilaCount = haldorPrefab.m_quantity;
                        haldorPrefab.m_quantity = count;
                        var isUnique = haldorPrefab.m_unique;
                        haldorPrefab.m_unique = false;

                        ZoneSystem.instance.GenerateLocations(haldorPrefab);

                        args.Context.AddString(
                            $"Done. Now this is list of all haldor posible positions: {merchantPositions.GetString()}");
                        haldorPrefab.m_quantity = haldorLocationsVanilaCount;
                        haldorPrefab.m_unique = isUnique;
                    }
                    catch (Exception e)
                    {
                        args.Context.AddString("<color=red>Error: " + e.Message + "</color>");
                    }
                }, true);
        }
    }
}