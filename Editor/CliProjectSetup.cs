#if UNITY_EDITOR
using System;
using System.Text.RegularExpressions;
using NDesk.Options;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

namespace com.unity.cliprojectsetup
{
    public class CliProjectSetup
    {
        private readonly Regex customArgRegex = new Regex("-([^=]*)=", RegexOptions.Compiled);
        private readonly PlatformSettings platformSettings = new PlatformSettings();

        public void ConfigureFromCmdlineArgs()
        {
            ParseCommandLineArgs();
            ConfigureSettings();
        }

        private void ParseCommandLineArgs()
        {
            var args = Environment.GetCommandLineArgs();
            EnsureOptionsLowerCased(args);
            var optionSet = DefineOptionSet();
            var unParsedArgs = optionSet.Parse(args);
            platformSettings.SerializeToAsset();
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
            // Setup all-inclusive player settings
            ConfigureCrossplatformSettings();
        }

        private void ConfigureCrossplatformSettings()
        {
            if (platformSettings.PlayerGraphicsApi != GraphicsDeviceType.Null)
            {
                PlayerSettings.SetUseDefaultGraphicsAPIs(platformSettings.BuildTarget, false);
                PlayerSettings.SetGraphicsAPIs(platformSettings.BuildTarget,
                    new[] {platformSettings.PlayerGraphicsApi});
            }

            PlayerSettings.graphicsJobs = platformSettings.GraphicsJobs;
            PlayerSettings.MTRendering = platformSettings.MtRendering;
            PlayerSettings.colorSpace = platformSettings.ColorSpace;
            PlayerSettings.SetScriptingBackend(EditorUserBuildSettings.selectedBuildTargetGroup,
                platformSettings.ScriptingImplementation);
            BurstCompiler.Options.EnableBurstCompilation = platformSettings.EnableBurst;
        }

        private OptionSet DefineOptionSet()
        {
            var optionsSet = new OptionSet();
            optionsSet.Add("scriptingbackend=",
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
                "logging commit revision id",
                packageundertestname => platformSettings.PackageUnderTestName = packageundertestname);
            optionsSet.Add("packageundertestrev=",
                "logging commit revision id",
                packageundertestrev => platformSettings.PackageUnderTestRevision = packageundertestrev);
            optionsSet.Add("packageundertestrevdate=",
                "logging commit revision date",
                packageundertestrevdate => platformSettings.PackageUnderTestRevisionDate = packageundertestrevdate);
            optionsSet.Add("packageundertestbranch=",
                "branch of the logging repo being used.",
                packageundertestbranch => platformSettings.PackageUnderTestBranch = packageundertestbranch);
            optionsSet.Add("joblink=",
                "Hyperlink to test job.",
                joblink => platformSettings.JobLink = joblink);
            return optionsSet;
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
            var sb = scriptingBackend.ToLower();
            if (sb.Equals("mono"))
            {
                platformSettings.ScriptingImplementation = ScriptingImplementation.Mono2x;
            }
        }
    }
}
#endif