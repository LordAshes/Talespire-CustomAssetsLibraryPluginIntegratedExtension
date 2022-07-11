using BepInEx;
using Bounce.Unmanaged;
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

        #region Post Spawn Router

        public IEnumerator PostSpawnHandlerRouter(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Mini "+creatureData.CreatureId+" Placed ("+databaseData.Name+")"); }
            yield return new WaitForSeconds(0.1f);
            if (creatureData.Link == "SpawnMode=CodeSpawn")
            {
                // Supress Post Spawn Callback
                creatureData.Link = "";
            }
            else
            {
                // Trigger Post Spawn Callback
                Dictionary<string, string> tags = new Dictionary<string, string>();
                // Build Tags Dictionary
                foreach (string item in ((AssetDb.DbEntry)inputs[1]).Tags)
                {
                    if(item.Contains(":")) { tags.Add(item.Substring(0,item.IndexOf(":")).ToUpper(), item.Substring(item.IndexOf(":")+1)); } else { tags.Add(item, item); }
                }
                if (!tags.ContainsKey("KIND")) { tags.Add("KIND", "Creature"); }
                string kind = Helpers.ModifyKindBasedOnModifier(tags["KIND"]);
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Asset is kind " + tags["KIND"] + " being treated as " + kind); }
                // Trigger Remote Post Spawn Callback
                CustomAssetsLibraryPluginIntegratedExtention._self.StartCoroutine("PostSpawn" + kind + "Handler", new object[] { creatureData, databaseData, tags});
                if (creatureData.Link!=null && creatureData.Link.Trim() != "") 
                {
                    // Trigger Remote Post Spawn Callback
                    try
                    {
                        // AssetDataPlugin.SendInfo(creatureData.Link, DateTime.UtcNow);
                    }
                    catch(Exception)
                    {
                        Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: AssetDataPlugin Not Available. Notification Ignored"); 
                    }
                }
            }
        }

        #endregion

        #region Post Spawn Handlers

        public IEnumerator PostSpawnAudioHandler(object[] inputs)
        {
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Audio"); }
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            RequestHandler.Audio(creatureData.CreatureId, 1);
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnAuraHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Aura "+creatureData.CreatureId); }
            CreatureBoardAsset asset = null;
            CreaturePresenter.TryGetAsset(creatureData.CreatureId, out asset);
            if (asset != null)
            {
                // Apply custom shader
                Helpers.AdjustShader(asset, asset.IsExplicitlyHidden);
                // Attach aura to selected mini
                GameObject target = Utility.GetAssetLoader(LocalClient.SelectedCreatureId);
                GameObject aura = Utility.GetAssetLoader(creatureData.CreatureId);
                if (target != null && aura != null)
                {
                    aura.transform.position = target.transform.position;
                    aura.transform.rotation = target.transform.rotation;
                    aura.transform.SetParent(target.transform);
                }
                else
                {
                    SystemMessage.DisplayInfoText("Unable To Attach Aura");
                }
            }
            yield return new WaitForSeconds(0.1f);

        }

        public IEnumerator PostSpawnCreatureHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Creature " + creatureData.CreatureId); }
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnEffectHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Effect " + creatureData.CreatureId); }
            CreatureBoardAsset asset = null;
            CreaturePresenter.TryGetAsset(creatureData.CreatureId, out asset);
            if (asset != null)
            {
                // Apply custom shader
                Helpers.AdjustShader(asset, asset.IsExplicitlyHidden);
            }
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnEncounterHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Encounter " + creatureData.CreatureId); }
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnFilterHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Filter " + creatureData.CreatureId); }
            CreatureBoardAsset asset = null;
            CreaturePresenter.TryGetAsset(creatureData.CreatureId, out asset);
            if (asset != null)
            {
                // Apply custom shader
                Helpers.AdjustShader(asset, asset.IsExplicitlyHidden);
            }
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnPropHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Prop " + creatureData.CreatureId); }
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnSlabHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Slab " + creatureData.CreatureId); }
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnTileHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Tile " + creatureData.CreatureId); }
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnTransformHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Transform " + creatureData.CreatureId); }
            CreatureBoardAsset assetOrig;
            CreaturePresenter.TryGetAsset(LocalClient.SelectedCreatureId, out assetOrig);
            CreatureBoardAsset assetNew;
            CreaturePresenter.TryGetAsset(creatureData.CreatureId, out assetNew);
            GameObject go = Utility.GetAssetLoader(creatureData.CreatureId);
            if(assetOrig!=null && assetNew!=null)
            {
                Vector3 pos = assetOrig.CorrectPos;
                Quaternion rot = assetOrig.CorrectRotation;
                float height = assetOrig.CorrectHeight;
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Removing Old Asset"); }
                assetOrig.RequestDelete();
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Adjusting New Asset"); }
                assetNew.CorrectPos = pos;
                assetNew.CorrectRotation = rot;
                assetNew.CorrectHeight = height;
            }
            yield return new WaitForSeconds(0.1f);
        }

        #endregion
    }
}
