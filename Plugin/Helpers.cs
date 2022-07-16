using BepInEx;
using Bounce.Unmanaged;
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

            public static void ShowHide(CreatureBoardAsset __instance, bool hidden)
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: "+(hidden?"Hiding":"Showing Asset")); }
                // Get Asset Info
                CreatureBoardAsset asset = null;
                CreaturePresenter.TryGetAsset(__instance.CreatureId, out asset);
                if (asset == null) { return; }
                AssetDb.DbEntry assetInfo = null;
                AssetDb.TryGetIndexData(asset.BoardAssetId, out assetInfo);
                if (assetInfo == null) { return; }
                Renderer[] renderers = asset.GetComponentsInChildren<Renderer>();
                foreach(Renderer renderer in renderers)
                {
                    if(!renderer.material.shader.name.StartsWith("Taleweaver"))
                    {
                        renderer.enabled = !hidden;
                    }
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
