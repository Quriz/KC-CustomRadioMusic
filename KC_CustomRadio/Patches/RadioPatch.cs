using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace Quriz.CustomRadio;

static class RadioPatch
{
    /// <summary>
    /// Add <see cref="CustomRadioController"/> to Radio and stop default music.
    /// </summary>
    [HarmonyPatch(typeof(Radio), "Start")]
    private static class Start
    {
        private static void Postfix(Radio __instance)
        {
            __instance.gameObject.AddComponent<CustomRadioController>();
            __instance.ToggleRadio(false);
        }
    }
    
    [HarmonyPatch(typeof(Radio), nameof(Radio.ToggleRadio), new []{ typeof(bool) })]
    private static class ToggleRadio
    {
        private static bool Prefix(Radio __instance, bool isOpen, ref GameObject ___onEffectGO)
        {
            // Set Radio.IsOpen
            var propertyIsOpen = typeof(Radio).GetProperty("IsOpen", BindingFlags.Instance | BindingFlags.NonPublic);
            if (propertyIsOpen != null)
            {
                propertyIsOpen.SetValue(__instance, isOpen);
            }
            
            // Enable/disable music particle effect
            ___onEffectGO.SetActive(isOpen);
            
            return false; // Skip original method
        }
    }
    
    [HarmonyPatch(typeof(Radio), nameof(Radio.SwitchNextChannel))]
    private static class SwitchNextChannel
    {
        private static bool Prefix(Radio __instance)
        {
            __instance.GetCustomRadioController().NextSong();
            
            return false; // Skip original method
        }
    }
    
    [HarmonyPatch(typeof(RadioOpenSpeciality), nameof(RadioOpenSpeciality.DoItemSpeciality))]
    private static class RadioOpenSpeciality_DoItemSpeciality
    {
        private static bool Prefix(RadioOpenSpeciality __instance, ref Radio ___radio)
        {
            if (__instance.isCalledByMe)
                ___radio.GetCustomRadioController().OpenUI();
                
            return false; // Skip original method
        }
    }
        
    private static CustomRadioController GetCustomRadioController(this Radio radio) 
        => radio.GetComponent<CustomRadioController>();
}