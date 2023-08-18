using System;
using Extensions;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using static MerchantSpawnTweaks.Plugin;

namespace MerchantSpawnTweaks;

public static class TerminalCommands
{
    private static bool isServer => SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null;
    private static string modName => ModName;


    [HarmonyPatch(typeof(Terminal), nameof(Terminal.InitTerminal))]
    [HarmonyWrapSafe]
    internal class AddChatCommands
    {
        private static void Postfix()
        {
            new Terminal.ConsoleCommand("RandomlyRelocateMerchant", "",
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
            new Terminal.ConsoleCommand("addMerchantPos",
                "player position will be added to list of valid positions for merchant to spawn",
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
                        merchantPositions.Add(newPos);
                        SetMerchantPositionsConfig();
                    }
                    catch (Exception e)
                    {
                        args.Context.AddString("<color=red>Error: " + e.Message + "</color>");
                    }
                }, true);
        }
    }
}