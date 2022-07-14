#if UNITY_EDITOR
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using com.unity.test.metadatamanager;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
#if URP
using UnityEngine.Rendering.Universal;
#endif
#if ENABLE_VR
using UnityEngine.XR;
#endif

namespace com.unity.cliprojectsetup
{
    public class PlatformSettings : IPlatformSettings
    { 
#if UNITY_EDITOR
        public BuildTargetGroup BuildTargetGroup => EditorUserBuildSettings.selectedBuildTargetGroup;
        public BuildTarget BuildTarget => EditorUserBuildSettings.activeBuildTarget;
        public ScriptingImplementation? ScriptingImplementation;
        public ApiCompatibilityLevel? ApiCompatibilityLevel;
        public AndroidArchitecture AndroidTargetArchitecture = AndroidArchitecture.ARM64;
        public ManagedStrippingLevel ManagedStrippingLevel;
#endif
        private readonly string UnavailableMsg = "unavailable";
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
        public string StereoRenderingMode;
        public string StereoRenderingModeDesktop;
        public string StereoRenderingModeAndroid;
        public string SimulationMode;
        public string PluginVersion;
        public string XrTarget;
        public string DeviceRuntimeVersion;
        public string FfrLevel;
        public string Vsync;

        private readonly Regex revisionValueRegex = new Regex("\"revision\": \"([a-f0-9]*)\"",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex majorMinorVersionValueRegex = new Regex("([0-9]*\\.[0-9]*\\.)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void SerializeToAsset()
        {
            var settings = CustomMetadataManager.Instance;
            var pathParts = Application.dataPath.Split('/');
            settings.ProjectName = pathParts.Length >= 2 ? pathParts[pathParts.Length - 2] : string.Empty;
            settings.PlayerGraphicsApi = PlayerGraphicsApi.ToString();
            settings.MtRendering = MtRendering;
            settings.GraphicsJobs = GraphicsJobs;
            settings.EnableBurst = EnableBurst;
            settings.ScriptingBackend = ScriptingImplementation.ToString();
            settings.ColorSpace = ColorSpace.ToString();
            settings.Username = Username = Environment.UserName;
            settings.PackageUnderTestName = PackageUnderTestName;
            settings.PackageUnderTestVersion = PackageUnderTestVersion;
            settings.PackageUnderTestRevision = PackageUnderTestRevision;
            settings.PackageUnderTestRevisionDate = PackageUnderTestRevisionDate;
            settings.PackageUnderTestBranch = PackageUnderTestBranch;
            settings.TestsRevision = TestsRevision;
            settings.TestsRevisionDate = TestsRevisionDate;
            settings.TestsBranch = TestsBranch;
            settings.JobLink = JobLink;
            settings.JobWorkerCount = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount;
            settings.ApiCompatibilityLevel = ApiCompatibilityLevel.ToString();
            settings.StripEngineCode = StringEngineCode;
            settings.ManagedStrippingLevel = ManagedStrippingLevel.ToString();
            settings.ScriptDebugging = ScriptDebugging;
            settings.TestProjectName = TestProjectName;
            settings.TestProjectRevision = TestProjectRevision;
            settings.TestProjectRevisionDate = TestProjectRevisionDate;
            settings.TestProjectBranch = TestProjectBranch;
            settings.EnabledXrTarget = XrTarget;
            settings.StereoRenderingMode = StereoRenderingMode;
            settings.StereoRenderingModeDesktop = StereoRenderingModeDesktop;
            settings.StereoRenderingModeAndroid = StereoRenderingModeAndroid;
            settings.SimulationMode = SimulationMode;
            settings.PluginVersion = PluginVersion;
            settings.DeviceRuntimeVersion = DeviceRuntimeVersion;
            settings.FfrLevel = FfrLevel;
            settings.AndroidTargetArchitecture = AndroidTargetArchitecture.ToString();

            GetPackageUnderTestVersionInfo(settings);
            settings.RenderPipeline = RenderPipeline =  $"{(GraphicsSettings.renderPipelineAsset != null ? GraphicsSettings.renderPipelineAsset.name : "BuiltInRenderer")}";

#if URP
            settings.AntiAliasing = GraphicsSettings.renderPipelineAsset != null
                ? ((UniversalRenderPipelineAsset) GraphicsSettings.renderPipelineAsset).msaaSampleCount
                : QualitySettings.antiAliasing;
#else
            settings.AntiAliasing = QualitySettings.antiAliasing;
#endif

#if ENABLE_VR
#if OCULUS_SDK
            // These fields are used by the performance test framework and are an artifact from this class
            // previously using the provider - specific enums before converting to a cross-platform friendly string
            if (BuildTarget == BuildTarget.Android)
            {
                settings.StereoRenderingModeAndroid = StereoRenderingMode;
            }
            else
            {
                settings.StereoRenderingModeDesktop = StereoRenderingMode;
            }
            
#if OCULUS_SDK_PERF
            settings.PluginVersion = string.Format("OculusPluginVersion|{0}", OculusStats.PluginVersion);
#endif
#endif
#if XR_SDK
            settings.StereoRenderingMode = StereoRenderingMode;
#else
            if (!string.IsNullOrEmpty(StereoRenderingMode))
            {
                // legacy xr has different enum for player settings and runtime settings for stereo rendering mode
                var builtInXrStereoPath = (StereoRenderingPath)Enum.Parse(typeof(StereoRenderingPath), StereoRenderingMode);
                settings.StereoRenderingMode = GetXrStereoRenderingPathMapping(builtInXrStereoPath).ToString();
            }
#endif
#endif
            CustomMetadataManager.SaveSettingsAssetInEditor();
        }

        public void GetPackageUnderTestVersionInfo(CurrentSettings settings)
        {
            var packageManagerClient = new PackageManagerClient();

            if (!string.IsNullOrEmpty(PackageUnderTestName) && packageManagerClient.PackageUnderTestPresentInProject(PackageUnderTestName))
            {
                var packageUnderTestInfo =
                    packageManagerClient.GetPackageInfo(PackageUnderTestName);

                settings.PackageUnderTestVersion = packageUnderTestInfo.version;

                // if PackageRevision is empty, then it wasn't passed in on the command line (which is
                // usually going to be the case if we're running in tests at the PR level for the package).
                // In this case, we most likely are using a released package reference, so let's try to get
                // the revision from the package.json.
                settings.PackageUnderTestRevision = 
                    !string.IsNullOrEmpty(PackageUnderTestRevision) ? PackageUnderTestRevision :
                    !string.IsNullOrEmpty(TryGetRevisionFromPackageJson(PackageUnderTestName))  ? TryGetRevisionFromPackageJson(PackageUnderTestName) :
                    UnavailableMsg;

                // if PackageUnderTestRevisionDate is empty, then it wasn't passed in on the command line (which is
                // usually going to be the case if we're running in tests at the PR level for the package).
                // In this case, we most likely are using a released package reference, so let's try to get
                // the revision date from the package manager api instead.
                settings.PackageUnderTestRevisionDate = 
                    !string.IsNullOrEmpty(PackageUnderTestRevisionDate) ? PackageUnderTestRevisionDate :
                    packageUnderTestInfo.datePublished != null ? GetPackageUnderTestRevisionDate(packageUnderTestInfo.datePublished) : 
                    UnavailableMsg;

                // if PackageUnderTestBranch is empty, then it wasn't passed in on the command line (which is
                // usually going to be the case if we're running in tests at the PR level for the package).
                // In this case, we most likely are using a released package reference, so let's try to infer
                // the branch from the major.minor version of the package via the package manager API
                settings.PackageUnderTestBranch = 
                    !string.IsNullOrEmpty(PackageUnderTestBranch) ? PackageUnderTestBranch :
                    !string.IsNullOrEmpty(packageUnderTestInfo.version) ? GetPackageUnderTestBranch(packageUnderTestInfo.version) :
                    UnavailableMsg;
            }
            else
            {
                settings.PackageUnderTestRevision = UnavailableMsg;
                settings.PackageUnderTestVersion = UnavailableMsg;
                settings.PackageUnderTestRevisionDate = UnavailableMsg;
                settings.PackageUnderTestBranch = UnavailableMsg;
            }
        }

        public string GetPackageUnderTestBranch(string version)
        {
            var matches = majorMinorVersionValueRegex.Matches(version);
            return matches.Count > 0 ? string.Concat(matches[0].Groups[0].Value, "x") : "release";
        }

        public string GetPackageUnderTestRevisionDate(DateTime? datePublished)
        {
            return datePublished != null ?
                    ((DateTime)datePublished).ToString("s", DateTimeFormatInfo.InvariantInfo) : UnavailableMsg;
        }

        public string TryGetRevisionFromPackageJson(string packageName)
        {
            var revision = string.Empty;
            try
            {
                var packageAsString = File.ReadAllText($"Packages/{packageName}/package.json");
                var matches = revisionValueRegex.Matches(packageAsString);
                if (matches.Count > 0)
                {
                    revision = matches[0].Groups[1].Value;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return revision;
        }

#if UNITY_EDITOR && ENABLE_VR
        public XRSettings.StereoRenderingMode GetXrStereoRenderingPathMapping(StereoRenderingPath stereoRenderingPath)
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
#endif
    }
}
#endif
