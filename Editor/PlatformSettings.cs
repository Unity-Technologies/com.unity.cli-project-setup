#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using com.unity.test.performance.runtimesettings;
#if OCULUS_SDK
using Unity.XR.Oculus;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.PackageManager;
#if URP
using UnityEngine.Rendering.Universal;
#endif
#if ENABLE_VR
using UnityEngine.XR;
#endif

namespace com.unity.cliprojectsetup
{
    public class PlatformSettings
    { 
#if UNITY_EDITOR
        public BuildTarget BuildTarget => EditorUserBuildSettings.activeBuildTarget;
        public BuildTargetGroup BuildTargetGroup => EditorUserBuildSettings.selectedBuildTargetGroup;
        public ScriptingImplementation ScriptingImplementation = ScriptingImplementation.IL2CPP;
        public ApiCompatibilityLevel ApiCompatibilityLevel = ApiCompatibilityLevel.NET_2_0;
        public AndroidArchitecture AndroidTargetArchitecture = AndroidArchitecture.ARM64;
        public ManagedStrippingLevel ManagedStrippingLevel;
#endif
        public GraphicsDeviceType PlayerGraphicsApi;
        public string PackageUnderTestName;
        public string PackageUnderTestVersion;
        public string PackageUnderTestRevision;
        public string PackageUnderTestRevisionDate;
        public string PackageUnderTestBranch;
        public ColorSpace ColorSpace = ColorSpace.Gamma;
        public bool EnableBurst = true;
        public bool GraphicsJobs;
        public bool MtRendering = true;
        public string RenderPipeline;
        public string TestsBranch;
        public string TestsRevision;
        public string TestsRevisionDate;
        public string Username;
        public string JobLink;
        public int JobWorkerCount = -1; // sentinel value indicating we don't want to set the JobWorkerCount
        public bool StringEngineCode;
        public bool ScriptDebugging;
        public string TestProjectName;
        public string TestProjectRevision;
        public string TestProjectRevisionDate;
        public string TestProjectBranch;
        public string EnabledXrTarget;
        public string StereoRenderingMode;
        public string StereoRenderingModeDesktop;
        public string StereoRenderingModeAndroid;
        public string SimulationMode;
        public string PluginVersion;
        public string XrTarget;
        public string DeviceRuntimeVersion;
        public string FfrLevel;

        private static readonly string resourceDir = "Assets/Resources";
        private static readonly string settingsAssetName = "/settings.asset";
        private readonly Regex revisionValueRegex = new Regex("\"revision\": \"([a-f0-9]*)\"",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex majorMinorVersionValueRegex = new Regex("([0-9]*\\.[0-9]*\\.)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        

        public void SerializeToAsset()
        {
            var settingsAsset = ScriptableObject.CreateInstance<CurrentSettings>();

            settingsAsset.PlayerGraphicsApi = PlayerGraphicsApi.ToString();
            settingsAsset.MtRendering = MtRendering;
            settingsAsset.GraphicsJobs = GraphicsJobs;
            settingsAsset.EnableBurst = EnableBurst;
            settingsAsset.ScriptingBackend = ScriptingImplementation.ToString();
            settingsAsset.ColorSpace = ColorSpace.ToString();
            settingsAsset.Username = Username = Environment.UserName;
            settingsAsset.PackageUnderTestName = PackageUnderTestName;
            settingsAsset.PackageUnderTestVersion = PackageUnderTestVersion;
            settingsAsset.PackageUnderTestRevision = PackageUnderTestRevision;
            settingsAsset.PackageUnderTestRevisionDate = PackageUnderTestRevisionDate;
            settingsAsset.PackageUnderTestBranch = PackageUnderTestBranch;
            settingsAsset.TestsRevision = TestsRevision;
            settingsAsset.TestsRevisionDate = TestsRevisionDate;
            settingsAsset.TestsBranch = TestsBranch;
            settingsAsset.JobLink = JobLink;
            settingsAsset.JobWorkerCount = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount;
            settingsAsset.ApiCompatibilityLevel = ApiCompatibilityLevel.ToString();
            settingsAsset.StripEngineCode = StringEngineCode;
            settingsAsset.ManagedStrippingLevel = ManagedStrippingLevel.ToString();
            settingsAsset.ScriptDebugging = ScriptDebugging;
            settingsAsset.TestProjectName = TestProjectName;
            settingsAsset.TestProjectRevision = TestProjectRevision;
            settingsAsset.TestProjectRevisionDate = TestProjectRevisionDate;
            settingsAsset.TestProjectBranch = TestProjectBranch;
            settingsAsset.EnabledXrTarget = EnabledXrTarget;
            settingsAsset.StereoRenderingMode = StereoRenderingMode;
            settingsAsset.StereoRenderingModeDesktop = StereoRenderingModeDesktop;
            settingsAsset.StereoRenderingModeAndroid = StereoRenderingModeAndroid;
            settingsAsset.SimulationMode = SimulationMode;
            settingsAsset.PluginVersion = PluginVersion;
            settingsAsset.DeviceRuntimeVersion = DeviceRuntimeVersion;
            settingsAsset.FfrLevel = FfrLevel;
            settingsAsset.AndroidTargetArchitecture = AndroidTargetArchitecture.ToString();
            settingsAsset.StereoRenderingMode = StereoRenderingMode;

            GetPackageUnderTestVersionInfo(settingsAsset);
            settingsAsset.RenderPipeline = RenderPipeline =  $"{(GraphicsSettings.renderPipelineAsset != null ? GraphicsSettings.renderPipelineAsset.name : "BuiltInRenderer")}";

#if URP
            settingsAsset.AntiAliasing = GraphicsSettings.renderPipelineAsset != null
                ? ((UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset).msaaSampleCount
                : QualitySettings.antiAliasing;
#else
            settingsAsset.AntiAliasing = QualitySettings.antiAliasing;
#endif

#if OCULUS_SDK
            // These fields are used by the performance test framework and are an artifact from this class
            // previously using the provider - specific enums before converting to a cross-platform friendly string
            if (BuildTarget == BuildTarget.Android)
            {
                settingsAsset.StereoRenderingModeAndroid = StereoRenderingPath;
            }
            else
            {
                settingsAsset.StereoRenderingModeDesktop = StereoRenderingPath;
            }
            
#if OCULUS_SDK_PERF
            settingsAsset.PluginVersion = string.Format("OculusPluginVersion|{0}", OculusStats.PluginVersion);
#endif
#endif
#if XR_SDK
            settingsAsset.StereoRenderingMode = StereoRenderingPath;
#else
            if (!string.IsNullOrEmpty(StereoRenderingMode))
            {
                // legacy xr has different enum for player settings and runtime settings for stereo rendering mode
                var builtInXrStereoPath = (StereoRenderingPath)Enum.Parse(typeof(StereoRenderingPath), StereoRenderingMode);
                settingsAsset.StereoRenderingMode = GetXrStereoRenderingPathMapping(builtInXrStereoPath).ToString();
            }
#endif
            SaveSettingsAssetOnStartup(settingsAsset);
        }

        private void GetPackageUnderTestVersionInfo(CurrentSettings settingsAsset)
        {
            var listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
            {
            }

            if (listRequest.Result.Any(r => r.name.Equals(PackageUnderTestName)))
            {
                var packageUnderTestInfo =
                    listRequest.Result.First(r => r.name.Equals(PackageUnderTestName));

                settingsAsset.PackageUnderTestVersion = packageUnderTestInfo.version;

                // if PackageRevision is empty, then it wasn't passed in on the command line (which is
                // usually going to be the case if we're running in tests at the PR level for the package).
                // In this case, we most likely are using a released package reference, so let's try to get
                // the revision from the package.json.
                settingsAsset.PackageUnderTestRevision = string.IsNullOrEmpty(PackageUnderTestRevision) ? 
                    TryGetRevisionFromPackageJson(PackageUnderTestRevision) 
                    : PackageUnderTestRevision;

                // if PackageUnderTestRevisionDate is empty, then it wasn't passed in on the command line (which is
                // usually going to be the case if we're running in tests at the PR level for the package).
                // In this case, we most likely are using a released package reference, so let's try to get
                // the revision date from the package manager api instead.
                settingsAsset.PackageUnderTestRevisionDate = string.IsNullOrEmpty(PackageUnderTestRevisionDate)
                    ? TryGetPackageUnderTestRevisionDate(packageUnderTestInfo.datePublished)
                    : PackageUnderTestRevisionDate;

                // if PackageUnderTestBranch is empty, then it wasn't passed in on the command line (which is
                // usually going to be the case if we're running in tests at the PR level for the package).
                // In this case, we most likely are using a released package reference, so let's try to infer
                // the branch from the major.minor version of the package via the package manager API
                settingsAsset.PackageUnderTestBranch = string.IsNullOrEmpty(PackageUnderTestBranch)
                    ? TryGetPackageUnderTestBranch(packageUnderTestInfo.version)
                    : PackageUnderTestBranch;
            }
        }

        private string TryGetPackageUnderTestBranch(string version)
        {
            var matches = majorMinorVersionValueRegex.Matches(version);
            return matches.Count > 0 ? string.Concat(matches[0].Groups[0].Value, "x") : "release";
        }

        private string TryGetPackageUnderTestRevisionDate(DateTime? datePublished)
        {
            return datePublished != null ?
                    ((DateTime)datePublished).ToString("s", DateTimeFormatInfo.InvariantInfo) : "unavailable";
        }

        private string TryGetRevisionFromPackageJson(string packageName)
        {
            string revision = null;
            var packageAsString = File.ReadAllText(string.Format("Packages/{0}/package.json", packageName));
            var matches = revisionValueRegex.Matches(packageAsString);
            if (matches.Count > 0)
            {
                revision = matches[0].Groups[1].Value;
            }

            return revision;
        }
#if UNITY_EDITOR
        public static void SaveSettingsAsset(CurrentSettings settingsAsset)
        {
            if (!Directory.Exists(resourceDir))
            {
                Directory.CreateDirectory(resourceDir);
            }
            if (!Resources.FindObjectsOfTypeAll<CurrentSettings>().Any())
            {
                AssetDatabase.CreateAsset(settingsAsset, resourceDir + settingsAssetName);
            }
            EditorUtility.SetDirty(settingsAsset);
            AssetDatabase.SaveAssets();
        }

        private void SaveSettingsAssetOnStartup(CurrentSettings settingsAsset)
        {
            if (!Directory.Exists(resourceDir))
            {
                Directory.CreateDirectory(resourceDir);
            }
            AssetDatabase.CreateAsset(settingsAsset, resourceDir + settingsAssetName);
            AssetDatabase.SaveAssets();
        }

#if ENABLE_VR
        private XRSettings.StereoRenderingMode GetXrStereoRenderingPathMapping(StereoRenderingPath stereoRenderingPath)
        {
            switch (stereoRenderingPath)
            {
                case UnityEditor.StereoRenderingPath.SinglePass:
                    return XRSettings.StereoRenderingMode.SinglePass;
                case UnityEditor.StereoRenderingPath.MultiPass:
                    return XRSettings.StereoRenderingMode.MultiPass;
                case UnityEditor.StereoRenderingPath.Instancing:
                    return XRSettings.StereoRenderingMode.SinglePassInstanced;
                default:
                    return XRSettings.StereoRenderingMode.SinglePassMultiview;
            }
        }
#endif
#endif
    }
}
#endif