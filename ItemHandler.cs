using System;
using MyceliumNetworking;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx;
using ConfigSync;
using HarmonyLib;
using ItemLibrary.Wrappers;
using UnityEngine;
using WebSocketSharp;
using Zorro.Core;
using static UnityEngine.Rendering.VolumeComponent;

namespace ItemLibrary
{
    public static class ItemHandler
    {
        internal static int initialItemCount; // Use it to remove from range whenever we leave a lobby | 0 based, number is the last index used
        public static List<Item> ItemList = new List<Item>();
        public static List<Item> TemporaryItemList = new List<Item>();
        
        internal static void AddAllItemsToDatabase()
        {
            ItemDatabase database = SingletonAsset<ItemDatabase>.Instance;
            Item[] itemListArray = ItemList.ToArray();
            
            database.Objects = database.Objects.AddRangeToArray(itemListArray);
        }

        internal static void OnLobbyEntered()
        {
            if (!MyceliumNetwork.IsHost)
                return;

            int itemCount = initialItemCount;

            for (int index = 0; index < TemporaryItemList.Count; index++)
            {
                //If we never update the value of the config it will never be inserted in non-host clients either, wink wink
                if (TemporaryItemList[index].useInGame == false)
                    continue;
                
                itemCount++;
                string GUIDCombo = TemporaryItemList[index].modGuid + TemporaryItemList[index].displayName;

                if (!IDPatcherCompatibility.Enabled)
                {
                    if (itemCount > byte.MaxValue)
                    {
                        ItemPlugin.Logger.LogError($"Over byte max value and IDPatcher is not present! Will not be adding {GUIDCombo} and following items");
                        return;
                    }
                }

                Configuration? configuration = Synchronizer.GetConfigOfGUID(GUIDCombo);
                if (configuration == null)
                {
                    ItemPlugin.Logger.LogError($"Could not find configuration of GUID combo {GUIDCombo}");
                    continue;
                }
                
                ItemList.Add(TemporaryItemList[index]);
                ItemPlugin.Logger.LogDebug($"Assigned ID {itemCount} to item {ItemList[index].displayName}");
                IDPatcherCompatibility.SetItemID(ItemList[index], itemCount);
                configuration.SetValue(itemCount);
            }

            AddAllItemsToDatabase();
        }

        internal static void OnLobbyLeft()
        {
            ItemList = new List<Item>(); // Wipes prev. ItemList
            ItemDatabase itemDatabase = SingletonAsset<ItemDatabase>.Instance;

            bool compare(int a, int b)
            {
                return a <= b;
            }

            itemDatabase.Objects = itemDatabase.Objects.Where(i => IDPatcherCompatibility.GetItemID(i) <= initialItemCount).ToArray();
            //itemDatabase.Objects = itemDatabase.Objects.Where(i => IDPatcherCompatibility.CompareIDToNumber(i, initialItemCount, compare)).ToArray();
        }

        /// <summary>
        /// Gets an item from TemporaryItemList from a GUID + DisplayName combination
        /// </summary>
        /// <param name="guidCombination">Combination of an Item's GUID and its DisplayName</param>
        /// <returns>If the Item is found it will be returned, else returns null</returns>
        public static Item? GetItemFromGUIDCombination(string guidCombination)
        {
            return TemporaryItemList.FirstOrDefault(i => i.modGuid + i.displayName == guidCombination);
        }

        /// <summary>
        /// Assigns an ID to your item and a globally unique identifier. Also gives you the option to choose where your item spawns.
        /// </summary>
        /// <param name="item">The item you want to assign</param>
        /// <param name="guid">An identifier for your item, usually to know what mod it came from. Is used as a prefix to the item's displayName | ex: MyMod.Skull (MyMod. is the GUID)</param>
        /// <param name="spawnableIn">List of the names of the scenes your item can spawn in.
        /// Defaults to a list only including "All", which will ignore any other strings and make it possible to spawn in all scenes.
        /// Remember to set item.spawnable if you wish your item to spawn to true as this does not ignore it</param>
        /// <param name="useInGame">If the item will be used, if false the item will never be created, only matters if player is the host. Defaults to true.</param>
        /// <returns>Configuration that was created from the item</returns>
        public static Configuration AssignItem(Item item, string guid, List<string>? spawnableIn, bool useInGame = true)
        {
            if (item.displayName.IsNullOrEmpty())
                ItemPlugin.Logger.LogError($"Item has null or empty displayName! Unless you have a specific GUID for the item this will cause problems!");

            if (spawnableIn == null)
                spawnableIn = new List<string> { "All" };

            if (item.name.IsNullOrEmpty())
            {
                item.name = guid + item.displayName;
                ItemPlugin.Logger.LogWarning($"Item name is null or empty, set it to {item.name}");
            }

            ItemPlugin.Logger.LogDebug($"Assigned item with GUID {guid} and GUID combo {guid + item.displayName}");

            item.modGuid = guid;
            item.useInGame = useInGame;
            item.spawnableIn = spawnableIn;

            TemporaryItemList.Add(item);

            Configuration configuration = new ConfigSync.Configuration(item.displayName, item.modGuid + item.displayName, 0);
            configuration.ConfigChanged += delegate
            {
                if (!MyceliumNetwork.InLobby || MyceliumNetwork.IsHost)
                    return;

                // "Just use the item parameter you already have 4head!" Well if I did do that then the variable would be in the heap which is cringe
                // Leaving the config in the heap is... *Fine*, ConfigSync would need to implement GUID in the action callbacks, so we could forego heap for stack
                Item? configItem = GetItemFromGUIDCombination(configuration.ConfigGUID); 
                if (configItem == null)
                {
                    ItemPlugin.Logger.LogError($"Could not get Item from GUID combo {configuration.ConfigGUID}");
                    return;
                }

                Item emptyItem = ScriptableObject.CreateInstance<Item>();
                while ((int)configuration.CurrentValue > ItemList.Count)
                {
                    ItemList.Add(emptyItem);
                }

                ItemList[(int)configuration.CurrentValue] = configItem;
                IDPatcherCompatibility.SetItemID(configItem, (int)configuration.CurrentValue);

                ItemDatabase database = SingletonAsset<ItemDatabase>.Instance;
                database.Objects = database.Objects.AddToArray(configItem);
            };
            return configuration;
        }

        #region CCCC Wrappers

        /// <summary>
        /// Wrapper for useInGame field.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="useInGame"></param>
        public static void SetUseInGame(Item item, bool useInGame)
        {
            item.useInGame = useInGame;
        }

        /// <summary>
        /// Wrapper for ModGUID field.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="guid"></param>
        public static void SetGUID(Item item, string guid)
        {
            item.modGuid = guid;
        }

        /// <summary>
        /// Wrapper for SpawnableIn field.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="spawnableIn"></param>
        public static void SetSpawnableIn(Item item, List<string> spawnableIn)
        {
            item.spawnableIn = spawnableIn;
        }

        /// <summary>
        /// Wrapper for ModGUID field.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetGUID(Item item)
        {
            return item.modGuid;
        }

        /// <summary>
        /// Wrapper for UseInGame field.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool GetUseInGame(Item item)
        {
            return item.useInGame;
        }

        /// <summary>
        /// Wrapper for SpawnableIn field.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<string> GetSpawnableIn(Item item)
        {
            return item.spawnableIn;
        }

        #endregion

        /*
        // I'm fairly sure it works, just don't know if I should include it.
        private static BepInPlugin? GetBepInPluginAttribute(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                object? attribute = type.GetCustomAttributes(true)
                    .FirstOrDefault(a => a.GetType().Name == "BepInPlugin");
                if (attribute != null)
                    return (BepInPlugin)attribute;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="assetBundle"></param>
        /// <returns>GUID which your items were assigned with</returns>
        public static string AssignAllItems(Assembly assembly, AssetBundle assetBundle)
        {

            string guid = "";
            BepInPlugin? attribute = GetBepInPluginAttribute(assembly);

            if (attribute != null)
            {
                guid = attribute.GUID.Replace(attribute.Name, "");
            }
            else
            {
                guid = assembly.FullName;
            }

            Item[]? items = assetBundle.LoadAllAssets<Item>();
            foreach (Item item in items)
            {
                AssignItem(item, guid);
            }

            return guid;
        }*/
    }
}
