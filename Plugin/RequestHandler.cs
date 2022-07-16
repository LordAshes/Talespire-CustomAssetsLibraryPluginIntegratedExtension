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

        #region Remote Request Router
        public static void RemoteRequestRouter(string action, string source, string key, string previous, string value)
        {
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Remote " + action + " Request For " + key + " (For " + source + ") " + previous + "->" + value); }
            if (action != "invalid")
            {
                key = key.Substring(CustomAssetsLibraryPluginIntegratedExtention.Guid.Length + 1);
                key = key.ToLower() + "." + action.ToLower();
                value = value.Substring(0, value.LastIndexOf("@"));
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Processing " +key+" -> "+value); }
                switch (key)
                {
                    case "animate.add":
                    case "animate.modify":
                        RequestHandler.Animate(new CreatureGuid(source), int.Parse(value));
                        break;
                    case "animate.remove":
                        RequestHandler.Animate(new CreatureGuid(source), -1);
                        break;
                    case "audio.add":
                    case "audio.modify":
                        RequestHandler.Audio(new CreatureGuid(source), 1);
                        break;
                    case "audio.remove":
                        RequestHandler.Audio(new CreatureGuid(source), -1);
                        break;
                    case "stop.add":
                    case "stop.modify":
                    case "stop.remove":
                        RequestHandler.Animate(new CreatureGuid(source), -1);
                        RequestHandler.Audio(new CreatureGuid(source), -1);
                        break;
                    case "analyze.add":
                    case "analyze.modify":
                    case "anazyle.remove":
                        RequestHandler.Analyze(new CreatureGuid(source), -1);
                        break;
                }
            }
        }

        #endregion

        #region Request Handlers
        public static class RequestHandler
        {
            public static void Animate(CreatureGuid cid, int selection)
            {
                try
                {
                    GameObject asset = Utility.GetAssetLoader(cid);
                    if (asset != null)
                    {
                        Animation anim = asset.GetComponentInChildren<Animation>();
                        if (anim != null)
                        {
                            if (selection == -1)
                            {
                                Debug.Log("Custom Assets Library Plugin Integrated Extension: Stopping Animation On " + asset.name);
                                anim.Stop();
                            }
                            else if (selection == 0)
                            {
                                SystemMessage.AskForTextInput("Animation", "Animation Name:", "OK", (animName) =>
                                {
                                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Starting Animation '" + animName + "' On " + asset.name);
                                    anim.Play(animName);
                                }, null, "Cancel", null, "");
                            }
                            else
                            {
                                Debug.Log("Custom Assets Library Plugin Integrated Extension: Starting Animation '" + "Anim" + selection.ToString("d2") + "' On " + asset.name);
                                anim.Play("Anim" + selection.ToString("d2"));
                            }
                        }
                        else
                        {
                            Debug.Log("Custom Assets Library Plugin Integrated Extension: Unable To Find Animation Component On Asset " + asset.name);
                        }
                    }
                    else
                    {
                        Debug.Log("Custom Assets Library Plugin Integrated Extension: No Selected Asset To Animate");
                    }
                }
                catch(Exception x)
                {
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Error Processing Animate "+selection+" On "+cid);
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning(x); }
                }
            }

            public static void Audio(CreatureGuid cid, int selection)
            {
                try
                {
                    GameObject asset = Utility.GetAssetLoader(cid);
                    if (asset != null)
                    {
                        AudioSource audio = asset.GetComponentInChildren<AudioSource>();
                        if (audio != null)
                        {
                            if (selection == -1)
                            {
                                Debug.Log("Custom Assets Library Plugin Integrated Extension: Stopping Audio On " + asset.name);
                                audio.Stop();
                            }
                            else
                            {
                                Debug.Log("Custom Assets Library Plugin Integrated Extension: Starting Audio On " + asset.name);
                                audio.Play();
                            }
                        }
                        else
                        {
                            Debug.Log("Custom Assets Library Plugin Integrated Extension: Unable To Find AudioSource Component On Asset " + asset.name);
                        }
                    }
                    else
                    {
                        Debug.Log("Custom Assets Library Plugin Integrated Extension: No Selected Asset For Audio Function");
                    }
                }
                catch(Exception x)
                {
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Error Processing Audio " + selection + " On " + cid);
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning(x); }
                }
            }

            public static void BuildSlabs(CreatureGuid notApplicable, int selection)
            {
                Debug.Log("Custom Assets Library Plugin Integrated Extension: Paste Slab Or Multi-Slab Activated");
                _self.StartCoroutine(RequestHandler.BuildMultipleSlabs(DirtyClipboardHelper.PullFromClipboard(), 0.1f));
            }

            public static void Analyze(CreatureGuid cid, object unused)
            {
                try
                {
                    GameObject go = Utility.GetAssetLoader(cid);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Analyzing Mini " + go.name);
                    AnalyzeGameObject(go);
                }
                catch (Exception x)
                {
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Error Processing Analyze On " + cid);
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning(x); }
                }
            }

            public static IEnumerator BuildMultipleSlabs(string slabCode, float delay)
            {
                List<Data.SlabInfo> slabs = null;
                try
                {
                    slabs = JsonConvert.DeserializeObject<List<Data.SlabInfo>>(slabCode);
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Content Looks Like Multi-Slab Data");
                }
                catch (Exception)
                {
                    slabs = new List<Data.SlabInfo>() { new Data.SlabInfo() { position = new Unity.Mathematics.float3(0f, 0f, 0f), code = slabCode } };
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Content Looks Single-Slab Data");
                }
                foreach (Data.SlabInfo slab in slabs)
                {
                    Copied copied = default(Copied);
                    if (BoardSessionManager.Board.PushStringToTsClipboard(slab.code, 0, slab.code.Length, out copied) == PushStringToTsClipboardResult.Success)
                    {
                        Copied mostRecentCopied_LocalOnly = BoardSessionManager.Board.GetMostRecentCopied_LocalOnly();
                        if (mostRecentCopied_LocalOnly != null)
                        {
                            Debug.Log("Custom Assets Library Plugin Integrated Extension: Placing Slab. X:" + slab.position.x + " y:" + slab.position.x + " z:" + slab.position.z + " Slab: " + slab.code);
                            BoardSessionManager.Board.PasteCopied(slab.position, 0, 0UL);
                            Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Slab Placement Delay = " + delay);
                            yield return new WaitForSeconds(delay);
                        }
                    }
                }
            }

            #endregion

            #region Request Helpers

            private static void AnalyzeGameObject(GameObject go)
            {
                foreach (MeshRenderer mr in go.GetComponentsInChildren<MeshRenderer>())
                {
                    UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Mesh Renderer '" + mr.name + "' uses material with shader '" + mr.material.shader.name + "'");
                    foreach (Material mat in mr.materials)
                    {
                        UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Mesh Renderer '" + mr.name + "' has material with shader '" + mat.shader.name + "'");
                    }
                }
                foreach (SkinnedMeshRenderer mr in go.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Skinned Mesh Renderer '" + mr.name + "' uses material with shader '" + mr.material.shader.name + "'");
                    foreach (Material mat in mr.materials)
                    {
                        UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Skinned Mesh Renderer '" + mr.name + "' has material with shader '" + mat.shader.name + "'");
                    }
                }
                foreach (Transform trans in go.transform.Children())
                {
                    if (trans.gameObject != null)
                    {
                        AnalyzeGameObject(trans.gameObject);
                    }
                }
            }

            #endregion
        }
    }
}
