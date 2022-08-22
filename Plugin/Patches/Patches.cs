using BepInEx;
using Bounce.Singletons;
using Bounce.Unmanaged;
using DataModel;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace LordAshes
{
    public partial class CustomAssetsLibraryPluginIntegratedExtention : BaseUnityPlugin
    {
        public static partial class Patches
        {
            public static List<CreatureBoardAsset> spawnList = new List<CreatureBoardAsset>();

            [HarmonyPatch(typeof(UI_AssetBrowserSlotItem), "Spawn")]
            public class PatchSpawn
            {
                public static bool Prefix(UI_AssetBrowserSlotItem __instance, NGuid ____nGuid)
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Library Selection Made (Asset Id " + ____nGuid + ")"); }
                    return CustomAssetsLibraryPluginIntegratedExtention.PreSpawnHandlerRouter(____nGuid, AssetDb.GetIndexData(new BoardAssetGuid(____nGuid)));
                }
            }

            [HarmonyPatch(typeof(CreatureBoardAsset), "OnBaseLoaded")]
            public static class PatcheOnBaseLoaded
            {
                public static bool Prefix(CreatureBoardAsset __instance)
                {
                    string nameBlock = (__instance.Name != null) ? __instance.Name : ((__instance.name != null) ? __instance.name : "(Unknown)");
                    nameBlock = Utility.GetCreatureName(nameBlock);
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Board Placement Of " + nameBlock); }
                    spawnList.Add(__instance);
                    return true;
                }
            }

            [HarmonyPatch(typeof(CreatureBoardAsset), "OnVisibilityChanged")]
            public static class PatchOnVisibilityChanged
            {
                public static void Postfix(ref CreatureBoardAsset __instance)
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Synchronizing The Hide State For Non-TS Shader Content On Creature '"+__instance.Name+"'"); }
                    CustomAssetsLibraryPluginIntegratedExtention._self.StartCoroutine(Helpers.ShowHide(__instance));
                }
            }
        }
    }
}
