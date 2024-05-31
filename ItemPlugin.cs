using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using CessilCellsCeaChells.CeaChore;
using HarmonyLib;
using MyceliumNetworking;
using UnityEngine;
using Zorro.Core;

namespace ItemLibrary
{
    [ContentWarningPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_VERSION, false)]
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency(MyceliumNetworking.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("hyydsz-ShopUtils", BepInDependency.DependencyFlags.SoftDependency)] // Compatibility with ShopUtils, we *don't* have to load after it but just to make sure
    [BepInDependency("Notest.ItemIDPlugin", BepInDependency.DependencyFlags.SoftDependency)]
    public class ItemPlugin : BaseUnityPlugin
    {
        public static ItemPlugin Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger { get; private set; } = null!;
        internal static Harmony? Harmony { get; set; }
        internal static uint modID = (uint)Hash128.Compute(MyPluginInfo.PLUGIN_GUID).GetHashCode();

        private void Awake()
        {
            Logger = base.Logger;
            Instance = this;
            
            MyceliumNetwork.RegisterNetworkObject(this, modID);

            MyceliumNetwork.LobbyEntered += ItemHandler.OnLobbyEntered;
            MyceliumNetwork.LobbyLeft += ItemHandler.OnLobbyLeft;

            MyceliumNetwork.LobbyEntered += EntryHandler.OnLobbyEntered;
            MyceliumNetwork.LobbyLeft += EntryHandler.OnLobbyLeft;
            EntryHandler.GetEntryCount();

            Patch();

            Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
        }

        internal static void Patch()
        {
            Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

            Logger.LogDebug("Patching...");

            Harmony.PatchAll();

            Logger.LogDebug("Finished patching!");
        }

        internal static void Unpatch()
        {
            Logger.LogDebug("Unpatching...");

            Harmony?.UnpatchSelf();

            Logger.LogDebug("Finished unpatching!");
        }
    }
}
