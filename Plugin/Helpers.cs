using BepInEx;
using Bounce.Unmanaged;
using DataModel;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

            public static void AdjustShader(CreatureBoardAsset __instance, bool hidden)
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Adjusting Show/Hide Shader"); }
                // Get Asset Info
                CreatureBoardAsset asset = null;
                CreaturePresenter.TryGetAsset(__instance.CreatureId, out asset);
                if (asset == null) { return; }
                AssetDb.DbEntry assetInfo = null;
                AssetDb.TryGetIndexData(asset.BoardAssetId, out assetInfo);
                if (assetInfo == null) { return; }
                if (assetInfo.Tags == null) { return; }
                // Build Tags Dictionary
                Dictionary<string, string> tags = new Dictionary<string, string>();
                foreach (string item in assetInfo.Tags)
                {
                    if (item.Contains(":")) { tags.Add(item.Substring(0, item.IndexOf(":")).ToUpper(), item.Substring(item.IndexOf(":") + 1)); } else { tags.Add(item, item); }
                }
                if (!tags.ContainsKey("KIND")) { tags.Add("KIND", "Creature"); }
                // Correct Sharder If Necessary
                if ((assetInfo != null) && ("|Aura|Effect|".Contains(tags["KIND"])))
                {
                    switch (hidden)
                    {
                        case false:
                            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Asset Is Visible: Applying Asset Bundle Shader"); }
                            _self.StartCoroutine(_self.ApplyShader(Utility.GetAssetLoader(__instance.CreatureId), tags, ""));
                            break;
                        case true:
                            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Asset Is Hidden: Applying Taleweaver/CreatureShader Shader"); }
                            _self.StartCoroutine(_self.ApplyShader(Utility.GetAssetLoader(__instance.CreatureId), tags, "Taleweaver/CreatureShader"));
                            break;
                    }
                }
                else
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Non-Aura & Non-Effect. No adjustment necessary."); }
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

        private IEnumerator ApplyShader(GameObject asset, Dictionary<string, string> tags, string shaderName = "")
        {
            string applyShaderName = "";
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Applying '"+(shaderName!=""?shaderName:"AssetBundle")+"' Shader"); }
            if (asset != null)
            {
                if (tags.ContainsKey("PREFAB"))
                {
                    yield return new WaitForSeconds(0.1f);
                    Renderer[] rends = asset.GetComponentsInChildren<Renderer>();
                    for(int r=0; r< asset.GetComponentsInChildren<Renderer>().Length; r++)
                    {
                        if (!rends[r].material.shader.name.StartsWith("Taleweaver"))
                        {
                            if (!Helpers.shaderNames.ContainsKey(tags["PREFAB"] + ":" + rends[r].name + ":" + rends[r].material.name))
                            {
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Saving prefab '" + tags["PREFAB"] + "' renderer '" + rends[r].name + "' material '" + rends[r].material.name + "' shader '" + rends[r].material.shader.name + "'"); }
                                Helpers.shaderNames.Add(tags["PREFAB"] + ":" + rends[r].name + ":" + rends[r].material.name, rends[r].material.shader.name);
                            }
                            if (!Helpers.shaders.ContainsKey(rends[r].material.shader.name))
                            {
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Saving Shader '" + rends[r].material.shader.name + "' Reference"); }
                                Helpers.shaders.Add(rends[r].material.shader.name, rends[r].material.shader);
                            }
                        }
                        applyShaderName = (shaderName!="") ? shaderName : (Helpers.shaderNames.ContainsKey(tags["PREFAB"] + ":" + rends[r].name + ":" + rends[r].material.name) ? Helpers.shaderNames[tags["PREFAB"] + ":" + rends[r].name + ":" + rends[r].material.name] : "Standard");
                        if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Changing mini '" + tags["PREFAB"] + "' renderer '" + rends[r].name + "' material '" + rends[r].material.name + "' from shader '" + rends[r].material.shader.name + "' to shader '" + applyShaderName + "'"); }
                        if (!Helpers.shaders.ContainsKey(applyShaderName))
                        {
                            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Don't have shader '" + rends[r].material.shader.name + "' saved. Trying to get a reference."); }
                            Helpers.shaders.Add(applyShaderName, Shader.Find(applyShaderName));
                        }
                        asset.GetComponentsInChildren<Renderer>()[r].material.shader = Helpers.shaders[applyShaderName];
                        if(applyShaderName!= rends[r].material.shader.name)
                        {
                            Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Unable to apply shader '" + applyShaderName + "'");
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Asset Missing 'PREFAB' tag");
                }
            }
            else
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Asset Is Null. Hide/Show Before Spawn?"); }
            }
        }
    }
}
