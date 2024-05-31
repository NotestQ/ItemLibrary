using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ConfigSync;
using HarmonyLib;
using ItemLibrary.Wrappers;
using MyceliumNetworking;
using Zorro.Core;
using Zorro.Core.Serizalization;

namespace ItemLibrary
{
    public static class EntryHandler
    {
        public static int InitialEntries;
        public static List<ItemDataEntry>? EntryList = new List<ItemDataEntry>();
        public static List<ItemDataEntry> TemporaryEntryList = new List<ItemDataEntry>();

        internal static void OnLobbyEntered()
        {
            if (!MyceliumNetwork.IsHost)
                return;

            for (int index = 0; index < TemporaryEntryList.Count; index++)
            {

                string name = TemporaryEntryList[index].GetType().Name;
                string GUID = $"Entry_{name}";

                Configuration? configuration = Synchronizer.GetConfigOfGUID(GUID);

                if (configuration == null)
                {
                    ItemPlugin.Logger.LogError($"Could not find configuration of GUID {GUID}");
                    continue;
                }
                
                ItemPlugin.Logger.LogDebug($"Assigned entry ID {InitialEntries + index} to entry {name}");
                configuration.SetValue(InitialEntries + index);
            }
        }

        internal static void OnLobbyLeft()
        {
            EntryList = new List<ItemDataEntry>();
        }

        public static void AssignEntry(ItemDataEntry entry)
        {
            ItemPlugin.Logger.LogWarning(entry.GetType().Name);
            TemporaryEntryList.Add(entry);

            Configuration configuration = new Configuration(entry.GetType().Name, $"Entry_{entry.GetType().Name}", 0);
            configuration.ConfigChanged += delegate
            {
                if (!MyceliumNetwork.InLobby || MyceliumNetwork.IsHost)
                    return;

                ItemDataEntry? configEntry = TemporaryEntryList.FirstOrDefault(e =>
                    $"Entry_{e.GetType().Name}" == configuration.ConfigGUID);

                if (configEntry == null)
                {
                    ItemPlugin.Logger.LogError($"Could not get Entry from GUID {configuration.ConfigGUID}");
                    return;
                }

                while ((int)configuration.CurrentValue > EntryList.Count)
                {
                    EntryList.Add(new EmptyEntry());
                }

                EntryList[(int)configuration.CurrentValue] = configEntry;
            };
        }

        internal static void GetEntryCount()
        {
            if (ShopUtilsCompatibility.Enabled)
                InitialEntries = ShopUtilsCompatibility.ShopUtilsRegisteredEntries;
            ItemPlugin.Logger.LogWarning(InitialEntries);
            Type[] types = Assembly.GetAssembly(typeof(ItemDataEntry)).GetTypes();
            foreach (Type type in types)
            {
                if (type.IsSubclassOf(typeof(ItemDataEntry)))
                    InitialEntries += 1;
            }
        }
    }

    internal class EmptyEntry : ItemDataEntry
    {
        public override void Deserialize(BinaryDeserializer binaryDeserializer)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(BinarySerializer binarySerializer)
        {
            throw new NotImplementedException();
        }
    }
}
