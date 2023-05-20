#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NDesk.Options;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using ConfigureProject;
using UnityEditor.Build;

namespace com.unity.cliprojectsetup
{
    public class CliProjectSetup
    {
        private readonly string[] commandLineArgs;
        public readonly List<string> ScenesToAddToBuild = new List<string>();

        private readonly Regex customArgRegex = new Regex("-([^=]*)=", RegexOptions.Compiled);
        public PlatformSettings PlatformSettings = new PlatformSettings();

        private List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
        public string[] CliArgs;
        
        public CliProjectSetup(string[] args = null, PlatformSettings platformSettings = null)
        {
            if (args != null)
            {
                commandLineArgs = args;
            }

            if (platformSettings != null)
            {
                PlatformSettings = platformSettings;
            }
        }

        public void ConfigureFromCmdlineArgs()
        {
            ParseCommandLineArgs();
            ConfigureSettings();
            AddTestScenesToBuild();
            PlatformSettings.SerializeToAsset();
        }

        public virtual void AddTestScenesToBuild()
        {

            foreach (var sceneToAddToBuild in ScenesToAddToBuild)
            {
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(sceneToAddToBuild, true));
            }

            // Note, this completely replaces the list of buildSettingScenes currently in the project with only our tests buildSettingScenes.
            if (editorBuildSettingsScenes.Count != 0)
            {
                SetEditorBuildSettingsScenes(editorBuildSettingsScenes.ToArray());
            }
        }

        protected virtual void SetEditorBuildSettingsScenes(EditorBuildSettingsScene[] buildSettingScenes)
        {
            EditorBuildSettings.scenes = buildSettingScenes;
        }

        public virtual void ParseCommandLineArgs()
        {
            CliArgs = commandLineArgs ?? Environment.GetCommandLineArgs();
            EnsureOptionsLowerCased();
            var optionSet = DefineOptionSet();
            var unParsedArgs = optionSet.Parse(CliArgs);
        }

        public void EnsureOptionsLowerCased()
        {
            for (var i = 0; i < CliArgs.Length; i++)
            {
                if (customArgRegex.IsMatch(CliArgs[i]))
                {
                    CliArgs[i] = customArgRegex.Replace(CliArgs[i], customArgRegex.Matches(CliArgs[i])[0].ToString().ToLower());
                }
            }
        }

        public virtual void ConfigureSettings()
        {
            ConfigureCrossplatformSettings();
            // If Android, setup Android player settings
            if (PlatformSettings.BuildTarget == BuildTarget.Android)
            {
                ConfigureAndroidSettings();
            }

            // If iOS, setup iOS player settings
            if (PlatformSettings.BuildTarget == BuildTarget.iOS)
            {
                ConfigureIosSettings();
            }

            if (!string.IsNullOrEmpty(PlatformSettings.XrTarget))
            {
                ConfigureXrSettings();
            }
        }

        public virtual void ConfigureXrSettings()
        {
#if XR_SDK
            XRPlatformSettings<PlatformSettings>.Configure(PlatformSettings);
#endif
        }

        public virtual void ConfigureIosSettings()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, string.Format("com.unity3d.{0}", PlayerSettings.productName));
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Development;
        }

        public virtual void ConfigureAndroidSettings()
        {

            // AndroidTargetArchitecture has a default value of ARM64 and ScriptingImplementation is nullable. Further ARMv7 is only compatible with Mono2x.
            // Given baseline assumption and constraints, we want to help the user avoid issues as much as we can here

            // If AndroidTargetArchitecture is ARMv7, it's because the user passed it in on the CLI (ARM64 is the default in PlatformSettings). Further,
            // if ScriptingImplementation is IL2CPP, it's also because the user passed it in on the CLI, but these are incompatible, but most likely the
            // user didn't know that, so let's help them out but setting ScriptingImplementation to Mono2x.
            if (PlatformSettings.AndroidTargetArchitecture == AndroidArchitecture.ARMv7 &&
                PlatformSettings.ScriptingImplementation != ScriptingImplementation.Mono2x)
            {
                SetScriptingImplementationToMono2x();
                SetPlayerSettingsAndroidScriptingBackend();
            }

            // If ScriptingImplementation is Mono, it's because the user passed it in on the CLI. Further,
            // if AndroidArchitecture is NOT ARMv7, it's most likely because they didn't specify an AndroidArchitecture and it took it's default value of ARM64,
            // but these are incompatible, so let's help them out but setting AndroidArchitecture to ARMv7.
            if (PlatformSettings.ScriptingImplementation == ScriptingImplementation.Mono2x &&
                PlatformSettings.AndroidTargetArchitecture != AndroidArchitecture.ARMv7)
            {
                SetAndroidTargetArchitectureToARMv7();
            }

            SetPlayerSettingsAndroidTargetArchitectures();
        }

        public virtual void SetAndroidTargetArchitectureToARMv7()
        {
            PlatformSettings.AndroidTargetArchitecture = AndroidArchitecture.ARMv7;
        }

        public virtual void SetScriptingImplementationToMono2x()
        {
            PlatformSettings.ScriptingImplementation = ScriptingImplementation.Mono2x;
        }

        public virtual void SetPlayerSettingsAndroidScriptingBackend()
        {
            PlayerSettings.SetScriptingBackend(
                NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup),
                (ScriptingImplementation)PlatformSettings.ScriptingImplementation);
        }

        public virtual void SetPlayerSettingsAndroidTargetArchitectures()
        {
            PlayerSettings.Android.targetArchitectures = PlatformSettings.AndroidTargetArchitecture;
        }

        public virtual void ConfigureCrossplatformSettings()
        {
            if (PlatformSettings.PlayerGraphicsApi != GraphicsDeviceType.Null)
            {
                PlayerSettings.SetUseDefaultGraphicsAPIs(PlatformSettings.BuildTarget, false);
                PlayerSettings.SetGraphicsAPIs(PlatformSettings.BuildTarget,
                    new[] {PlatformSettings.PlayerGraphicsApi});
            }

            // Default to no vsync for performance tests

            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i);
                QualitySettings.vSyncCount = !string.IsNullOrEmpty(PlatformSettings.Vsync) ? Convert.ToInt32(PlatformSettings.Vsync) : 0;
            }
            
            PlayerSettings.graphicsJobs = PlatformSettings.GraphicsJobs;
            PlayerSettings.MTRendering = PlatformSettings.MtRendering;
            PlayerSettings.colorSpace = PlatformSettings.ColorSpace;
            TrySetScriptingBackend();
            TrySetApiCompatabilityLevel();
            PlayerSettings.stripEngineCode = PlatformSettings.StripEngineCode;
            PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup), PlatformSettings.ManagedStrippingLevel);
            EditorUserBuildSettings.allowDebugging = PlatformSettings.ScriptDebugging;

            if (PlatformSettings.JobWorkerCount >= 0)
            {
                SetJobWorkerCount();
            }
        }

        public virtual void SetJobWorkerCount()
        {
            Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount = PlatformSettings.JobWorkerCount;
        }

        public virtual void TrySetApiCompatabilityLevel()
        {
            if (PlatformSettings.ApiCompatibilityLevel != null)
            {
                SetApiCompatabilityLevel();
            }
        }

        public virtual void SetApiCompatabilityLevel()
        {
            PlayerSettings.SetApiCompatibilityLevel(
                NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup),
                (ApiCompatibilityLevel)PlatformSettings.ApiCompatibilityLevel);
        }

        public virtual void TrySetScriptingBackend()
        {
            if (PlatformSettings.ScriptingImplementation != null)
            {
                SetScriptingBackend();
            }
        }

        public virtual void SetScriptingBackend()
        {
            PlayerSettings.SetScriptingBackend(
                NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup),
                (ScriptingImplementation)PlatformSettings.ScriptingImplementation);
        }

        private OptionSet DefineOptionSet()
        {
            var optionsSet = new OptionSet();
            optionsSet.Add("scripting-backend|scriptingbackend=",
                "Scripting backend to use. Values: IL2CPP, Mono", ParseScriptingBackend);
            optionsSet.Add("playergraphicsapi=", "Graphics API based on GraphicsDeviceType.",
                graphicsDeviceType =>
                    PlatformSettings.PlayerGraphicsApi = ParseEnum<GraphicsDeviceType>(graphicsDeviceType));
            optionsSet.Add("colorspace=", "Linear or Gamma color space.",
                colorSpace => PlatformSettings.ColorSpace = ParseEnum<ColorSpace>(colorSpace));
            optionsSet.Add("mtRendering",
                "Enable or disable multithreaded rendering. Enabled is default. Use option to enable, or use option and append '-' to disable.",
                option => PlatformSettings.MtRendering = option != null);
            optionsSet.Add("graphicsJobs",
                "Enable graphics jobs rendering. Disabled is default. Use option to enable, or use option and append '-' to disable.",
                option => PlatformSettings.GraphicsJobs = option != null);
            optionsSet.Add("testsrev=",
                "revision id of the tests being used.",
                testsrev => PlatformSettings.TestsRevision = testsrev);
            optionsSet.Add("testsrevdate=",
                "revision date of the tests being used.",
                revDate => PlatformSettings.TestsRevisionDate = revDate);
            optionsSet.Add("testsbranch=",
                "branch of the tests repo being used.",
                testsbranch => PlatformSettings.TestsBranch = testsbranch);
            optionsSet.Add("enableburst",
                "Enable burst. Enabled is default. Use option to disable: -enableburst",
                option => PlatformSettings.EnableBurst = option != null);
            optionsSet.Add("packageundertestname=",
                "package under test commit revision id",
                packageundertestname => PlatformSettings.PackageUnderTestName = packageundertestname);
            optionsSet.Add("packageundertestversion=",
                "package under test version",
                packageundertestrev => PlatformSettings.PackageUnderTestVersion = packageundertestrev);
            optionsSet.Add("packageundertestrev=",
                "package under test commit revision id",
                packageundertestrev => PlatformSettings.PackageUnderTestRevision = packageundertestrev);
            optionsSet.Add("packageundertestrevdate=",
                "package under test commit revision date",
                packageundertestrevdate => PlatformSettings.PackageUnderTestRevisionDate = packageundertestrevdate);
            optionsSet.Add("packageundertestbranch=",
                "branch of the package under test repo being used.",
                packageundertestbranch => PlatformSettings.PackageUnderTestBranch = packageundertestbranch);
            optionsSet.Add("testprojectname=",
                "test project commit revision id",
                testprojectname => PlatformSettings.TestProjectName = testprojectname);
            optionsSet.Add("testprojectrevision=",
                "test project commit revision id",
                testprojectrevision => PlatformSettings.TestProjectRevision = testprojectrevision);
            optionsSet.Add("testprojectrevdate=",
                "test project commit revision date",
                testprojectrevdate => PlatformSettings.TestProjectRevisionDate = testprojectrevdate);
            optionsSet.Add("testprojectbranch=",
                "branch of the test project repo being used.",
                testprojectbranch => PlatformSettings.TestProjectBranch = testprojectbranch);
            optionsSet.Add("joblink=",
                "Hyperlink to test job.",
                joblink => PlatformSettings.JobLink = joblink);
            optionsSet.Add("jobworkercount=",
                "Number of job workers to use. Range is 0 - number of cores minus 1.",
                jobworkercount =>
                {
                    if (int.TryParse(jobworkercount, out var count))
                    {
                        PlatformSettings.JobWorkerCount = count;
                    }
                });
            optionsSet.Add("apicompatibilitylevel=", "API compatibility to use. Default is NET_2_0",
                apicompatibilitylevel => PlatformSettings.ApiCompatibilityLevel =
                    ParseEnum<ApiCompatibilityLevel>(apicompatibilitylevel));
            optionsSet.Add("stripenginecode",
                "Enable Engine code stripping. Disabled is default. Use option to enable, or use option and append '-' to disable.",
                option => PlatformSettings.StripEngineCode = option != null);
            optionsSet.Add("managedstrippinglevel=", "Managed stripping level to use. Default is Disabled",
                managedstrippinglevel => PlatformSettings.ManagedStrippingLevel =
                    ParseEnum<ManagedStrippingLevel>(managedstrippinglevel));
            optionsSet.Add("scriptdebugging",
                "Enable scriptdebugging. Disabled is default. Use option to enable, or use option and append '-' to disable.",
                scriptdebugging => PlatformSettings.ScriptDebugging = scriptdebugging != null);
            optionsSet.Add("addscenetobuild=",
                "Specify path to scene to add to the build, Path is relative to Assets folder.",
                AddSceneToBuildList);
                optionsSet.Add("openxrfeatures=",
                "Add array of feature names to enable for openxr. ex [r:MockRuntime,OculusQuestFeature] should be name of feature class. Add r: before the feature name to make it required. Required features will fail the job if not found",
                features => PlatformSettings.OpenXRFeatures = features);
            optionsSet.Add("enabledxrtarget|enabledxrtargets=",
                "XR target to enable in player settings. Values: " +
                "\r\n\"Oculus\"\r\n\"OpenVR\"\r\n\"cardboard\"\r\n\"daydream\"\r\n\"MockHMD\"\r\n\"OculusXRSDK\"\r\n\"MockHMDXRSDK\"\r\n\"MagicLeapXRSDK\"\r\n\"WindowsMRXRSDK\"\r\n\"PSVR2\"",
                xrTarget => PlatformSettings.XrTarget = xrTarget);
            optionsSet.Add("stereorenderingmode|stereorenderingpath=", "Stereo rendering mode to enable. SinglePass is default.",
                srm => PlatformSettings.StereoRenderingMode = srm);
            optionsSet.Add("simulationmode=",
                "Enable Simulation modes for Windows MR in Editor. Values: \r\n\"HoloLens\"\r\n\"WindowsMR\"\r\n\"Remoting\"",
                simMode => PlatformSettings.SimulationMode = simMode);
            optionsSet.Add("deviceruntimeversion=",
                "runtime version of the device we're running on.",
                deviceruntime => PlatformSettings.DeviceRuntimeVersion = string.Format("deviceruntimeversion|{0}", deviceruntime));
            optionsSet.Add("ffrlevel=",
                "ffr level we're running at",
                ffrlevel => PlatformSettings.FfrLevel = string.Format("ffrlevel|{0}", ffrlevel));
            optionsSet.Add("enablefoveatedrendering", "enable foveated rendering", foveatedrendering => PlatformSettings.FoveatedRendering = foveatedrendering != null);
            optionsSet.Add("androidtargetarchitecture=",
                "Android Target Architecture to use.",
                androidtargetarchitecture => PlatformSettings.AndroidTargetArchitecture = ParseEnum<AndroidArchitecture>(androidtargetarchitecture));
            optionsSet.Add("vsync=",
                "test project commit revision id", vsync => PlatformSettings.Vsync = vsync);
            return optionsSet;
        }


        private void AddSceneToBuildList(string scene)
        {
            if (!string.IsNullOrEmpty(scene))
            {
                var cleanScene = scene.Replace("\"", string.Empty);
                var sceneName = cleanScene.ToLower().StartsWith("assets/") ? cleanScene : "Assets/" + cleanScene;
                if (!ScenesToAddToBuild.Contains(sceneName))
                {
                    ScenesToAddToBuild.Add(sceneName);
                }
            }
        }

        public static T ParseEnum<T>(string stringToParse)
        {
            T thisType;
            try
            {
                thisType = (T) Enum.Parse(typeof(T), stringToParse);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Couldn't cast {stringToParse} to {typeof(T)}", e);
            }

            return thisType;
        }

        private void ParseScriptingBackend(string scriptingBackend)
        {
            if (scriptingBackend.ToLower().StartsWith("mono"))
                PlatformSettings.ScriptingImplementation = ScriptingImplementation.Mono2x;
            else
                PlatformSettings.ScriptingImplementation = ScriptingImplementation.IL2CPP;
        }
    }
}
#endif
