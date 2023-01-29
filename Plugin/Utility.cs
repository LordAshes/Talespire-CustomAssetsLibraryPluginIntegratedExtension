using BepInEx;
using BepInEx.Configuration;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LordAshes
{
    public partial class CustomAssetsLibraryPluginIntegratedExtention : BaseUnityPlugin
    {
        public static class Utility
        {
            public static void PostOnMainPage(System.Reflection.MemberInfo plugin)
            {
                SceneManager.sceneLoaded += (scene, mode) =>
                {
                    try
                    {
                        if (scene.name == "UI")
                        {
                            TextMeshProUGUI betaText = GetUITextByName("BETA");
                            if (betaText)
                            {
                                betaText.text = "INJECTED BUILD - unstable mods";
                            }
                        }
                        else
                        {
                            TextMeshProUGUI modListText = GetUITextByName("TextMeshPro Text");
                            if (modListText)
                            {
                                BepInPlugin bepInPlugin = (BepInPlugin)Attribute.GetCustomAttribute(plugin, typeof(BepInPlugin));
                                if (modListText.text.EndsWith("</size>"))
                                {
                                    modListText.text += "\n\nMods Currently Installed:\n";
                                }
                                modListText.text += "\nLord Ashes' " + bepInPlugin.Name + " - " + bepInPlugin.Version;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.Log(ex);
                    }
                };
            }

            /// <summary>
            /// Function to check if the board is loaded
            /// </summary>
            /// <returns></returns>
            public static bool isBoardLoaded()
            {
                return CameraController.HasInstance && BoardSessionManager.HasInstance && !BoardSessionManager.IsLoading;
            }

            /// <summary>
            /// Method to properly evaluate shortcut keys. 
            /// </summary>
            /// <param name="check"></param>
            /// <returns></returns>
            public static bool StrictKeyCheck(KeyboardShortcut check)
            {
                if (!check.IsUp()) { return false; }
                foreach (KeyCode modifier in new KeyCode[] { KeyCode.LeftAlt, KeyCode.RightAlt, KeyCode.LeftControl, KeyCode.RightControl, KeyCode.LeftShift, KeyCode.RightShift })
                {
                    if (Input.GetKey(modifier) != check.Modifiers.Contains(modifier)) { return false; }
                }
                return true;
            }

            /// <summary>
            /// Method to generate a Guid from a string
            /// </summary>
            /// <param name="input">Text</param>
            /// <returns>Guid based on the input text</returns>
            public static Guid GuidFromString(string input)
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] hash = md5.ComputeHash(Encoding.Default.GetBytes(input));
                    return new Guid(hash);
                }
            }

            /// <summary>
            /// Method to obtain the Base Loader Game Object based on a CreatureGuid
            /// </summary>
            /// <param name="cid">Creature Guid</param>
            /// <returns>BaseLoader Game Object</returns>
            public static GameObject GetBaseLoader(CreatureGuid cid)
            {
                try
                {
                    CreatureBoardAsset asset = null;
                    CreaturePresenter.TryGetAsset(cid, out asset);
                    if (asset != null)
                    {
                        Transform baseLoader = null;
                        Traverse(asset.transform, "BaseLoader", 0, 10, ref baseLoader);
                        if (baseLoader != null)
                        {
                            return baseLoader.GetChild(0).gameObject;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            }

            /// <summary>
            /// Method to obtain the Asset Loader Game Object based on a CreatureGuid
            /// </summary>
            /// <param name="cid">Creature Guid</param>
            /// <returns>AssetLoader Game Object</returns>
            public static GameObject GetAssetLoader(CreatureGuid cid)
            {
                try
                {
                    CreatureBoardAsset asset = null;
                    CreaturePresenter.TryGetAsset(cid, out asset);
                    if (asset != null)
                    {
                        Transform assetLoader = null;
                        Traverse(asset.transform, "AssetLoader", 0, 10, ref assetLoader);
                        if (assetLoader != null)
                        {
                            if (assetLoader.childCount > 0)
                            {
                                return assetLoader.GetChild(0).gameObject;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                    return null;
                }
                catch
                {
                    return null;
                }
            }

            public static void Traverse(Transform root, string seek, int depth, int depthMax, ref Transform match)
            {
                try
                {
                    if (match != null) { return; }
                    if (root.name == seek) { match = root; return; }
                    foreach (Transform child in root.Children())
                    {
                        if (depth < depthMax)
                        {
                            Traverse(child, seek, depth + 1, depthMax, ref match);
                        }
                    }
                }
                catch
                {
                }
            }

            public static void DeleteChildrenCustomContent(Transform root, string seek, int depth, int depthMax)
            {
                try
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Deleting Children Custom Objects: Found '" + root.name + " ("+root.gameObject.name+")', Seeking '"+seek+"', Depth="+depth); }
                    if (root.name.StartsWith(seek) || root.gameObject.name.StartsWith(seek)) 
                    {
                        if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Deleting Game Object "+ root.name+" ("+root.gameObject.name+")"); }
                        GameObject.Destroy(root.gameObject);
                    }
                    foreach (Transform child in root.Children())
                    {
                        if (depth < depthMax)
                        {
                            DeleteChildrenCustomContent(child, seek, depth + 1, depthMax);
                        }
                    }
                }
                catch
                {
                }
            }

            public static string GetCreatureName(string nameBlock)
            {
                if (nameBlock==null) { return "(Unknown)"; }
                if (!nameBlock.Contains("<size=0>")) { return nameBlock; }
                return nameBlock.Substring(0, nameBlock.IndexOf("<size=0>")).Trim();
            }

            private static TextMeshProUGUI GetUITextByName(string name)
            {
                TextMeshProUGUI[] texts = UnityEngine.Object.FindObjectsOfType<TextMeshProUGUI>();
                for (int i = 0; i < texts.Length; i++)
                {
                    if (texts[i].name == name)
                    {
                        return texts[i];
                    }
                }
                return null;
            }
        }
    }
}
