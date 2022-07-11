using BepInEx;
using System;
using System.Reflection;
using UnityEngine;

namespace LordAshes
{
    public partial class CustomAssetsLibraryPluginIntegratedExtention : BaseUnityPlugin
    {

        public static class AssetDataPluginSoftDependency
        {
            public static MethodInfo SendInfo = null;
            public static MethodInfo SetInfo = null;
            public static MethodInfo ClearInfo = null;

            public static void Initialize()
            {
                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Checking For AssetDataPlugin (" + BepInEx.Paths.PluginPath + "\\LordAshes-AssetDataPlugin\\AssetDataPlugin.dll)..."); }
                if (System.IO.File.Exists(BepInEx.Paths.PluginPath + "\\LordAshes-AssetDataPlugin\\AssetDataPlugin.dll"))
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: AssetDataPlugin Is Present. Remote Mode Enabled."); }
                    Assembly aby = Assembly.LoadFile(BepInEx.Paths.PluginPath + "/LordAshes-AssetDataPlugin/AssetDataPlugin.dll");
                    Type type = null;
                    foreach (Type foundType in aby.GetTypes()) { if (foundType.Name == "AssetDataPlugin") { type = foundType; break; } }
                    if (type != null)
                    {
                        try
                        {
                            if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: AssetDataPlugin Seems To Be Loaded. Subscribing To " + CustomAssetsLibraryPluginIntegratedExtention.Guid + ".*"); }
                            type.GetMethod("SubscribeViaReflection").Invoke(null, new object[] { CustomAssetsLibraryPluginIntegratedExtention.Guid + ".*", "LordAshes.CustomAssetsLibraryPluginIntegratedExtention, CustomAssetsLibraryPluginIntegratedExtension", "RemoteRequestRouter" });
                        }
                        catch (Exception x)
                        {
                            Debug.LogWarning("Custom Assets Library Plugin Integrated Extension: Unable to Subscribe To " + CustomAssetsLibraryPluginIntegratedExtention.Guid + ".*");
                            Debug.LogWarning(x);
                        }
                        foreach (MethodInfo method in type.GetRuntimeMethods())
                        {
                            if (method.Name == "ClearInfo") 
                            { 
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Getting ClearInfo Reference"); } 
                                ClearInfo = method; 
                            }
                            if (method.Name == "SendInfo" && method.GetParameters()[1].ParameterType.ToString() == "System.String") 
                            {
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Getting SendInfo Reference"); }
                                SendInfo = method; 
                            }
                            if (method.Name == "SetInfo" && method.GetParameters()[2].ParameterType.ToString() == "System.String")
                            {
                                if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.high) { Debug.Log("Custom Assets Library Plugin Integrated Extension: Getting SetInfo Reference"); }
                                SetInfo = method; 
                            }
                            if (ClearInfo != null && SendInfo != null && SetInfo != null) { break; }
                        }
                    }
                }
                else
                {
                    if (CustomAssetsLibraryPluginIntegratedExtention.Diagnostics() >= DiagnosticMode.low) { Debug.Log("Custom Assets Library Plugin Integrated Extension: AssetDataPlugin Is Not Present. Local Mode Enabled."); }
                }
            }
        }
    }
}