using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ItemLibrary.Patches
{
    [HarmonyPatch(typeof(RoundArtifactSpawner))]
    internal class RoundArtifactSpawnerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(RoundArtifactSpawner.Awake))]
        private static void AwakePatch(RoundArtifactSpawner __instance)
        {
            string scene = __instance.gameObject.scene.name;
            ItemPlugin.Logger.LogDebug($"Patched round artifact spawner for scene {scene}");
            __instance.possibleSpawns = __instance.possibleSpawns.AddRangeToArray(
                ItemHandler.ItemList!.Where(i => i.spawnable 
                                                && (i.spawnableIn.Contains(scene) || i.spawnableIn.Contains("All"))).ToArray());

            foreach (Item item in __instance.possibleSpawns)
            {
                ItemPlugin.Logger.LogDebug(item.name);
            }
        }
    }
}
