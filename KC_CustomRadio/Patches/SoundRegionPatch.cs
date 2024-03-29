using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace Quriz.CustomRadio
{
    static class SoundRegionPatch
    {
        [HarmonyPatch(typeof(SoundRegion), "Update")]
        private static class Update
        {
            private static bool Prefix()
            {
                return false; // Skip original method
            }
        }
    }
}