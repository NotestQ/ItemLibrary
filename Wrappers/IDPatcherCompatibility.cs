using System;
using System.Linq;
using System.Runtime.CompilerServices;
using ItemIDPlugin;
using MonoMod.Cil;
using Zorro.Core;

namespace ItemLibrary.Wrappers
{
    public static class IDPatcherCompatibility
    {
        private static bool? _enabled;

        public static bool Enabled
        {
            get
            {
                if (_enabled == null)
                {
                    _enabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("Notest.ItemIDPlugin");
                }
                return (bool)_enabled;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static int GetItemID(Item item)
        {
            if (Enabled)
                return ItemHelper.GetItemID(item);

            return GetID(item);
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public static void SetItemID(Item item, int id)
        {
            if (Enabled)
            {
                ItemHelper.SetItemID(item, id);
                return;
            }

            SetID(item, (byte)id);
        }

        private static void SetID(Item item, byte num)
        {
            item.id = num;
        }

        private static byte GetID(Item item)
        {
            return item.id;
        }

        public static bool CompareIDToNumber(Item item, int num, Func<int, int, bool> compareMethod)
        {
            if (Enabled)
                return compareMethod(GetItemID(item), num);

            return compareMethod((int)GetID(item), num); // DO NOT USE .id DIRECTLY. C# DOES NOT LIKE THAT.
        }
    }
}