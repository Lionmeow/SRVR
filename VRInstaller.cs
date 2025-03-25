﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using HarmonyLib;
using Mono.Cecil;
using Mono.Cecil.Rocks;
using SRML;
using SRML.Utils;
using Unity.XR.OpenVR;
using UnityEngine;
using Valve.VR;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace SRVR
{
    public static class VRInstaller
    {
        public static bool IsAfterInstall = true;
        public static List<Assembly> Assemblies = new List<Assembly>();
        public static string VRInstallerPath;
        
        public static bool Prefix(string className, ref Type __result)
        {
            __result = AccessTools.TypeByName(className);
            return false;
        }

        public static void InstallPatch()
        {
            
            Harmony harmony = new Harmony("SRVR.Installer");
            harmony.Patch(typeof(Unity.XR.OpenVR.OpenVRHelpers).GetMethod("GetType", new []
            {
                typeof(string), typeof(bool)
            }), prefix: new HarmonyMethod(typeof(VRInstaller).GetMethod("Prefix")));
        }
        public static void Install()
        {
            var unitySubsystemsDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, "UnitySubsystems"));
            var pluginsDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, "Plugins", "x86_64"));
            var streamingAssetsDirectory = new DirectoryInfo(Path.Combine(Application.streamingAssetsPath, "SteamVR"));

            var execAssembly = typeof(EntryPoint).Assembly;
            VRInstallerPath = Path.Combine(Application.dataPath, "SRVRInstaller.exe");
            if (unitySubsystemsDirectory.Exists && File.Exists(Path.Combine(pluginsDirectory.FullName, "openvr_api.dll")) && streamingAssetsDirectory.Exists && File.Exists(VRInstallerPath))
            {
                IsAfterInstall = true;
                //

            }
            else
            {
                IsAfterInstall = false;
                unitySubsystemsDirectory.Create();
            
                // Create the "SteamVR" directory under StreamingAssets
                if (!streamingAssetsDirectory.Exists)
                    streamingAssetsDirectory.Create();
            
                // Loop through all resource names
                foreach (var manifestResourceName in execAssembly.GetManifestResourceNames())
                {
                    if (manifestResourceName.Contains("SRVRInstaller.exe"))
                    {
                        string UnitySubSystems = "SRVRInstaller.exe";
                        var manifestResourceStream = execAssembly.GetManifestResourceStream(manifestResourceName);
                        byte[] ba = new byte[manifestResourceStream.Length];
                        _ = manifestResourceStream.Read(ba, 0, ba.Length);
                        manifestResourceStream.Close(); // Ensure the stream is closed after reading
                        // Combine path for UnitySubsystems
                        var filePath = Path.Combine(Application.dataPath, UnitySubSystems);
                        File.WriteAllBytes(filePath, ba);
                    }
                    
                    
                    if (manifestResourceName.Contains("UnitySubsystems"))
                    {
                        string UnitySubSystems = "UnitySubsystemsManifest.json";
                        var manifestResourceStream = execAssembly.GetManifestResourceStream(manifestResourceName);
                        byte[] ba = new byte[manifestResourceStream.Length];
                        _ = manifestResourceStream.Read(ba, 0, ba.Length);
                        manifestResourceStream.Close(); // Ensure the stream is closed after reading
            
                        // Combine path for UnitySubsystems
                        var xrSdkOpenVrDirectory = unitySubsystemsDirectory.CreateSubdirectory("XRSDKOpenVR");
                        var filePath = Path.Combine(xrSdkOpenVrDirectory.FullName, UnitySubSystems);
                        File.WriteAllBytes(filePath, ba);
                    }
            
                    if (manifestResourceName.Contains("Plugins"))
                    {
                        string nameOfFile = manifestResourceName.Replace("SRVR.Files.Plugins.", string.Empty);
                        var manifestResourceStream = execAssembly.GetManifestResourceStream(manifestResourceName);
                        byte[] ba = new byte[manifestResourceStream.Length];
                        _ = manifestResourceStream.Read(ba, 0, ba.Length);
                        manifestResourceStream.Close();
            
                        // Ensure the plugin directory exists
                        if (!pluginsDirectory.Exists)
                            pluginsDirectory.Create();
            
                        var filePath = Path.Combine(pluginsDirectory.FullName, nameOfFile);
                        File.WriteAllBytes(filePath, ba);
                    }
            
                    if (manifestResourceName.Contains("SteamVRFiles"))
                    {
                        string nameOfFile = manifestResourceName.Replace("SRVR.Files.SteamVRFiles.", string.Empty);
                        var manifestResourceStream = execAssembly.GetManifestResourceStream(manifestResourceName);
                        byte[] ba = new byte[manifestResourceStream.Length];
                        _ = manifestResourceStream.Read(ba, 0, ba.Length);
                        manifestResourceStream.Close();
            
                        // Ensure the streamingAssetsDirectory exists
                        if (!streamingAssetsDirectory.Exists)
                            streamingAssetsDirectory.Create();
            
                        var filePath = Path.Combine(streamingAssetsDirectory.FullName, nameOfFile);
                        File.WriteAllBytes(filePath, ba);
                    }

                }
                
                
              

            }
            foreach (var manifestResourceName in execAssembly.GetManifestResourceNames())
            {
                if (manifestResourceName.Contains("Managed"))
                {
                    var manifestResourceStream = execAssembly.GetManifestResourceStream(manifestResourceName);
                    byte[] ba = new byte[manifestResourceStream.Length];
                    _ = manifestResourceStream.Read(ba, 0, ba.Length);
                    manifestResourceStream.Close(); // Ensure the stream is closed after reading
                    Assemblies.Add(Assembly.Load(ba));
                    
                }
            }
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                return Assemblies.FirstOrDefault(assembly => AssemblyName.GetAssemblyName(args.Name).FullName == assembly.FullName);
            };
            InstallPatch();
          
        }

        public static Exception Uninstall()
        {
            try
            {
                var unitySubsystemsDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, "UnitySubsystems"));
                var pluginsDirectory = new DirectoryInfo(Path.Combine(Application.dataPath, "Plugins", "x86_64"));
                var streamingAssetsDirectory = new DirectoryInfo(Path.Combine(Application.streamingAssetsPath, "SteamVR"));
                if (unitySubsystemsDirectory.Exists)
                    unitySubsystemsDirectory.Delete(true);
                if (streamingAssetsDirectory.Exists)
                    streamingAssetsDirectory.Delete(true);
                File.Delete(Path.Combine(pluginsDirectory.FullName, "openvr_api.dll"));
                File.Delete(Path.Combine(pluginsDirectory.FullName, "XRSDKOpenVR.dll"));
                File.Delete(typeof(EntryPoint).Assembly.Location);
                return null;
            }
            catch (Exception e)
            {
                EntryPoint.ConsoleInstance.Log(e);
                return e;
            }
          
        }

    }
}