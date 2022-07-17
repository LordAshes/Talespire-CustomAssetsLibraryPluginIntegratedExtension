using BepInEx;
using Bounce.Unmanaged;
using DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LordAshes
{
    public partial class CustomAssetsLibraryPluginIntegratedExtention : BaseUnityPlugin
    {
        public static class Helpers
        {
            public static Dictionary<string, string> shaderNames = new Dictionary<string, string>();
            public static Dictionary<string, Shader> shaders = new Dictionary<string, Shader>();

            public static void SpawnPrevent()
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Preventing Asset Spawn"); }
                if (SingletonBehaviour<BoardToolManager>.HasInstance)
                {
                    SingletonBehaviour<BoardToolManager>.Instance.SwitchToTool<DefaultBoardTool>(BoardToolManager.Type.Normal);
                }
            }

            public static CreatureGuid SpawnCreature(CreatureDataV2 creatureData)
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Creating Mini Of Type " + creatureData.BoardAssetIds[0] + " Which " + (creatureData.ExplicitlyHidden ? "Is" : "Is Not") + " Hidden"); }

                Quaternion q = Quaternion.Euler(new Vector3(creatureData.Rotation.x.ToDegrees(), creatureData.Rotation.y.ToDegrees(), creatureData.Rotation.z.ToDegrees()));

                Debug.Log("Custom Assets Library Plugin Integrated Extension: SpawnResult=" + Convert.ToString(CreatureManager.TryCreateAndAddNewCreature(creatureData, creatureData.Position, new Unity.Mathematics.quaternion(q.x, q.y, q.z, q.w), creatureData.Flying, creatureData.ExplicitlyHidden, false)));

                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Registering mini for saving"); }
                BuildingBoardTool.RecordInBuildHistory(creatureData.GetActiveBoardAssetId());

                return creatureData.CreatureId;
            }

            public static IEnumerator SpawnCreatureByNGuid(NGuid nguid)
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Pre Spawn Handler: Spawning Creature By NGuid"); }
                yield return new WaitForSeconds(0.1f);
                CreatureBoardAsset asset;
                CreaturePresenter.TryGetAsset(LocalClient.SelectedCreatureId, out asset);
                Vector3 spawnPos = (asset != null) ? asset.CorrectPos : Vector3.zero;
                Quaternion spawnRot = (asset != null) ? asset.CorrectRotation : Quaternion.Euler(Vector3.zero);
                Helpers.SpawnCreature(new CreatureDataV2()
                {
                    CreatureId = new CreatureGuid(new Bounce.Unmanaged.NGuid(System.Guid.NewGuid())),
                    BoardAssetIds = new NGuid[] { nguid },
                    Position = spawnPos,
                    Rotation = Bounce.Mathematics.bam3.FromEulerDegrees(spawnRot.eulerAngles),
                    ExplicitlyHidden = false,
                    Flying = false
                });
            }

            public static IEnumerator ShowHide(CreatureBoardAsset __instance, float pause = 0.1f)
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Hide/Show Update"); }
                yield return new WaitForSeconds(pause);
                if (__instance != null)
                {
                    bool visible = true;
                    ShaderStateRef shader = default(ShaderStateRef);
                    __instance.TryGetShaderState(out shader);
                    if (__instance.IsExplicitlyHidden) { visible = false; }
                    if (shader.State.IsCreatureHiddenByVolume) { visible = false; }
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: IsHidden = " + __instance.IsExplicitlyHidden + " And IsVisible = " + shader.State.IsCreatureHiddenByVolume + " -> Show = " + visible); }
                    Renderer[] renderers = __instance.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Creature '" + __instance.Name + "' has " + renderer.GetType().ToString() + " '" + renderer.name + "' material '" + renderer.material.name + "' shader '" + renderer.material.shader.name + "'"); }
                        if (!renderer.material.shader.name.StartsWith("Taleweaver"))
                        {
                            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Setting Creature '" + __instance.Name + "' has " + renderer.GetType().ToString() + " '" + renderer.name + "' Enabled = " + visible); }
                            renderer.enabled = visible;
                        }
                    }
                }
                else
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Instance In Null"); }
                }
            }

            public static string ModifyKindBasedOnModifier(string kind)
            {
                if (Input.GetKey(KeyCode.LeftShift)) { return "Creature"; }
                if (Input.GetKey(KeyCode.RightShift)) { return "Transform"; }
                if (Input.GetKey(KeyCode.LeftControl)) { return "Effect"; }
                if (Input.GetKey(KeyCode.RightControl)) { return "Aura"; }
                if (Input.GetKey(KeyCode.LeftAlt)) { return "Audio"; }
                if (Input.GetKey(KeyCode.RightAlt)) { return "Filter"; }
                return kind;
            }
        }
    }
}
