using System;
using System.Collections.Generic;
using System.Text;
using HarmonyLib;

namespace ItemLibrary.Patches
{
    [HarmonyPatch(typeof(ItemInstanceData))]
    internal class ItemInstanceDataPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemInstanceData.GetEntryIdentifier))]
        private static bool GetEntryIdentifierPatch(ref dynamic __result, Type type)
        {
            int index = EntryHandler.EntryList!.FindIndex(e => type == e.GetType());
            if (index < 0)
                return true;

            __result = (byte)(index + EntryHandler.InitialEntries + 1); // Expects 1 based index, so we need to add 1 since index is 0 based

            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemInstanceData.GetEntryType))]
        private static bool GetEntryTypePatch(ref ItemDataEntry __result, ref byte identifier) // TODO: Change to int
        {
            int id = identifier - EntryHandler.InitialEntries - 1; // identifier is 1 based, but we need 0 based, so we remove 1
            if (id < 0)
                return true;

            ItemDataEntry entry = EntryHandler.EntryList![id];
            __result = (ItemDataEntry)Activator.CreateInstance(entry.GetType());
            
            return false;
        }
    }
}
