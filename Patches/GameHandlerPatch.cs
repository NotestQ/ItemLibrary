using HarmonyLib;
using ShopUtils;
using System;
using System.Collections.Generic;
using System.Text;
using Zorro.Core;

namespace ItemLibrary.Patches
{
    [HarmonyPatch(typeof(GameHandler))]
    internal static class GameHandlerPatches
    {
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Last)]
        [HarmonyAfter(["hyydsz-ShopUtils"])]
        [HarmonyPatch("Initialize")]
        private static void Initialize()
        {
            ItemHandler.initialItemCount = SingletonAsset<ItemDatabase>.Instance.Objects.Length - 1;
        }
    }
}
