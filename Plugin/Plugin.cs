using BepInEx;
using BepInEx.Configuration;
using Bounce.Unmanaged;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(LordAshes.FileAccessPlugin.Guid, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(CustomAssetsLibrary.CustomAssetLib.Guid, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("org.lordashes.plugins.assetdata", BepInDependency.DependencyFlags.SoftDependency)]
    public partial class CustomAssetsLibraryPluginIntegratedExtention : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Custom Assets Library Plugin Integrated Extension";
        public const string Guid = "org.lordashes.plugins.customassetslibraryintegratedextension";
        public const string Version = "1.3.0.0";

        // Public Enum
        public enum DiagnosticMode
        {
            none = 0,
            low = 1,
            high = 2,
            ultra = 3
        }

        public enum OperationMode
        {
            rebuildIndexAlways = 0,
            rebuildIndexIfMissing = 1,
            rebuildNever = 2
        }

        public class KeyboardHandler
        {
            public KeyboardShortcut trigger { get; set; }
            public bool alwaysLocal { get; set; } = false;
            public string handlerMethod { get; set; }
            public object handlerParameter { get; set; }
        }

        // Configuration
        private ConfigEntry<DiagnosticMode> diagnosticMode { get; set; }

        public static ConfigEntry<float> heightBarShaderDelay { get; set; }

        public static CustomAssetsLibraryPluginIntegratedExtention _self = null;

        public static List<KeyCode> activeModifierKeys = new List<KeyCode>();

        public bool pluginOk = false;

        public int tryCount = 0;

        public static Dictionary<string, KeyboardHandler> keyboardHandlers = new Dictionary<string, KeyboardHandler>()
        {
            {"Play Animaton 01", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha1, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 1 } },
            {"Play Animaton 02", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 2 } },
            {"Play Animaton 03", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha3, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 3 } },
            {"Play Animaton 04", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 4 } },
            {"Play Animaton 05", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 5 } },
            {"Play Animaton 06", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 6 } },
            {"Play Animaton 07", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 7 } },
            {"Play Animaton By Name", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha8, KeyCode.LeftControl), handlerMethod = "Animate", handlerParameter = 0 } },
            {"Play Audio", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha9, KeyCode.LeftControl), handlerMethod = "Audio", handlerParameter = -1 } },
            {"Stop All", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha0, KeyCode.LeftControl), handlerMethod = "Stop", handlerParameter = -1 } },
            {"Paste Multi-Slab", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.S, KeyCode.LeftControl), handlerMethod = "BuildSlabs", handlerParameter = -1, alwaysLocal = true } },
            {"Analyze Game Object", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.A, KeyCode.RightControl), handlerMethod = "Analyze", handlerParameter = -1, alwaysLocal = true } },
        };

        void Awake()
        {
            _self = this;

            diagnosticMode = Config.Bind("Troubleshooting", "Diagnostic Mode", DiagnosticMode.high);

            UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: "+this.GetType().AssemblyQualifiedName+" Active. Diagnostic Level = "+diagnosticMode.Value.ToString());

            heightBarShaderDelay = Config.Bind("Settings", "Delay To Determine Height Bar Has Stopped Moving", 0.5f);

            for (int t=0; t<keyboardHandlers.Count; t++)
            {
                keyboardHandlers[keyboardHandlers.ElementAt(t).Key].trigger = Config.Bind("Hotkeys", keyboardHandlers.ElementAt(t).Key, keyboardHandlers.ElementAt(t).Value.trigger).Value;
            }

            Harmony harmony = new Harmony(Guid);
            try
            {
                harmony.PatchAll();
                pluginOk = true;
            }
            catch (Exception)
            {
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Plugin seems broken possibly due to BR Update. Be patient while we fix it.");
                harmony.UnpatchSelf();
            }

            AssetDataPluginSoftDependency.Initialize();

            Utility.PostOnMainPage(this.GetType());
        }

        void Update()
        {
            if (pluginOk)
            {
                if (Utility.isBoardLoaded())
                {
                    // Board is loaded
                    if (CustomAssetsLibrary.Patches.AssetDbOnSetupInternalsPatch.HasSetup)
                    {
                        if (Patches.spawnList.Count > 0)
                        {
                            tryCount++;
                            if (tryCount > 100) 
                            {
                                Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Unable To Process " + Patches.spawnList.ElementAt(0).name + " (" + Patches.spawnList.ElementAt(0).CreatureId + "). Removing from spawn list.");
                                Patches.spawnList.RemoveAt(0); tryCount = 0; 
                            }
                            for (int spawnId = 0; spawnId < Patches.spawnList.Count; spawnId++)
                            {
                                CreatureBoardAsset asset = null;
                                // Asset is available
                                if (CreaturePresenter.TryGetAsset(Patches.spawnList[spawnId].CreatureId, out asset))
                                {
                                    // Asset has finished dropping in
                                    if (asset.HasDroppedIn)
                                    {
                                        // Asset had loaded
                                        if (Utility.GetAssetLoader(Patches.spawnList[spawnId].CreatureId) != null)
                                        {
                                            try
                                            {
                                                // Creasture data accessible
                                                CreatureDataV2 creatureData;
                                                if (CreatureManager.TryGetCreatureData(Patches.spawnList[spawnId].CreatureId, out creatureData))
                                                {
                                                    AssetDb.DbEntry databaseData = AssetDb.GetIndexData(Patches.spawnList[spawnId].BoardAssetId);
                                                    StartCoroutine("PostSpawnHandlerRouter", new object[] { creatureData, databaseData });
                                                    Patches.spawnList.RemoveAt(spawnId);
                                                    spawnId--;
                                                    tryCount = 0;
                                                }
                                            }
                                            catch (Exception) {; }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    foreach (KeyboardHandler handler in keyboardHandlers.Values)
                    {
                        if (Utility.StrictKeyCheck(handler.trigger))
                        {
                            if (AssetDataPluginSoftDependency.SetInfo != null && !handler.alwaysLocal)
                            {
                                // Remote request for functionality
                                Debug.Log("Custom Assets Library Plugin Integrated Extension: (Remote Mode) User Requested Setting Of " + CustomAssetsLibraryPluginIntegratedExtention.Guid + "." + handler.handlerMethod + " With Parameter " + Convert.ToString(handler.handlerParameter));
                                AssetDataPluginSoftDependency.SetInfo.Invoke(null, new object[] { LocalClient.SelectedCreatureId.ToString(), CustomAssetsLibraryPluginIntegratedExtention.Guid + "." + handler.handlerMethod, Convert.ToString(handler.handlerParameter) + "@" + DateTime.UtcNow.ToString(), false });
                            }
                            else
                            {
                                // Local request for functionality
                                Debug.Log("Custom Assets Library Plugin Integrated Extension: (Local Mode Only) User Requested Execution Of " + handler.handlerMethod + " With Parameter " + Convert.ToString(handler.handlerParameter));
                                typeof(CustomAssetsLibraryPluginIntegratedExtention.RequestHandler).GetMethod(handler.handlerMethod).Invoke(null, new object[] { LocalClient.SelectedCreatureId, handler.handlerParameter });
                            }
                        }
                    }
                }
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra)
                {
                    if (Patches.spawnList.Count > 0) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Backlog Entries = " + Patches.spawnList.Count+" @ Try "+tryCount); }
                }
            }
        }

        public static DiagnosticMode Diagnostics()
        {
            return CustomAssetsLibraryPluginIntegratedExtention._self.diagnosticMode.Value;
        }
    }
}
