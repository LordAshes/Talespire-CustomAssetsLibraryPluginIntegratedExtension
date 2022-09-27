using BepInEx;
using Bounce.Unmanaged;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LordAshes
{
    public partial class CustomAssetsLibraryPluginIntegratedExtention : BaseUnityPlugin
    {

        #region Pre Spawn Router

        public static bool PreSpawnHandlerRouter(NGuid guid, AssetDb.DbEntry entry)
        {
            // Trigger Post Spawn Callback
            Dictionary<string, string> tags = new Dictionary<string, string>();
            // Build Tags Dictionary
            foreach (string item in entry.Tags)
            {
                if (item.Contains(":")) { tags.Add(item.Substring(0, item.IndexOf(":")).ToUpper(), item.Substring(item.IndexOf(":") + 1)); } else { tags.Add(item, item); }
            }
            if (!tags.ContainsKey("KIND")) { tags.Add("KIND", "Creature"); }
            string kind = Helpers.ModifyKindBasedOnModifier(tags["KIND"]);
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Pre Spawn Router: Asset is kind "+tags["KIND"]+" being treated as "+kind); }
            // Trigger Remote Pre Spawn Callback
            return (bool)typeof(CustomAssetsLibraryPluginIntegratedExtention).GetMethod("PreSpawn" + kind + "Handler").Invoke(null, new object[] { guid, entry, tags });            
        }

        #endregion

        #region Pre Spawn Handlers

        public static bool PreSpawnAudioHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Pre Spawn Handler: Processing Audio Of Type "+nguid.ToString()); }
            return true;
        }

        public static bool PreSpawnAuraHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Pre Spawn Handler: Processing Aura Of Type " + nguid.ToString()); }
            Helpers.SpawnPrevent();
            Helpers.SpawnCreatureByNGuid(nguid);
            return false;
        }

        public static bool PreSpawnCreatureHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Pre Spawn Handler: Processing Creature Of Type " + nguid.ToString()); }
            return true;
        }

        public static bool PreSpawnEffectHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Pre Spawn Handler: Processing Effect Of Type " + nguid.ToString()); }
            return true;
        }

        public static bool PreSpawnEncounterHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Pre Spawn Handler: Processing Encounter Of Type " + nguid.ToString()); }
            SystemMessage.DisplayInfoText("Encounter Type Is Not Yet Supported");
            Helpers.SpawnPrevent();
            return false;
        }

        public static bool PreSpawnFilterHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Pre Spawn Handler: Processing Filter Of Type " + nguid.ToString()); }
            Helpers.SpawnPrevent();
            Helpers.SpawnCreatureByNGuid(nguid);
            return false;
        }

        public static bool PreSpawnPropHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Pre Spawn Handler: Processing Prop Of Type " + nguid.ToString()); }
            return true;
        }

        public static bool PreSpawnSlabHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Pre Spawn Handler: Processing Slab Of Type " + nguid.ToString()); }
            Helpers.SpawnPrevent();
            string assetLocation = tags["ASSETBUNDLE"];
            AssetBundle assetBundle = null;
            foreach(AssetBundle loadedAssetBundle in AssetBundle.GetAllLoadedAssetBundles())
            {
                if (loadedAssetBundle.GetAllAssetNames().Contains(System.IO.Path.GetFileNameWithoutExtension(assetLocation))) { assetBundle = loadedAssetBundle; }
            }
            if (assetBundle == null) { assetBundle = AssetBundle.LoadFromFile(assetLocation); }
            string slabCode = assetBundle.LoadAsset<TextAsset>("Info.txt").text;
            _self.StartCoroutine(RequestHandler.BuildMultipleSlabs(slabCode, 0.1f));
            return false;
        }

        public static bool PreSpawnTileHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Pre Spawn Handler: Processing Tile Of Type " + nguid.ToString()); }
            return true;
        }

        public static bool PreSpawnTransformHandler(NGuid nguid, AssetDb.DbEntry databaseData, Dictionary<string, string> tags)
        {
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Pre Spawn Handler: Processing Transform Of Type " + nguid.ToString()); }
            Helpers.SpawnPrevent();
            Helpers.SpawnCreatureByNGuid(nguid);
            return false;
        }

        #endregion
    }
}
