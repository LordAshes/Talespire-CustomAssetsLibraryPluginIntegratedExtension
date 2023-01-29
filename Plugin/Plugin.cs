using BepInEx;
using BepInEx.Configuration;
using Bounce.Unmanaged;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace LordAshes
{
    [BepInPlugin(Guid, Name, Version)]
    [BepInDependency(LordAshes.FileAccessPlugin.Guid, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(LordAshes.AssetDataPlugin.Guid, BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency(RadialUI.RadialUIPlugin.Guid, BepInDependency.DependencyFlags.HardDependency)]
    public partial class CustomAssetsLibraryPluginIntegratedExtention : BaseUnityPlugin
    {
        // Plugin info
        public const string Name = "Custom Assets Library Plugin Integrated Extension";
        public const string Guid = "org.lordashes.plugins.customassetslibraryintegratedextension";
        public const string Version = "2.0.0.1";

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
            public string handlerMethod { get; set; }
            public object handlerParameter { get; set; }
        }

        // Configuration
        private ConfigEntry<DiagnosticMode> diagnosticMode { get; set; }

        public static ConfigEntry<float> showHideUpdateDelay { get; set; }
        public static ConfigEntry<string> animationNames { get; set; }

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
            {"Apply Blend Shape 01", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha1, KeyCode.LeftAlt), handlerMethod = "BlendShape", handlerParameter = 1 } },
            {"Apply Blend Shape 02", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha2, KeyCode.LeftAlt), handlerMethod = "BlendShape", handlerParameter = 2 } },
            {"Apply Blend Shape 03", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha3, KeyCode.LeftAlt), handlerMethod = "BlendShape", handlerParameter = 3 } },
            {"Apply Blend Shape 04", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha4, KeyCode.LeftAlt), handlerMethod = "BlendShape", handlerParameter = 4 } },
            {"Apply Blend Shape 05", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha5, KeyCode.LeftAlt), handlerMethod = "BlendShape", handlerParameter = 5 } },
            {"Apply Blend Shape 06", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha6, KeyCode.LeftAlt), handlerMethod = "BlendShape", handlerParameter = 6 } },
            {"Apply Blend Shape 07", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha7, KeyCode.LeftAlt), handlerMethod = "BlendShape", handlerParameter = 7 } },
            {"Play Audio", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha9, KeyCode.LeftControl), handlerMethod = "Audio", handlerParameter = -1 } },
            {"Stop All", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha0, KeyCode.LeftControl), handlerMethod = "Stop", handlerParameter = -1 } },
            {"Stop All (Alternate)", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.Alpha0, KeyCode.LeftAlt), handlerMethod = "Stop", handlerParameter = -1 } },
        };

        public static Dictionary<string, KeyboardHandler> keyboardHandlersForLocalActions = new Dictionary<string, KeyboardHandler>()
        {
            {"Paste Multi-Slab", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.S, KeyCode.LeftControl), handlerMethod = "BuildSlabs", handlerParameter = -1} },
            {"Analyze Game Object", new KeyboardHandler() {trigger = new KeyboardShortcut(KeyCode.A, KeyCode.RightControl), handlerMethod = "Analyze", handlerParameter = -1} },
        };

        private static MethodInfo spawnCreature = null;

        void Awake()
        {
            _self = this;

            diagnosticMode = Config.Bind("Troubleshooting", "Diagnostic Mode", DiagnosticMode.high);

            Debug.Log("Custom Assets Library Plugin Integrated Extension: "+this.GetType().AssemblyQualifiedName+" Active. Diagnostic Level = "+diagnosticMode.Value.ToString());

            showHideUpdateDelay = Config.Bind("Settings", "Delay After Spawn To Update Non-TS Shader Content", 3.0f);

            animationNames = Config.Bind("Settings", "Animation Names", "Idle,Ready,Melee,Range,Dead,Magic");

            RadialUI.RadialUIPlugin.AddCustomButtonOnCharacter(CustomAssetsLibraryPluginIntegratedExtention.Guid + ".RemoveAura",
                new MapMenu.ItemArgs()
                {
                    Action = (a,b)=> 
                    {
                        if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Clearing Aura On "+ Convert.ToString(LocalClient.SelectedCreatureId)); }
                        AssetDataPlugin.ClearInfo(LocalClient.SelectedCreatureId.ToString(),CustomAssetsLibraryPluginIntegratedExtention.Guid+".aura"); 
                    },
                    FadeName = true,
                    CloseMenuOnActivate = true,
                    Icon = FileAccessPlugin.Image.LoadSprite("RemoveAura.png"),
                    Title = "Remove Aura"
                });

            for (int t=0; t<keyboardHandlers.Count; t++)
            {
                keyboardHandlers[keyboardHandlers.ElementAt(t).Key].trigger = Config.Bind("Hotkeys", keyboardHandlers.ElementAt(t).Key, keyboardHandlers.ElementAt(t).Value.trigger).Value;
            }

            Harmony harmony = new Harmony(Guid);
            try
            {
                harmony.PatchAll();
                pluginOk = true;
                foreach (string suffix in new string[] { "animate", "audio", "blendshape", "stop" })
                {
                    AssetDataPlugin.Subscribe(CustomAssetsLibraryPluginIntegratedExtention.Guid + "." + suffix, RemoteRequestRouter, AssetDataPlugin.Backlog.Checker.CheckSourceAsCreature);
                }
                foreach (string suffix in new string[] { "aura" })
                {
                    AssetDataPlugin.Subscribe(CustomAssetsLibraryPluginIntegratedExtention.Guid + "." + suffix, RemoteRequestRouter, AssetDataPlugin.Backlog.Checker.CheckSourceAndValueAsCreature);
                }
                foreach (string suffix in new string[] { "filter" })
                {
                    AssetDataPlugin.Subscribe(CustomAssetsLibraryPluginIntegratedExtention.Guid + "." + suffix, RemoteRequestRouter, (dc) => 
                    {
                        if (dc.action!=AssetDataPlugin.ChangeAction.remove)
                        {
                            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Checking Filter " + dc.value); }
                            CreatureBoardAsset verifyAsset = null;
                            CreaturePresenter.TryGetAsset(new CreatureGuid(dc.value.ToString()), out verifyAsset);
                            if (verifyAsset != null) { return true; } else { return false; }
                        }
                        else
                        {
                            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Spawn Handler: Filter Removal Requires No Check"); }
                            return true;
                        }
                    });
                }
                Utility.PostOnMainPage(this.GetType());
            }
            catch (Exception)
            {
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Plugin seems broken possibly due to BR Update. Be patient while we fix it.");
                harmony.UnpatchSelf();
            }
        }

        void Update()
        {
            if (pluginOk)
            {
                if (Utility.isBoardLoaded())
                {
                    // Board is loaded
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

                    RequestHandler.UpdateBlendShape();

                    foreach (KeyboardHandler handler in keyboardHandlers.Values)
                    {
                        if (Utility.StrictKeyCheck(handler.trigger))
                        {
                            // Remote request for functionality
                            Debug.Log("Custom Assets Library Plugin Integrated Extension: (Remote Mode) User Requested Setting Of " + CustomAssetsLibraryPluginIntegratedExtention.Guid + "." + handler.handlerMethod + " With Parameter " + Convert.ToString(handler.handlerParameter));
                            AssetDataPlugin.SetInfo(LocalClient.SelectedCreatureId.ToString(), CustomAssetsLibraryPluginIntegratedExtention.Guid + "." + handler.handlerMethod, Convert.ToString(handler.handlerParameter) + "@" + DateTime.UtcNow.ToString());
                        }
                    }
                    foreach (KeyboardHandler handler in keyboardHandlersForLocalActions.Values)
                    {
                        if (Utility.StrictKeyCheck(handler.trigger))
                        {
                            // Local request for functionality
                            Debug.Log("Custom Assets Library Plugin Integrated Extension: (Local Mode) User Requested Setting Of " + CustomAssetsLibraryPluginIntegratedExtention.Guid + "." + handler.handlerMethod + " With Parameter " + Convert.ToString(handler.handlerParameter));
                            MethodInfo method = typeof(RequestHandler).GetMethod(handler.handlerMethod);
                            method.Invoke(null, new object[] { LocalClient.SelectedCreatureId, Convert.ToString(handler.handlerParameter) + "@" + DateTime.UtcNow.ToString() });
                        }
                    }
                }
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra)
                {
                    if (Patches.spawnList.Count > 0) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Spawn Entries = " + Patches.spawnList.Count+" @ Try "+tryCount); }
                }
            }
        }

        public static DiagnosticMode Diagnostics()
        {
            return CustomAssetsLibraryPluginIntegratedExtention._self.diagnosticMode.Value;
        }
    }
}
