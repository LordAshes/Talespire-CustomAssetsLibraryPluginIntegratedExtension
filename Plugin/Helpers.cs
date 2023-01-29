using BepInEx;
using Bounce.TaleSpire.AssetManagement;
using Bounce.Unmanaged;
using DataModel;
using LegacyDataModel.Beta;
using LegacyDataModel.V4;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Mathematics;
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
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Preventing Asset Spawn"); }
                if (SingletonBehaviour<BoardToolManager>.HasInstance)
                {
                    SingletonBehaviour<BoardToolManager>.Instance.SwitchToTool<DefaultBoardTool>(BoardToolManager.Type.Normal);
                }
            }

            public static CreatureGuid SpawnCreature(CreatureDataV2 creatureData)
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Creating Mini Of Type " + creatureData.BoardAssetIds[0] + " Which " + (creatureData.ExplicitlyHidden ? "Is" : "Is Not") + " Hidden"); }

                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra)
                {
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Creature Spawn: Alias = " + creatureData.Alias);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Creature Spawn: CreatureId = " + creatureData.CreatureId);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Creature Spawn: BoardAssetIds = " + String.Join(",", creatureData.BoardAssetIds) + " (Active " + Convert.ToString(creatureData.GetActiveBoardAssetId()) + ")");
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Creature Spawn: ActiveMorphIndex = " + Convert.ToString(creatureData.ActiveMorphIndex));
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Creature Spawn: ExplicitlyHidden = " + creatureData.ExplicitlyHidden);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Creature Spawn: Flying = " + creatureData.Flying);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Creature Spawn: Link = " + creatureData.Link);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Creature Spawn: Position = " + creatureData.Position.x + "," + creatureData.Position.y + "," + creatureData.Position.z);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Creature Spawn: Rotation = " + creatureData.Rotation.ToEulerDegrees().x + "," + creatureData.Rotation.ToEulerDegrees().y + "," + creatureData.Rotation.ToEulerDegrees().z);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Creature Spawn: UniqueId = " + creatureData.UniqueId);
                }

                float3 position = new Unity.Mathematics.float3(creatureData.Position.x, creatureData.Position.y, creatureData.Position.z);
                Quaternion rotation = Quaternion.Euler(new Vector3(creatureData.Rotation.x.ToDegrees(), creatureData.Rotation.y.ToDegrees(), creatureData.Rotation.z.ToDegrees()));

                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Executing Spawn"); }

                if(spawnCreature==null)
                {
                    foreach (MethodInfo mi in typeof(CreatureManager).GetRuntimeMethods().ToArray())
                    {
                        if (mi.Name == "AddOrRequestAddCreature") { spawnCreature = mi; break; }
                    }
                }

                SpawnCreatureResult spawnResult = (SpawnCreatureResult)spawnCreature.Invoke(null, new object[] { creatureData, new PlayerGuid[] { LocalPlayer.Id }, true, true });

                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Spawn Result = "+spawnResult.ToString()); }

                if (spawnResult == SpawnCreatureResult.Success)
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Adding To History"); }

                    BuildingBoardTool.RecordInBuildHistory(creatureData.GetActiveBoardAssetId().Value);
                }

                return creatureData.CreatureId;
            }

            public static void SpawnCreatureByNGuid(NGuid nguid, string alias = "")
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Spawning Creature By NGuid"); }
                CreatureBoardAsset asset;
                CreaturePresenter.TryGetAsset(LocalClient.SelectedCreatureId, out asset);
                Vector3 spawnPos = (asset != null) ? asset.CorrectPos : Vector3.zero;
                Quaternion spawnRot = (asset != null) ? asset.CorrectRotation : Quaternion.Euler(Vector3.zero);
                CreatureDataV2 creatureData = new CreatureDataV2(new BoardAssetGuid(nguid));
                creatureData.Alias = alias;
                creatureData.CreatureId = new CreatureGuid(new Bounce.Unmanaged.NGuid(System.Guid.NewGuid()));
                creatureData.PackedScales = new CreatureDataV2.ScalesPack(0.1875f);
                creatureData.Position = spawnPos;
                creatureData.Rotation = Bounce.Mathematics.bam3.FromEulerDegrees(spawnRot.eulerAngles);
                creatureData.ExplicitlyHidden = false;
                creatureData.Flying = false;
                Helpers.SpawnCreature(creatureData);
            }

            public static IEnumerator ShowHide(CreatureBoardAsset __instance, float pause = 0.1f)
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Helpers: Hide/Show Update"); }
                yield return new WaitForSeconds(pause);
                if (__instance != null)
                {
                    bool visible = true;
                    if (!__instance.IsVisible) { visible = false; }
                    if (__instance.IsExplicitlyHidden) { visible = false; }
                    if (__instance.ShaderStateRef.State.IsCreatureHiddenByVolume) { visible = false; }
                    if (!__instance.ShaderStateRef.State.InActiveLineOfSight && !LocalClient.IsInGmMode) { visible = false; }
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: "+Utility.GetCreatureName(__instance.Name)+" Show = " + visible+" (Hide: "+ __instance.IsExplicitlyHidden+", HideVolume: "+ __instance.ShaderStateRef.State.IsCreatureHiddenByVolume+", HeightBar: "+!__instance.IsVisible+", LOS: "+ __instance.ShaderStateRef.State.InActiveLineOfSight+")"); }
                    Renderer[] ren = __instance.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in ren)
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
                if (Input.GetKey(KeyCode.RightControl)) { return "Creature"; }
                if (Input.GetKey(KeyCode.RightShift)) { return "Transform"; }
                if (Input.GetKey(KeyCode.RightAlt)) { return "Audio"; }
                if (Input.GetKey(KeyCode.LeftShift)) { return "Effect"; }
                if (Input.GetKey(KeyCode.LeftControl)) { return "Aura"; }
                if (Input.GetKey(KeyCode.LeftAlt)) { return "Filter"; }
                return kind;
            }

            public static Dictionary<string, string> GetAssetTags(BoardAssetGuid nguid)
            {
                AssetDb.DbEntry info;
                AssetDb.TryGetIndexData(nguid, out info);
                Dictionary<string, string> tags = new Dictionary<string, string>();
                foreach (string tag in info.Tags)
                {
                    if (tag.Contains(":"))
                    {
                        tags.Add(tag.Substring(0, tag.IndexOf(":")), tag.Substring(tag.IndexOf(":") + 1));
                    }
                    else
                    {
                        tags.Add(tag, tag);
                    }
                }
                return tags;
            }

            public static Dictionary<string, object> GetAssetInfo(BoardAssetGuid nguid)
            {
                string prefabName = GetAssetTags(nguid)["Prefab"];
                Dictionary<string,object> info = null;
                foreach (AssetBundle ab in AssetBundle.GetAllLoadedAssetBundles())
                {
                    if (ab.name == prefabName)
                    {
                        string json = ab.LoadAsset<TextAsset>("Info.txt").text;
                        info = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                    }
                }
                return info;
            }
        }
    }
}
