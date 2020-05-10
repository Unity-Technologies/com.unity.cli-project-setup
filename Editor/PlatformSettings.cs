#if OCULUS_SDK
using Unity.XR.Oculus;
#endif
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using com.unity.xr.test.runtimesettings;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.PackageManager;
#if URP
using UnityEngine.Rendering.Universal;
#endif
#endif
using UnityEngine;
using UnityEngine.Rendering;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;
#if ENABLE_VR
using UnityEngine.XR;
#endif


namespace com.unity.cliconfigmanager
{
    public class PlatformSettings
    {
#if UNITY_EDITOR
        public BuildTargetGroup BuildTargetGroup => EditorUserBuildSettings.selectedBuildTargetGroup;
        public BuildTarget BuildTarget => EditorUserBuildSettings.activeBuildTarget;

        public string XrTarget;
        public GraphicsDeviceType PlayerGraphicsApi;

#if OCULUS_SDK
        public OculusSettings.StereoRenderingModeDesktop StereoRenderingModeDesktop;
        public OculusSettings.StereoRenderingModeAndroid StereoRenderingModeAndroid;
#else
        public StereoRenderingPath StereoRenderingPath;
#endif
        public bool MtRendering = true;
        public bool GraphicsJobs;
        public AndroidSdkVersions MinimumAndroidSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
        public AndroidSdkVersions TargetAndroidSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        public ScriptingImplementation ScriptingImplementation = ScriptingImplementation.IL2CPP;
        public string AppleDeveloperTeamId;
        public string IOsProvisioningProfileId;
        public ColorSpace ColorSpace = ColorSpace.Gamma;
        public string XrsdkRevision;
        public string XrsdkRevisionDate;
        public string XrsdkBranch;
        public string TestsRevision;
        public string TestsRevisionDate;
        public string TestsBranch;
        public string DeviceRuntimeVersion;
        public string SimulationMode;
        public string Username;
        public string RenderPipeline;
        public string FfrLevel;
        public AndroidArchitecture AndroidTargetArchitecture = AndroidArchitecture.ARM64;

        private readonly string resourceDir = "Assets/Resources";
        private readonly string xrManagementPackageName = "com.unity.xr.management";
        private readonly string perfTestsPackageName = "xr.sdk.oculus.performancetests";
        private readonly string urpPackageName = "com.unity.render-pipelines.universal";
        private readonly string oculusXrSdkPackageName = "com.unity.xr.oculus";
        private readonly string hdrpPackageName = "com.unity.testing.hdrp";

        private readonly Regex revisionValueRegex = new Regex("\"revision\": \"([a-f0-9]*)\"",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex majorMinorVersionValueRegex = new Regex("([0-9]*\\.[0-9]*\\.)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);


        public void SerializeToAsset()
        {
            var settingsAsset = ScriptableObject.CreateInstance<CurrentSettings>();

            settingsAsset.SimulationMode = SimulationMode;
            settingsAsset.PlayerGraphicsApi = PlayerGraphicsApi.ToString();
            settingsAsset.MtRendering = MtRendering;
            settingsAsset.GraphicsJobs = GraphicsJobs;
            settingsAsset.ColorSpace = ColorSpace.ToString();
            settingsAsset.EnabledXrTarget = XrTarget;
            settingsAsset.XrsdkRevision = GetOculusXrSdkPackageVersionInfo();
            settingsAsset.XrManagementRevision = GetXrManagementPackageVersionInfo();
            settingsAsset.UrpPackageVersionInfo = GetUrpPackageVersionInfo();
            settingsAsset.HdrpPackageVersionInfo = GetHdrpPackageVersionInfo();
            settingsAsset.PerfTestsPackageRevision = GetPerfTestsPackageVersionInfo();
            settingsAsset.DeviceRuntimeVersion = DeviceRuntimeVersion;
            settingsAsset.Username = Username = Environment.UserName;
            settingsAsset.FfrLevel = FfrLevel;
            settingsAsset.TestsRevision = TestsRevision;
            settingsAsset.TestsRevisionDate = TestsRevisionDate;
            settingsAsset.TestsBranch = TestsBranch;
            settingsAsset.AndroidTargetArchitecture = string.Format("AndroidTargetArchitecture|{0}", AndroidTargetArchitecture.ToString());
            settingsAsset.RenderPipeline = RenderPipeline =
                $"renderpipeline|{(GraphicsSettings.renderPipelineAsset != null ? GraphicsSettings.renderPipelineAsset.name : "BuiltInRenderer")}";

#if URP
            settingsAsset.AntiAliasing = GraphicsSettings.renderPipelineAsset != null
                ? ((UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset).msaaSampleCount
                : QualitySettings.antiAliasing;
#else
            settingsAsset.AntiAliasing = QualitySettings.antiAliasing;
#endif
            

#if OCULUS_SDK
        settingsAsset.StereoRenderingModeDesktop = StereoRenderingModeDesktop.ToString();
            settingsAsset.StereoRenderingModeAndroid = StereoRenderingModeAndroid.ToString();
#if OCULUS_SDK_PERF
            settingsAsset.PluginVersion = string.Format("OculusPluginVersion|{0}", OculusStats.PluginVersion);
#endif
#else
            settingsAsset.StereoRenderingMode = GetXrStereoRenderingPathMapping(StereoRenderingPath).ToString();
#endif
            CreateAndSaveCurrentSettingsAsset(settingsAsset);
        }

        private string GetXrManagementPackageVersionInfo()
        {
            string packageRevision = string.Empty;

            var listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
            {
            }

            if (listRequest.Result.Any(r => r.name.Equals(xrManagementPackageName)))
            {
                var xrManagementPckg =
                    listRequest.Result.First(r => r.name.Equals(xrManagementPackageName));

                var revision = TryGetRevisionFromPackageJson(xrManagementPackageName) ?? "unavailable";
                var version = xrManagementPckg.version;
                packageRevision = string.Format("XrManagementPackageName|{0}|XrManagementVersion|{1}|XrManagementRevision|{2}", xrManagementPackageName, version, revision);
            }

            return packageRevision;
        }

        private string GetUrpPackageVersionInfo()
        {
            string packageRevision = string.Empty;

            var listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
            {
            }

            if (listRequest.Result.Any(r => r.name.Equals(urpPackageName)))
            {
                var urpPckg =
                    listRequest.Result.First(r => r.name.Equals(urpPackageName));

                var revision = TryGetRevisionFromPackageJson(urpPackageName) ?? "unavailable";
                var version = urpPckg.version;
                packageRevision = string.Format("UrpPackageName|{0}|UrpVersion|{1}|UrpRevision|{2}", urpPackageName, version, revision);
            }

            return packageRevision;
        }

        private string GetHdrpPackageVersionInfo()
        {
            string packageRevision = string.Empty;

            var listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
            {
            }

            if (listRequest.Result.Any(r => r.name.Equals(hdrpPackageName)))
            {
                var urpPckg =
                    listRequest.Result.First(r => r.name.Equals(hdrpPackageName));

                var revision = TryGetRevisionFromPackageJson(hdrpPackageName) ?? "unavailable";
                var version = urpPckg.version;
                packageRevision = string.Format("HdrpPackageName|{0}|HdrpVersion|{1}|HdrpRevision|{2}", hdrpPackageName, version, revision);
            }

            return packageRevision;
        }

        private string GetPerfTestsPackageVersionInfo()
        {
            string packageRevision = string.Empty;

            var listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
            {
            }

            if (listRequest.Result.Any(r => r.name.Equals(perfTestsPackageName)))
            {
                var perfTestsPckg =
                    listRequest.Result.First(r => r.name.Equals(perfTestsPackageName));

                var revision = TryGetRevisionFromPackageJson(perfTestsPackageName) ?? "unavailable";
                var version = perfTestsPckg.version;
                packageRevision = string.Format("PerfTestsPackageName|{0}|PerfTestsVersion|{1}|PerfTestsRevision|{2}", perfTestsPackageName, version, revision);
            }

            return packageRevision;
        }

        private string GetOculusXrSdkPackageVersionInfo()
        {
            string packageRevision = string.Empty;

            var listRequest = Client.List(true);
            while (!listRequest.IsCompleted)
            {
            }

            if (listRequest.Result.Any(r => r.name.Equals(oculusXrSdkPackageName)))
            {
                var oculusXrsdkPckg =
                    listRequest.Result.First(r => r.name.Equals(oculusXrSdkPackageName));

                var version = oculusXrsdkPckg.version;

                // if XrsdkRevision is empty, then it wasn't passed in on the command line (which is
                // usually going to be the case if we're running in tests at the PR level for Xrsdk package).
                // In this case, we most likely are using a released package reference, so let's try to get
                // the revision from the package.json.
                if (string.IsNullOrEmpty(XrsdkRevision))
                {
                    XrsdkRevision = TryGetRevisionFromPackageJson(oculusXrSdkPackageName) ?? "unavailable";
                }

                // if XrsdkRevisionDate is empty, then it wasn't passed in on the command line (which is
                // usually going to be the case if we're running in tests at the PR level for Xrsdk package).
                // In this case, we most likely are using a released package reference, so let's try to get
                // the revision date from the package manager api instead.
                if (string.IsNullOrEmpty(XrsdkRevisionDate))
                {
                    TryGetXrsdkRevisionDate(oculusXrsdkPckg);
                }

                // if XrsdkBranch is empty, then it wasn't passed in on the command line (which is
                // usually going to be the case if we're running in tests at the PR level for Xrsdk package).
                // In this case, we most likely are using a released package reference, so let's try to infer
                // the branch from the major.minor version of the package via the package manager API
                if (string.IsNullOrEmpty(XrsdkBranch))
                {
                    TryGetXrsdkBranch(oculusXrsdkPckg);
                }
                packageRevision = string.Format(
                    "XrSdkName|{0}|XrSdkVersion|{1}|XrSdkRevision|{2}|XrSdkRevisionDate|{3}|XrSdkBranch|{4}", 
                    oculusXrSdkPackageName, 
                    version, 
                    XrsdkRevision, 
                    XrsdkRevisionDate, 
                    XrsdkBranch);
            }

            return packageRevision;
        }

        private void TryGetXrsdkBranch(PackageInfo oculusXrsdkPckg)
        {
            var matches = majorMinorVersionValueRegex.Matches(oculusXrsdkPckg.version);
            XrsdkBranch = matches.Count > 0 ? string.Concat(matches[0].Groups[0].Value, "x") : "release";
        }

        private void TryGetXrsdkRevisionDate(PackageInfo oculusXrsdkPckg)
        {
#if OCULUS_SDK
            XrsdkRevisionDate = 
                oculusXrsdkPckg.datePublished != null ? 
                    ((DateTime) oculusXrsdkPckg.datePublished).ToString("s", DateTimeFormatInfo.InvariantInfo) : "unavailable";
#endif
        }

        private string TryGetRevisionFromPackageJson(string packageName)
        {
            string revision = null;
            var packageAsString = File.ReadAllText(string.Format("Packages/{0}/package.json",packageName));
            var matches = revisionValueRegex.Matches(packageAsString);
            if (matches.Count > 0)
            {
                revision = matches[0].Groups[1].Value;
            }

            return revision;
        }

        private XRSettings.StereoRenderingMode GetXrStereoRenderingPathMapping(StereoRenderingPath stereoRenderingPath)
        {
            switch (stereoRenderingPath)
            {
                case StereoRenderingPath.SinglePass:
                    return XRSettings.StereoRenderingMode.SinglePass;
                case StereoRenderingPath.MultiPass:
                    return XRSettings.StereoRenderingMode.MultiPass;
                case StereoRenderingPath.Instancing:
                    return XRSettings.StereoRenderingMode.SinglePassInstanced;
                default:
                    return XRSettings.StereoRenderingMode.SinglePassMultiview;
            }
        }

        private void CreateAndSaveCurrentSettingsAsset(CurrentSettings settingsAsset)
        {
            if (!System.IO.Directory.Exists(resourceDir))
            {
                System.IO.Directory.CreateDirectory(resourceDir);
            }

            AssetDatabase.CreateAsset(settingsAsset, resourceDir + "/settings.asset");
            AssetDatabase.SaveAssets();
        }
#endif
    }
}