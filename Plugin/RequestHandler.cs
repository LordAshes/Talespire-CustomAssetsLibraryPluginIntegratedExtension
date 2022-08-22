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
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Processing " + key + " -> " + value); }
                switch (key)
                {
                    case "animate.add":
                    case "animate.modify":
                        RequestHandler.ApplyAnimate(new CreatureGuid(source), int.Parse(value));
                        break;
                    case "animate.remove":
                        RequestHandler.ApplyAnimate(new CreatureGuid(source), -1);
                        break;
                    case "audio.add":
                    case "audio.modify":
                        RequestHandler.ApplyAudio(new CreatureGuid(source), 1);
                        break;
                    case "audio.remove":
                        RequestHandler.ApplyAudio(new CreatureGuid(source), -1);
                        break;
                    case "stop.add":
                    case "stop.modify":
                    case "stop.remove":
                        RequestHandler.ApplyAnimate(new CreatureGuid(source), -1);
                        RequestHandler.ApplyAudio(new CreatureGuid(source), -1);
                        RequestHandler.ApplyBlendShapeSequence(new CreatureGuid(source), -1);
                        break;
                    case "analyze.add":
                    case "analyze.modify":
                    case "anazyle.remove":
                        RequestHandler.Analyze(new CreatureGuid(source), -1);
                        break;
                    case "blendshape.add":
                    case "blendshape.modify":
                        RequestHandler.ApplyBlendShapeSequence(new CreatureGuid(source), int.Parse(value));
                        break;
                }
            }
        }

        #endregion

        #region Request Handlers
        public static class RequestHandler
        {
            public enum BlendShapeTransitionStyle
            {
                Single = 1,
                PingPong = 2,
                Loop = 3
            }

            public class BlendShapeApplication
            {
                public BlendShapeTransitionStyle style = BlendShapeTransitionStyle.Single;
                public Tuple<SkinnedMeshRenderer, int> skin { get; set; } = null;
                public float start { get; set; } = 0.0f;
                public float current { get; set; } = 0.0f;
                public float end { get; set; } = 100.0f;
                public float step { get; set; } = 1.0f;
            }

            public class BlendShapeRequest
            {
                public BlendShapeTransitionStyle style = BlendShapeTransitionStyle.Single;
                public int blendShapeIndex = 0;
                public float start { get; set; } = 0.0f;
                public float end { get; set; } = 100.0f;
                public float step { get; set; } = 1.0f;
            }

            public class BlendShapeSequence
            {
                public List<BlendShapeRequest> elements = new List<BlendShapeRequest>();
            }

            public class BlendShapeSequences
            {
                public List<BlendShapeSequence> blendshapes = new List<BlendShapeSequence>();
            }

            private static List<BlendShapeApplication> blendShapesInProgress = new List<BlendShapeApplication>();

            public static void ApplyAnimate(CreatureGuid cid, int selection)
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
                catch (Exception x)
                {
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Error Processing Animate " + selection + " On " + cid);
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning(x); }
                }
            }

            public static void ApplyAudio(CreatureGuid cid, int selection)
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
                catch (Exception x)
                {
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Error Processing Audio " + selection + " On " + cid);
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning(x); }
                }
            }

            public static void ApplyBlendShapeSequence(CreatureGuid cid, int selection)
            {
                if (selection == -1)
                {
                    blendShapesInProgress.Clear();
                }
                else
                {
                    CreatureBoardAsset cba = null;
                    CreaturePresenter.TryGetAsset(cid, out cba);
                    Dictionary<string, object> info = new Dictionary<string, object>();
                    if (cba != null)
                    {
                        info = Helpers.GetAssetInfo(cba.BoardAssetId);
                    }
                    if (info.ContainsKey("blendshapes"))
                    {
                        try
                        {
                            BlendShapeSequences seqs = JsonConvert.DeserializeObject<BlendShapeSequences>("{\"blendshapes\": " + info["blendshapes"].ToString() + "\r\n}");
                            if (seqs.blendshapes.Count >= selection)
                            {
                                BlendShapeSequence seq = seqs.blendshapes[selection - 1];
                                foreach (BlendShapeRequest req in seq.elements)
                                {
                                    ApplyBlendShape(cid, req);
                                }
                            }
                        }
                        catch (Exception x)
                        {
                            Debug.Log("Custom Assets Library Plugin Integrated Extension: Exception While Blend Shaping");
                            Debug.LogException(x);
                        }
                    }
                }
            }

            public static void ApplyBlendShape(CreatureGuid cid, BlendShapeRequest req)
            {
                try
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Requesting Global Blend Shape " + req.blendShapeIndex + " From " + req.start + " To " + req.end + " By " + req.step + " (Style " + req.style.ToString() + ")"); }
                    GameObject asset = Utility.GetAssetLoader(cid);
                    if (asset != null)
                    {
                        SkinnedMeshRenderer[] skins = asset.GetComponentsInChildren<SkinnedMeshRenderer>();
                        List<Tuple<SkinnedMeshRenderer, int>> blendShapes = new List<Tuple<SkinnedMeshRenderer, int>>();
                        int blendshape = 0;
                        foreach (SkinnedMeshRenderer skin in skins)
                        {
                            for (int offset = 0; offset < skin.sharedMesh.blendShapeCount; offset++)
                            {
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Global Blend Shape " + blendshape + " = Renderer '" + skin.name + "' Blend Shape '" + skin.sharedMesh.GetBlendShapeName(offset) + "' (Index: " + offset + ")"); }
                                blendShapes.Add(new Tuple<SkinnedMeshRenderer, int>(skin, offset));
                                blendshape++;
                            }
                        }
                        if (skins != null)
                        {
                            try
                            {
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) 
                                { 
                                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Adding Blend Shape Sequence (" + req.blendShapeIndex + ") '" + blendShapes[req.blendShapeIndex - 1].Item1.name + "."+ blendShapes[req.blendShapeIndex - 1].Item1.sharedMesh.GetBlendShapeName(blendShapes[req.blendShapeIndex - 1].Item2) + "': "+req.start + " To " + req.end + " By " + req.step + " (" + req.style.ToString() + ")");
                                }
                                blendShapesInProgress.Add(new BlendShapeApplication()
                                {
                                    skin = blendShapes[req.blendShapeIndex - 1],
                                    style = req.style,
                                    start = req.start,
                                    current = req.start,
                                    end = req.end,
                                    step = req.step
                                });
                            }
                            catch (Exception)
                            {
                                Debug.Log("Custom Assets Library Plugin Integrated Extension: Unable To Access Blend Shape " + req.blendShapeIndex);
                            }
                        }
                        else
                        {
                            Debug.Log("Custom Assets Library Plugin Integrated Extension: Unable To Find SkinnedMeshRenderer Component On Asset " + asset.name);
                        }
                    }
                    else
                    {
                        Debug.Log("Custom Assets Library Plugin Integrated Extension: No Selected Asset To Blend Shape");
                    }
                }
                catch (Exception x)
                {
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Error Processing Blend Shape " + req.blendShapeIndex + " On " + cid);
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning(x); }
                }
            }

            public static void UpdateBlendShape()
            {
                for (int bs = 0; bs < blendShapesInProgress.Count; bs++)
                {
                    blendShapesInProgress[bs].current = blendShapesInProgress[bs].current + blendShapesInProgress[bs].step;
                    if (((blendShapesInProgress[bs].step < 0) && (blendShapesInProgress[bs].current <= blendShapesInProgress[bs].end)) ||
                        ((blendShapesInProgress[bs].step > 0) && (blendShapesInProgress[bs].current >= blendShapesInProgress[bs].end)))
                    {
                        blendShapesInProgress[bs].current = blendShapesInProgress[bs].end;
                        blendShapesInProgress[bs].skin.Item1.SetBlendShapeWeight(blendShapesInProgress[bs].skin.Item2, blendShapesInProgress[bs].current);
                        switch (blendShapesInProgress[bs].style)
                        {
                            case BlendShapeTransitionStyle.Single:
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Sequence '" + blendShapesInProgress[bs].skin.Item1.name+"."+ blendShapesInProgress[bs].skin.Item1.sharedMesh.GetBlendShapeName(blendShapesInProgress[bs].skin.Item2) + "' Mode '" + blendShapesInProgress[bs].style.ToString() + "' End Reached. Ending."); }
                                blendShapesInProgress.RemoveAt(bs); bs--;
                                break;
                            case BlendShapeTransitionStyle.PingPong:
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Sequence '" + blendShapesInProgress[bs].skin.Item1.name + "." + blendShapesInProgress[bs].skin.Item1.sharedMesh.GetBlendShapeName(blendShapesInProgress[bs].skin.Item2) + "' Mode '" + blendShapesInProgress[bs].style.ToString() + "' End Reached. Reversing."); }
                                blendShapesInProgress[bs].style = BlendShapeTransitionStyle.Single;
                                blendShapesInProgress[bs].step = -1 * blendShapesInProgress[bs].step;
                                blendShapesInProgress[bs].end = blendShapesInProgress[bs].start;
                                blendShapesInProgress[bs].start = blendShapesInProgress[bs].current;
                                break;
                            case BlendShapeTransitionStyle.Loop:
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Sequence '" + blendShapesInProgress[bs].skin.Item1.name + "." + blendShapesInProgress[bs].skin.Item1.sharedMesh.GetBlendShapeName(blendShapesInProgress[bs].skin.Item2) + "' Mode '" + blendShapesInProgress[bs].style.ToString() + "' End Reached. Looping."); }
                                blendShapesInProgress[bs].step = -1 * blendShapesInProgress[bs].step;
                                blendShapesInProgress[bs].end = blendShapesInProgress[bs].start;
                                blendShapesInProgress[bs].start = blendShapesInProgress[bs].current;
                                break;
                        }
                    }
                    else
                    {
                        if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Setting Sequence '" + blendShapesInProgress[bs].skin.Item1.name + "." + blendShapesInProgress[bs].skin.Item1.sharedMesh.GetBlendShapeName(blendShapesInProgress[bs].skin.Item2) + "' Mode '" + blendShapesInProgress[bs].style.ToString() + "' To " + blendShapesInProgress[bs].current); }
                        blendShapesInProgress[bs].skin.Item1.SetBlendShapeWeight(blendShapesInProgress[bs].skin.Item2, blendShapesInProgress[bs].current);
                    }
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
                    CreatureBoardAsset asset = null;
                    CreaturePresenter.TryGetAsset(cid, out asset);
                    if (asset != null)
                    {
                        Debug.Log("Custom Assets Library Plugin Integrated Extension: Analyzing Mini " + asset.name);
                        AnalyzeCreature(asset);
                    }
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

            private static void AnalyzeCreature(CreatureBoardAsset asset)
            {
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Object '" + asset.name + "' (Cid: " + asset.CreatureId + ", Type: " + asset.BoardAssetId + ")");
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Object '" + asset.name + "' is " + (asset.IsExplicitlyHidden ? "Hidden" : "Visible"));
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Object '" + asset.name + "' is " + (asset.IsFlying ? "Flying" : "Not Flying"));
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Object '" + asset.name + "' is " + (asset.IsGrounded ? "Grounded" : "Not Grounded"));
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Object '" + asset.name + "' is " + (asset.IsVisible ? "Visible" : "Not Visible"));
                AnalyzeGameObject(Utility.GetAssetLoader(asset.CreatureId));
            }

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
                foreach (Component component in go.GetComponentsInChildren<Component>())
                {
                    UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Component '" + component.name + "' (Type " + component.GetType().ToString() + ")");
                }
                foreach (Transform trans in go.transform.Children())
                {
                    if (trans.gameObject != null)
                    {
                    }

                    #endregion
                }
            }
        }
    }
}