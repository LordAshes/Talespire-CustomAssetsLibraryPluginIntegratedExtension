using BepInEx;
using Bounce.Unmanaged;
using DataModel;
using Newtonsoft.Json;
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

                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra)
                {
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Creature Spawn: Alias = " + creatureData.Alias);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Creature Spawn: CreatureId = " + creatureData.CreatureId);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Creature Spawn: BoardAssetIds = " + String.Join(",", creatureData.BoardAssetIds) + " (Active " + Convert.ToString(creatureData.GetActiveBoardAssetId()) + ")");
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Creature Spawn: ActiveMorphIndex = " + Convert.ToString(creatureData.ActiveMorphIndex));
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Creature Spawn: ExplicitlyHidden = " + creatureData.ExplicitlyHidden);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Creature Spawn: Flying = " + creatureData.Flying);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Creature Spawn: Link = " + creatureData.Link);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Creature Spawn: Position = " + creatureData.Position.x + "," + creatureData.Position.y + "," + creatureData.Position.z);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Creature Spawn: Rotation = " + creatureData.Rotation.ToEulerDegrees().x + "," + creatureData.Rotation.ToEulerDegrees().y + "," + creatureData.Rotation.ToEulerDegrees().z);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Creature Spawn: UniqueId = " + creatureData.UniqueId);
                }

                Quaternion q = Quaternion.Euler(new Vector3(creatureData.Rotation.x.ToDegrees(), creatureData.Rotation.y.ToDegrees(), creatureData.Rotation.z.ToDegrees()));

                Debug.Log("Custom Assets Library Plugin Integrated Extension: SpawnResult=" + Convert.ToString(CreatureManager.TryCreateAndAddNewCreature(creatureData, creatureData.Position, new Unity.Mathematics.quaternion(q.x, q.y, q.z, q.w), creatureData.Flying, creatureData.ExplicitlyHidden, false)));

                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Registering mini for saving"); }
                BuildingBoardTool.RecordInBuildHistory(creatureData.GetActiveBoardAssetId().Value);
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Spawn complete"); }

                return creatureData.CreatureId;
            }

            public static void SpawnCreatureByNGuid(NGuid nguid)
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Pre Spawn Handler: Spawning Creature By NGuid"); }
                CreatureBoardAsset asset;
                CreaturePresenter.TryGetAsset(LocalClient.SelectedCreatureId, out asset);
                Vector3 spawnPos = (asset != null) ? asset.CorrectPos : Vector3.zero;
                Quaternion spawnRot = (asset != null) ? asset.CorrectRotation : Quaternion.Euler(Vector3.zero);
                Helpers.SpawnCreature(new CreatureDataV2()
                {
                    CreatureId = new CreatureGuid(new Bounce.Unmanaged.NGuid(System.Guid.NewGuid())),
                    BoardAssetIds = new BoardAssetGuid[] { new BoardAssetGuid(nguid) },
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
                    if (!__instance.IsVisible) { visible = false; }
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Show = " + visible+" (Hide: "+ __instance.IsExplicitlyHidden+", HideVolume: "+ shader.State.IsCreatureHiddenByVolume+", HeightBar: "+!__instance.IsVisible+")"); }
                    MeshRenderer[] mr = __instance.GetComponentsInChildren<MeshRenderer>();
                    foreach (MeshRenderer renderer in mr)
                    {
                        if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Creature '" + __instance.Name + "' has " + renderer.GetType().ToString() + " '" + renderer.name + "' material '" + renderer.material.name + "' shader '" + renderer.material.shader.name + "'"); }
                        // if (!renderer.material.shader.name.StartsWith("Taleweaver"))
                        // {
                            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Setting Creature '" + __instance.Name + "' has " + renderer.GetType().ToString() + " '" + renderer.name + "' Enabled = " + visible); }
                            renderer.enabled = visible;
                        // }
                    }
                    SkinnedMeshRenderer[] smr = __instance.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach (SkinnedMeshRenderer renderer in smr)
                    {
                        if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Creature '" + __instance.Name + "' has " + renderer.GetType().ToString() + " '" + renderer.name + "' material '" + renderer.material.name + "' shader '" + renderer.material.shader.name + "'"); }
                        // if (!renderer.material.shader.name.StartsWith("Taleweaver"))
                        // {
                            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Setting Creature '" + __instance.Name + "' has " + renderer.GetType().ToString() + " '" + renderer.name + "' Enabled = " + visible); }
                            renderer.enabled = visible;
                        //}
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
