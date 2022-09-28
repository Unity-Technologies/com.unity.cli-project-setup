#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NDesk.Options;
/* TODO: Revisit burst logic when we're using it
#if ENABLE_BURST_AOT
using Unity.Burst;
#endif
*/
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using ConfigureProject;

namespace com.unity.cliprojectsetup
{
    public class CliProjectSetup
    {
        public List<string> ScenesToAddToBuild = new List<string>();

        public readonly List<string> ScriptDefines = new List<string>();

        private readonly Regex customArgRegex = new Regex("-([^=]*)=", RegexOptions.Compiled);
        private readonly PlatformSettings platformSettings = new PlatformSettings();

        public void ConfigureFromCmdlineArgs()
        {
            ParseCommandLineArgs();
            ConfigureSettings();
            AddTestScenesToBuild(ScenesToAddToBuild);
            platformSettings.SerializeToAsset();
        }

        public static void AddTestScenesToBuild(List<string> scenesToAddToBuild)
        {

            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
            foreach (var sceneToAddToBuild in scenesToAddToBuild)
            {
                editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(sceneToAddToBuild, true));
            }

            // Note, this completely replaces the list of scenes currently in the project with only our tests scenes.
            if (editorBuildSettingsScenes.Count != 0)
            {
                EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
            }
        }

        public void ParseCommandLineArgs()
        {
            var args = Environment.GetCommandLineArgs();
            EnsureOptionsLowerCased(args);
            var optionSet = DefineOptionSet();
            var unParsedArgs = optionSet.Parse(args);
        }

        private void EnsureOptionsLowerCased(string[] args)
        {
            for (var i = 0; i < args.Length; i++)
            {
                if (customArgRegex.IsMatch(args[i]))
                {
                    args[i] = customArgRegex.Replace(args[i], customArgRegex.Matches(args[i])[0].ToString().ToLower());
                }
            }
        }

        private void ConfigureSettings()
        {
            ConfigureCrossplatformSettings();
            // If Android, setup Android player settings
            if (platformSettings.BuildTarget == BuildTarget.Android)
            {
                ConfigureAndroidSettings();
            }

            // If iOS, setup iOS player settings
            if (platformSettings.BuildTarget == BuildTarget.iOS)
            {
                ConfigureIosSettings();
            }

            if (!string.IsNullOrEmpty(platformSettings.XrTarget))
            {
                XRPlatformSettings<PlatformSettings>.Configure(platformSettings);
            }
        }

        private void ConfigureIosSettings()
        {
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, string.Format("com.unity3d.{0}", PlayerSettings.productName));
            PlayerSettings.iOS.appleEnableAutomaticSigning = false;
            PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Development;
        }

        private void ConfigureAndroidSettings()
        {
            // If the user has specified AndroidArchitecture.ARMv7, but not specified ScriptingImplementation.Mono2x, or has incorrectly specified ScriptingImplementation.IL2CPP (not supported
            // with mono), then set to AndroidArchitecture.ARMv7 so we're in a compatible configuration state.
            if (platformSettings.AndroidTargetArchitecture == AndroidArchitecture.ARMv7 &&
                platformSettings.ScriptingImplementation != ScriptingImplementation.Mono2x)
            {
                platformSettings.ScriptingImplementation = ScriptingImplementation.Mono2x;
                PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup,
                    (ScriptingImplementation) platformSettings.ScriptingImplementation);
            }

            // If the user has specified mono scripting backend, but not specified AndroidArchitecture.ARMv7, or has incorrectly specified AndroidArchitecture.ARM64 (not supported
            // with mono), then set to AndroidArchitecture.ARMv7 so we're in a compatible configuration state.
            if (platformSettings.ScriptingImplementation == ScriptingImplementation.Mono2x &&
                platformSettings.AndroidTargetArchitecture != AndroidArchitecture.ARMv7)
            {
                platformSettings.AndroidTargetArchitecture = AndroidArchitecture.ARMv7;
            }
            PlayerSettings.Android.targetArchitectures = platformSettings.AndroidTargetArchitecture;
        }

        private void ConfigureCrossplatformSettings()
        {
            if (platformSettings.PlayerGraphicsApi != GraphicsDeviceType.Null)
            {
                PlayerSettings.SetUseDefaultGraphicsAPIs(platformSettings.BuildTarget, false);
                PlayerSettings.SetGraphicsAPIs(platformSettings.BuildTarget,
                    new[] {platformSettings.PlayerGraphicsApi});
            }

            // Default to no vsync for performance tests

            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                QualitySettings.SetQualityLevel(i);
                QualitySettings.vSyncCount = !string.IsNullOrEmpty(platformSettings.Vsync) ? Convert.ToInt32(platformSettings.Vsync) : 0;
            }
           
            //QualitySettings.vSyncCount = !string.IsNullOrEmpty(platformSettings.Vsync) ? Convert.ToInt32(platformSettings.Vsync) : 0;
            PlayerSettings.graphicsJobs = platformSettings.GraphicsJobs;
            PlayerSettings.MTRendering = platformSettings.MtRendering;
            PlayerSettings.colorSpace = platformSettings.ColorSpace;
            if (platformSettings.ScriptingImplementation != null)
            {
                PlayerSettings.SetScriptingBackend(
                    EditorUserBuildSettings.selectedBuildTargetGroup,
                    (ScriptingImplementation)platformSettings.ScriptingImplementation);
            }
                
            if (platformSettings.ApiCompatibilityLevel != null)
            {
                PlayerSettings.SetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup, (ApiCompatibilityLevel) platformSettings.ApiCompatibilityLevel);
            }
            
            PlayerSettings.stripEngineCode = platformSettings.StringEngineCode;
            PlayerSettings.SetManagedStrippingLevel(EditorUserBuildSettings.selectedBuildTargetGroup, platformSettings.ManagedStrippingLevel);
            EditorUserBuildSettings.allowDebugging = platformSettings.ScriptDebugging;
            /* TODO: Revisit burst logic when we're using it
            #if ENABLE_BURST_AOT
                        BurstCompiler.Options.EnableBurstCompilation = platformSettings.EnableBurst;
            #endif
            */
            if (platformSettings.JobWorkerCount >= 0)
            {
                try
                {
                    Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount = platformSettings.JobWorkerCount;
                }
                catch (Exception e)
                {
                    // If we try to set the JobWorkerCount to more than the number of cores - 1 for a given machine,
                    // an exception is thrown. In this case, catch the exception and just use the default JobWorkerCount,
                    // then save this as the value used in our platformSettings.
                    Debug.Log($"Exception caught while trying to set JobWorkerCount to {platformSettings.JobWorkerCount}. Exception: {e.Message}");
                    platformSettings.JobWorkerCount = Unity.Jobs.LowLevel.Unsafe.JobsUtility.JobWorkerCount;
                }

            }
        }

        private OptionSet DefineOptionSet()
        {
            var optionsSet = new OptionSet();
            optionsSet.Add("scripting-backend|scriptingbackend=",
                "Scripting backend to use. IL2CPP is default. Values: IL2CPP, Mono", ParseScriptingBackend);
            optionsSet.Add("playergraphicsapi=", "Graphics API based on GraphicsDeviceType.",
                graphicsDeviceType =>
                    platformSettings.PlayerGraphicsApi = TryParse<GraphicsDeviceType>(graphicsDeviceType));
            optionsSet.Add("colorspace=", "Linear or Gamma color space.",
                colorSpace => platformSettings.ColorSpace = TryParse<ColorSpace>(colorSpace));
            optionsSet.Add("mtRendering",
                "Enable or disable multithreaded rendering. Enabled is default. Use option to enable, or use option and append '-' to disable.",
                option => platformSettings.MtRendering = option != null);
            optionsSet.Add("graphicsJobs",
                "Enable graphics jobs rendering. Disabled is default. Use option to enable, or use option and append '-' to disable.",
                option => platformSettings.GraphicsJobs = option != null);
            optionsSet.Add("testsrev=",
                "revision id of the tests being used.",
                testsrev => platformSettings.TestsRevision = testsrev);
            optionsSet.Add("testsrevdate=",
                "revision date of the tests being used.",
                revDate => platformSettings.TestsRevisionDate = revDate);
            optionsSet.Add("testsbranch=",
                "branch of the tests repo being used.",
                testsbranch => platformSettings.TestsBranch = testsbranch);
            optionsSet.Add("enableburst",
                "Enable burst. Enabled is default. Use option to disable: -enableburst",
                option => platformSettings.EnableBurst = option != null);
            optionsSet.Add("packageundertestname=",
                "package under test commit revision id",
                packageundertestname => platformSettings.PackageUnderTestName = packageundertestname);
            optionsSet.Add("packageundertestversion=",
                "package under test version",
                packageundertestrev => platformSettings.PackageUnderTestVersion = packageundertestrev);
            optionsSet.Add("packageundertestrev=",
                "package under test commit revision id",
                packageundertestrev => platformSettings.PackageUnderTestRevision = packageundertestrev);
            optionsSet.Add("packageundertestrevdate=",
                "package under test commit revision date",
                packageundertestrevdate => platformSettings.PackageUnderTestRevisionDate = packageundertestrevdate);
            optionsSet.Add("packageundertestbranch=",
                "branch of the package under test repo being used.",
                packageundertestbranch => platformSettings.PackageUnderTestBranch = packageundertestbranch);
            optionsSet.Add("testprojectname=",
                "test project commit revision id",
                testprojectname => platformSettings.TestProjectName = testprojectname);
            optionsSet.Add("testprojectrevision=",
                "test project commit revision id",
                testprojectrevision => platformSettings.TestProjectRevision = testprojectrevision);
            optionsSet.Add("testprojectrevdate=",
                "test project commit revision date",
                testprojectrevdate => platformSettings.TestProjectRevisionDate = testprojectrevdate);
            optionsSet.Add("testprojectbranch=",
                "branch of the test project repo being used.",
                testprojectbranch => platformSettings.TestProjectBranch = testprojectbranch);
            optionsSet.Add("joblink=",
                "Hyperlink to test job.",
                joblink => platformSettings.JobLink = joblink);
            optionsSet.Add("jobworkercount=",
                "Number of job workers to use. Range is 0 - number of cores minus 1.",
                jobworkercount =>
                {
                    if (jobworkercount != null)
                    {
                        platformSettings.JobWorkerCount = Convert.ToInt32(jobworkercount);
                    }
                });
            optionsSet.Add("apicompatibilitylevel=", "API compatibility to use. Default is NET_2_0",
                apicompatibilitylevel => platformSettings.ApiCompatibilityLevel =
                    TryParse<ApiCompatibilityLevel>(apicompatibilitylevel));
            optionsSet.Add("stripenginecode",
                "Enable Engine code stripping. Disabled is default. Use option to enable, or use option and append '-' to disable.",
                option => platformSettings.StringEngineCode = option != null);
            optionsSet.Add("managedstrippinglevel=", "Managed stripping level to use. Default is low",
                managedstrippinglevel => platformSettings.ManagedStrippingLevel =
                    TryParse<ManagedStrippingLevel>(managedstrippinglevel));
            optionsSet.Add("scriptdebugging",
                "Enable scriptdebugging. Disabled is default. Use option to enable, or use option and append '-' to disable.",
                scriptdebugging => platformSettings.ScriptDebugging = scriptdebugging != null);
            optionsSet.Add("addscenetobuild=",
                "Specify path to scene to add to the build, Path is relative to Assets folder.",
                AddSceneToBuildList);
            optionsSet.Add("enabledxrtarget|enabledxrtargets=",
                "XR target to enable in player settings. Values: " +
                "\r\n\"Oculus\"\r\n\"OpenVR\"\r\n\"cardboard\"\r\n\"daydream\"\r\n\"MockHMD\"\r\n\"OculusXRSDK\"\r\n\"MockHMDXRSDK\"\r\n\"MagicLeapXRSDK\"\r\n\"WindowsMRXRSDK\"\r\n\"PSVR2\"",
                xrTarget => platformSettings.XrTarget = xrTarget);
            optionsSet.Add("stereorenderingmode|stereorenderingpath=", "Stereo rendering mode to enable. SinglePass is default.",
                srm => platformSettings.StereoRenderingMode = srm);
            optionsSet.Add("simulationmode=",
                "Enable Simulation modes for Windows MR in Editor. Values: \r\n\"HoloLens\"\r\n\"WindowsMR\"\r\n\"Remoting\"",
                simMode => platformSettings.SimulationMode = simMode);
            optionsSet.Add("deviceruntimeversion=",
                "runtime version of the device we're running on.",
                deviceruntime => platformSettings.DeviceRuntimeVersion = string.Format("deviceruntimeversion|{0}", deviceruntime));
            optionsSet.Add("ffrlevel=",
                "ffr level we're running at",
                ffrlevel => platformSettings.FfrLevel = string.Format("ffrlevel|{0}", ffrlevel));
            optionsSet.Add("androidtargetarchitecture=",
                "Android Target Architecture to use.",
                androidtargetarchitecture => platformSettings.AndroidTargetArchitecture = TryParse<AndroidArchitecture>(androidtargetarchitecture));
            optionsSet.Add("scriptdefine=",
                "String to add to the player setting script defines.",
                scriptDefine => ScriptDefines.AddRange(scriptDefine.Split(';')));
            optionsSet.Add("vsync=",
                "test project commit revision id", vsync => platformSettings.Vsync = vsync);
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

        public static T TryParse<T>(string stringToParse)
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
            platformSettings.ScriptingImplementation =
                scriptingBackend.ToLower().StartsWith("mono") ? ScriptingImplementation.Mono2x : ScriptingImplementation.IL2CPP;
        }
    }
}
#endif
