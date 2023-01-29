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
            RequestHandler.ApplyAudio(creatureData.CreatureId, 1);
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnAuraHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Aura "+creatureData.CreatureId); }
            // Request aura binding to target
            AssetDataPlugin.SetInfo(LocalClient.SelectedCreatureId.ToString(), CustomAssetsLibraryPluginIntegratedExtention.Guid + ".aura", creatureData.CreatureId.ToString() + "@" + DateTime.UtcNow.ToString(), false);
            yield return new WaitForSeconds(0.1f);
        }

        public IEnumerator PostSpawnCreatureHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Creature " + creatureData.CreatureId); }
            yield return new WaitForSeconds(0.1f);
            CreatureBoardAsset asset = null;
            CreaturePresenter.TryGetAsset(creatureData.CreatureId, out asset);
            if (asset != null)
            {
                CustomAssetsLibraryPluginIntegratedExtention._self.StartCoroutine(Helpers.ShowHide(asset, CustomAssetsLibraryPluginIntegratedExtention.showHideUpdateDelay.Value));
            }
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Looking For Variants"); }
            if (tags.ContainsKey("VARIANTS"))
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Variants Tag Is "+Convert.ToString(tags["VARIANTS"])); }
                if (tags["VARIANTS"] != null && tags["VARIANTS"] != "")
                {
                    List<MapMenuCustom_MorphSelector.ElemData> morphs = new List<MapMenuCustom_MorphSelector.ElemData>();
                    foreach (string stringId in tags["VARIANTS"].Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Ading Moprh '"+stringId+"'"); }
                        morphs.Add(new MapMenuCustom_MorphSelector.ElemData(new BoardAssetGuid(stringId), 1.0f));
                    }
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Applying Moprhs List"); }
                    CreatureManager.SendMorphListChanges(asset.CreatureId, morphs);
                }
            }
        }

        public IEnumerator PostSpawnEffectHandler(object[] inputs)
        {
            CreatureDataV2 creatureData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Effect " + creatureData.CreatureId); }
            yield return new WaitForSeconds(0.1f);
            CreatureBoardAsset asset = null;
            CreaturePresenter.TryGetAsset(creatureData.CreatureId, out asset);
            if (asset != null)
            {
                CustomAssetsLibraryPluginIntegratedExtention._self.StartCoroutine(Helpers.ShowHide(asset, CustomAssetsLibraryPluginIntegratedExtention.showHideUpdateDelay.Value));
            }
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
            CreatureDataV2 filterData = (CreatureDataV2)inputs[0];
            AssetDb.DbEntry databaseData = (AssetDb.DbEntry)inputs[1];
            Dictionary<string, string> tags = (Dictionary<string, string>)inputs[2];
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Processing Filter " + filterData.CreatureId); }
            // Request filter binding to target
            AssetDataPlugin.SetInfo(CreatureGuid.Empty.ToString(), CustomAssetsLibraryPluginIntegratedExtention.Guid + ".filter", filterData.CreatureId.ToString(), false);
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
