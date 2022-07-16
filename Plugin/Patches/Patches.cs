using BepInEx;
using Bounce.Singletons;
using Bounce.Unmanaged;
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
        private static NGuid heightBarStepId = NGuid.Empty;
        private static float heightBarLastPosition = 0.0f;

        private static StringComparer comparer = StringComparer.OrdinalIgnoreCase;

        public static partial class Patches
        {
            public static List<CreatureBoardAsset> spawnList = new List<CreatureBoardAsset>();

            [HarmonyPatch(typeof(UI_AssetBrowserSlotItem), "Spawn")]
            public class PatchSpawn
            {
                public static bool Prefix(UI_AssetBrowserSlotItem __instance, NGuid ____nGuid)
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Library Selection Made (Asset Id " + ____nGuid + ")"); }
                    return CustomAssetsLibraryPluginIntegratedExtention.PreSpawnHandlerRouter(____nGuid, AssetDb.GetIndexData(____nGuid));
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

            [HarmonyPatch(typeof(CreatureManager), "SetCreatureExplicitHideState")]
            public static class PatchSetCreatureExplicitHideState
            {
                public static bool Prefix(CreatureGuid creatureGuid, bool hideState)
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Sync Shader Due To SetCreatureExplicitHideState"); }
                    CreatureBoardAsset asset;
                    CreaturePresenter.TryGetAsset(creatureGuid, out asset);
                    if (asset != null)
                    {
                        Helpers.ShowHide(asset, hideState);
                    }
                    return true;
                }
            }

            [HarmonyPatch(typeof(HeightHidePlane), "SetHeight")]
            public static class PatchSetHeight
            {
                public static bool Prefix(float height, bool transition = false)
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Height=" + height + ", LastHeight=" + heightBarLastPosition); }
                    if (height != heightBarLastPosition)
                    {
                        heightBarLastPosition = height;
                        if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Height Bar Moving"); }
                        _self.StartCoroutine(DelayedShowHide(height));
                    }
                    return true;
                }
            }

            public static IEnumerator DelayedShowHide(float height)
            {
                NGuid refGuid = new NGuid(System.Guid.NewGuid());
                heightBarStepId = refGuid;
                yield return new WaitForSeconds(CustomAssetsLibraryPluginIntegratedExtention.heightBarShaderDelay.Value);
                if (heightBarStepId == refGuid)
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Syncing Show/Hide"); }
                    foreach (CreatureBoardAsset asset in CreaturePresenter.AllCreatureAssets)
                    {
                        Helpers.ShowHide(asset, asset.CorrectHeight > height);
                    }
                }
                else
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Still Moving. Doing Nothing."); }
                }
            }
        }
    }
}
