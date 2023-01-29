using BepInEx;
using Bounce.ManagedCollections;
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
        // public static void RemoteRequestRouter(string action, string source, string key, string previous, string value)
        public static void RemoteRequestRouter(AssetDataPlugin.DatumChange request)
        {
            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Remote " + request.action.ToString() + " Request For " + request.key + " (For " + request.source + ") " + request.previous + "->" + request.value); }
            request.key = request.key.Substring(CustomAssetsLibraryPluginIntegratedExtention.Guid.Length + 1);
            request.key = request.key.ToLower() + "." + request.action.ToString().ToLower();
            request.value = (request.value.ToString()+"@").Substring(0, (request.value.ToString()+"@").IndexOf("@"));
            switch (request.key)
            {
                case "animate.initial":
                case "animate.add":
                case "animate.modify":
                    RequestHandler.ApplyAnimate(new CreatureGuid(request.source), request.value.ToString());
                    break;
                case "animate.remove":
                    RequestHandler.ApplyAnimate(new CreatureGuid(request.source), "-1");
                    break;
                case "audio.initial":
                case "audio.add":
                case "audio.modify":
                    RequestHandler.ApplyAudio(new CreatureGuid(request.source), 1);
                    break;
                case "audio.remove":
                    RequestHandler.ApplyAudio(new CreatureGuid(request.source), -1);
                    break;
                case "aura.initial":
                case "aura.add":
                case "aura.modify":
                    RequestHandler.ApplyAura(new CreatureGuid(request.source), new CreatureGuid(request.value.ToString()));
                    break;
                case "aura.remove":
                    RequestHandler.RemoveAura(request.source, Convert.ToString(request.previous));
                    break;
                case "blendshape.initial":
                case "blendshape.add":
                case "blendshape.modify":
                    RequestHandler.ApplyBlendShapeSequence(new CreatureGuid(request.source), int.Parse(request.value.ToString()));
                    break;
                case "filter.initial":
                case "filter.add":
                case "filter.modify":
                    RequestHandler.ApplyFilter(new CreatureGuid(Convert.ToString(request.value)));
                    break;
                case "filter.remove":
                    RequestHandler.RemoveFilter(new CreatureGuid(Convert.ToString(request.previous)));
                    break;
                case "stop.initial":
                case "stop.add":
                case "stop.modify":
                case "stop.remove":
                    RequestHandler.ApplyAnimate(new CreatureGuid(request.source), "-1");
                    RequestHandler.ApplyAudio(new CreatureGuid(request.source), -1);
                    RequestHandler.ApplyBlendShapeSequence(new CreatureGuid(request.source), -1);
                    break;
                case "analyze.initial":
                case "analyze.add":
                case "analyze.modify":
                case "anazyle.remove":
                    RequestHandler.Analyze(new CreatureGuid(request.source), -1);
                    break;
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

            public static void ApplyAnimate(CreatureGuid cid, string selection)
            {
                try
                {
                    GameObject asset = Utility.GetAssetLoader(cid);
                    if (asset != null)
                    {
                        Animation anim = asset.GetComponentInChildren<Animation>();
                        if (anim != null)
                        {
                            if (selection == "-1")
                            {
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Stopping Animation On " + asset.name); }
                                anim.Stop();
                            }
                            else if (selection == "0")
                            {
                                SystemMessage.AskForTextInput("Animation", "Animation Name:", "OK", (animName) =>
                                {
                                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Starting Animation '" + animName + "' On " + asset.name);
                                    anim.Play(animName);
                                }, null, "Cancel", null, "");
                            }
                            else
                            {
                                int intSelection = 0;
                                if (int.TryParse(selection, out intSelection))
                                {
                                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Looking Up Animation '" + selection + "' On " + asset.name); }
                                    selection = GetAnimationName(anim, intSelection);
                                }
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Playing '" + selection + "' On " + asset.name); }
                                anim.Play(selection);
                            }
                        }
                        else
                        {
                            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Unable To Find Animation Component On Asset " + asset.name); }
                        }
                    }
                    else
                    {
                        if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: No Selected Asset To Animate"); }
                    }
                }
                catch (Exception x)
                {
                    Debug.LogError("Custom Assets Library Plugin Integrated Extension: Error Processing Animate " + selection + " On " + cid);
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
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Stopping Audio On " + asset.name); }
                                audio.Stop();
                            }
                            else
                            {
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Starting Audio On " + asset.name); }
                                audio.Play();
                            }
                        }
                        else
                        {
                            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Unable To Find AudioSource Component On Asset " + asset.name); }
                        }
                    }
                    else
                    {
                        if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: No Selected Asset For Audio Function"); }
                    }
                }
                catch (Exception x)
                {
                    Debug.LogError("Custom Assets Library Plugin Integrated Extension: Error Processing Audio " + selection + " On " + cid);
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.LogWarning(x); }
                }
            }

            public static void ApplyAura(CreatureGuid targetCid, CreatureGuid auraCid)
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Apply Aura " + auraCid); }
                GameObject targetBase = Utility.GetBaseLoader(targetCid);
                GameObject target = Utility.GetAssetLoader(targetCid);
                GameObject auraBase = Utility.GetBaseLoader(auraCid);
                GameObject aura = Utility.GetAssetLoader(auraCid);
                auraBase.name = "CustomContent:Base:" + targetCid + ":" + auraCid;
                aura.name = "CustomContent:Aura:" + targetCid + ":" + auraCid;
                if (target != null && targetBase != null && aura != null && auraBase != null)
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Apply Aura: Syncing Aura Base Position"); }
                    auraBase.transform.position = targetBase.transform.position;
                    auraBase.transform.rotation = target.transform.rotation;
                    auraBase.transform.SetParent(target.transform);
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Apply Aura: Syncing Aura Position"); }
                    aura.transform.position = targetBase.transform.position;
                    aura.transform.rotation = target.transform.rotation;
                    aura.transform.SetParent(target.transform);
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Apply Aura: Removing Base"); }
                    foreach (Renderer render in auraBase.GetComponentsInChildren<Renderer>())
                    {
                        render.enabled = false;
                    }
                }
                else
                {
                    Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Apply Aura: Unable To Access Asset");
                }
            }

            public static void RemoveAura(string identityCid, string auraCid)
            {
                if (auraCid.Contains("@")) { auraCid = auraCid.Substring(0, auraCid.IndexOf("@")); }
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Remove Aura "+auraCid+" From " + identityCid); }
                CreatureBoardAsset aura = null;
                CreaturePresenter.TryGetAsset(new CreatureGuid(auraCid), out aura);
                if (aura != null)
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Removing Aura From Talespire History"); }
                    aura.RequestDelete();
                    foreach (string obj in new string[] { "CustomContent:Base:" + identityCid + ":" + auraCid, "CustomContent:Aura:" + identityCid + ":" + auraCid })
                    {
                        if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Removing Object '"+obj+"'"); }
                        GameObject.Destroy(GameObject.Find(obj));
                    }
                }
            }

            public static void ApplyBlendShapeSequence(CreatureGuid cid, int selection)
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Applying BlendShape Sequence "+selection+" On Creature " + cid); }
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
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Requesting Global Blend Shape " + req.blendShapeIndex + " From " + req.start + " To " + req.end + " By " + req.step + " (Style " + req.style.ToString() + ")"); }
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
                                Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Unable To Access Blend Shape " + req.blendShapeIndex);
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Unable To Find SkinnedMeshRenderer Component On Asset " + asset.name);
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: No Selected Asset To Blend Shape");
                    }
                }
                catch (Exception x)
                {
                    Debug.LogError("Custom Assets Library Plugin Integrated Extension: Error Processing Blend Shape " + req.blendShapeIndex + " On " + cid);
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

            public static void ApplyFilter(CreatureGuid filterCid)
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Apply Filter " + filterCid); }
                GameObject camera = ActiveCameraManager.ActiveCamera.gameObject;
                GameObject filterAsset = Utility.GetAssetLoader(filterCid);
                GameObject filterBase = Utility.GetBaseLoader(filterCid);
                if (filterAsset != null)
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Apply Filter: Syncing Filter Position"); }
                    filterAsset.transform.position = camera.transform.position;
                    filterAsset.transform.rotation = camera.transform.rotation;
                    filterAsset.transform.SetParent(camera.transform);
                    filterBase.transform.position = camera.transform.position;
                    filterBase.transform.rotation = camera.transform.rotation;
                    filterBase.transform.SetParent(camera.transform);
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Apply Filter: Removing Base"); }
                    foreach (Renderer render in filterBase.GetComponentsInChildren<Renderer>())
                    {
                        render.enabled = false;
                    }
                }
                else
                {
                    Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Apply Filter: Unable To Access Filter");
                }
            }

            public static void RemoveFilter(CreatureGuid filterCid)
            {
                Debug.Log("Custom Assets Library Plugin Integrated Extension: Remove Filter: Removing Current Camera Filter ("+ filterCid+")");
                CreatureBoardAsset filter = null;
                CreaturePresenter.TryGetAsset(filterCid, out filter);
                if (filter != null)
                {
                    Debug.Log("Custom Assets Library Plugin Integrated Extension: Remove Filter: Camera Filter Object ("+filter.Name+") Remove Requested");
                    filter.RequestDelete();
                }
                else
                {
                    Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Remove Filter: Unable To Access Filter");
                }
            }

            public static void BuildSlabs(CreatureGuid unused1, object unused2)
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Paste Slab Or Multi-Slab Activated"); }
                string content = DirtyClipboardHelper.PullFromClipboard();
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Got:\r\n"+ content); }
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
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high)
                    {
                        Debug.Log("Custom Assets Library Plugin Integrated Extension: Content Looks Like Multi-Slab Data With " + slabs.Count + " Parts");
                    }
                }
                catch (Exception)
                {
                    slabs = new List<Data.SlabInfo>() { new Data.SlabInfo() { position = new Unity.Mathematics.float3(0f, 0f, 0f), code = slabCode } };
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high)
                    {
                        Debug.Log("Custom Assets Library Plugin Integrated Extension: Content Looks Single-Slab Data With 1 Part");
                    }
                }
                foreach (Data.SlabInfo slab in slabs)
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Processing Slab At "+Convert.ToString(slab.position)); }
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Processing Slab Code:\r\n" + slab.code); }
                    Copied copied = default(Copied);
                    if (BoardSessionManager.Board.PushStringToTsClipboard(slab.code, 0, slab.code.Length, out copied) == PushStringToTsClipboardResult.Success)
                    {
                        if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Slab Code Loaded"); }
                        Copied mostRecentCopied_LocalOnly = BoardSessionManager.Board.GetMostRecentCopied_LocalOnly();
                        if (mostRecentCopied_LocalOnly != null)
                        {
                            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Placing Slab. X:" + slab.position.x + " y:" + slab.position.x + " z:" + slab.position.z + " Slab: " + slab.code); }
                            BoardSessionManager.Board.PasteCopied(slab.position, 0, 0UL);
                            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.ultra) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Post Slab Placement Delay = " + delay); }
                            yield return new WaitForSeconds(delay);
                        }
                        else
                        {
                            Debug.Log("Custom Assets Library Plugin Integrated Extension: Unable To Process Most Recent Copied");
                        }
                    }
                    else
                    {
                        Debug.Log("Custom Assets Library Plugin Integrated Extension: Unable To Push Slab Code To TS Clipboard");
                    }
                }
            }

            #endregion

            #region Request Helpers

            private static void AnalyzeCreature(CreatureBoardAsset asset)
            {
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Object '" + Utility.GetCreatureName(asset.Name) + "' (Cid: " + asset.CreatureId + ", Type: " + asset.BoardAssetId + ")");
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Object '" + Utility.GetCreatureName(asset.Name) + "' is " + (asset.IsExplicitlyHidden ? "Hidden" : "Not Hidden"));
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Object '" + Utility.GetCreatureName(asset.Name) + "' is " + (asset.IsFlying ? "Flying" : "Not Flying"));
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Object '" + Utility.GetCreatureName(asset.Name) + "' is " + (asset.IsVisible ? "Visible" : "Not Visible"));
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Object '" + Utility.GetCreatureName(asset.Name) + "' is " + (asset.ShaderStateRef.State.IsCreatureHiddenByVolume ? "In Hide Volume" : "Not In Hide Volume"));
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Object '" + Utility.GetCreatureName(asset.Name) + "' is " + (asset.ShaderStateRef.State.InActiveLineOfSight ? "In LOS" : "Out Of LOS"));
                UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Object '" + Utility.GetCreatureName(asset.Name) + "' JSON is " + asset.Name);
                AnalyzeGameObject(Utility.GetAssetLoader(asset.CreatureId), 0);
            }

            private static void AnalyzeGameObject(GameObject go, int depth)
            {
                foreach (Renderer ren in go.GetComponentsInChildren<Renderer>())
                {
                    UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Renderer '" + ren.name + "' uses material with shader '" + ren.material.shader.name + "'");
                    foreach (Material mat in ren.materials)
                    {
                        UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: "+ren.GetType().ToString()+" Renderer '" + ren.name + "' has material with shader '" + mat.shader.name + "'");
                    }
                }
                foreach (Component component in go.GetComponents<Component>())
                {
                    UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Component '" + component.name + "' (Type " + component.GetType().ToString() + ")");
                }
                foreach (Transform trans in go.transform.Children())
                {
                    if (trans.gameObject != null && depth<5)
                    {
                        UnityEngine.Debug.Log("Custom Assets Library Plugin Integrated Extension: Child @ Level "+depth);
                        AnalyzeGameObject(trans.gameObject, depth+1);
                    }
                }
            }

            private static string GetAnimationName(Animation animation, int index)
            {
                string[] animNames = animationNames.Value.Split(',');
                List<AnimationState> anims = new List<AnimationState>(animation.Cast<AnimationState>());
                try
                {
                    // Try animation by names
                    if (Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Trying Animation '"+ animNames[index - 1] + "'"); }
                    foreach (AnimationState anim in anims)
                    {
                        if (anim.name == animNames[index - 1])
                        {
                            if (Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Loading Animation By State '"+ animNames[index - 1] + "'"); }
                            return anim.name;
                        }
                    }
                    // Try by default animation names
                    for (int i = 0; i < animNames.Length; i++) { animNames[i] = "Anim" + (i + 1).ToString("00"); }
                    if (Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Trying Animation '" + animNames[index - 1] + "'"); }
                    foreach (AnimationState anim in anims)
                    {
                        if (anim.name == animNames[index - 1])
                        {
                            if (Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Loading Animation By Default Anim Name '" + animNames[index - 1] + "'"); }
                            return anim.name;
                        }
                    }
                    // Try by index
                    if (Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Loading Animation By Index '" + index + "'"); }
                    return anims[index - 1].name;
                }
                catch 
                {
                    Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Animation Selection Not Supported On This Asset"); 
                    return anims[0].name;
                }
            }

            #endregion
        }
    }
}