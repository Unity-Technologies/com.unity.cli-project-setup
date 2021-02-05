using System;
using System.IO;
using System.Linq;
using com.unity.cliprojectsetup;
using ConfigureProject;
#if XR_SDK
#if UNITY_EDITOR
using UnityEditor.XR.Management;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.XR.Management;

namespace ConfigureXRProject
{
    public abstract class XrSdkPlatformSettings<T, U> : XRPlatformSettings<PlatformSettings>
    where T : ScriptableObject
    where U : XRLoader
    {
        private static readonly string xrsdkTestXrSettingsPath = "Assets/XR/Settings/Test Settings.asset";

        protected readonly T xrSettings = ScriptableObject.CreateInstance<T>();
        protected abstract string xrConfigName { get; }
        protected abstract string CmdlineParam { get; }

        protected override void ConfigureXr(PlatformSettings platformSettings)
        {
            if (!string.Equals(CmdlineParam, platformSettings.XrTarget, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
#if !UNITY_2020_OR_NEWER
            PlayerSettings.virtualRealitySupported = false;
#endif

            // Create our own test version of xr general settings.
            var xrGeneralSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            var buildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();

            EnsureArgumentsNotNull(xrGeneralSettings, buildTargetSettings, managerSettings);


            xrGeneralSettings.Manager = managerSettings;

            SetupLoader(xrGeneralSettings, buildTargetSettings, managerSettings);

            if (xrSettings == null)
            {
                throw new ArgumentNullException(
                    $"Tried to instantiate an instance of {typeof(T).Name} but it is null.");
            }

            SetRenderMode(platformSettings);

            AssetDatabase.AddObjectToAsset(xrSettings, xrsdkTestXrSettingsPath);
            AssetDatabase.SaveAssets();
            EditorBuildSettings.AddConfigObject(xrConfigName, xrSettings, true);

            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, buildTargetSettings, true);
        }

        public abstract void SetRenderMode(PlatformSettings platformSettings);

        private static void SetupLoader(XRGeneralSettings xrGeneralSettings,
            XRGeneralSettingsPerBuildTarget buildTargetSettings,
            XRManagerSettings managerSettings)
        {
            var loader = ScriptableObject.CreateInstance<U>();

            if (loader == null)
            {
                throw new ArgumentNullException(
                    $"Tried to instantiate an instance of {typeof(U).Name}, but it is null.");
            }

            loader.name = loader.GetType().Name;

            xrGeneralSettings.Manager.loaders.Add(loader);

            buildTargetSettings.SetSettingsForBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup,
                xrGeneralSettings);

            EnsureXrGeneralSettingsPathExists(xrsdkTestXrSettingsPath);

            AssetDatabase.CreateAsset(buildTargetSettings, xrsdkTestXrSettingsPath);

            AssetDatabase.AddObjectToAsset(xrGeneralSettings, xrsdkTestXrSettingsPath);
            AssetDatabase.AddObjectToAsset(managerSettings, xrsdkTestXrSettingsPath);
            AssetDatabase.AddObjectToAsset(loader, xrsdkTestXrSettingsPath);
        }

        private static void EnsureXrGeneralSettingsPathExists(string testXrGeneralSettingsPath)
        {
            var settingsPath = Path.GetDirectoryName(testXrGeneralSettingsPath);
            if (!Directory.Exists(settingsPath))
            {
                Directory.CreateDirectory(testXrGeneralSettingsPath);
            }
        }

        private static void EnsureArgumentsNotNull(XRGeneralSettings xrGeneralSettings,
            XRGeneralSettingsPerBuildTarget buildTargetSettings, XRManagerSettings managerSettings)
        {
            EnsureArgumentNotNull(xrGeneralSettings);
            EnsureArgumentNotNull(buildTargetSettings);
            EnsureArgumentNotNull(managerSettings);
        }

        private static void EnsureArgumentNotNull(object arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }
        }
    }
}
#else
    class LegacyVrPlatformSettings : XRPlatformSettings<PlatformSettings>
    {
        protected override void ConfigureXr(PlatformSettings platformSettings)
        {
            PlayerSettings.virtualRealitySupported = true;
    
            UnityEditorInternal.VR.VREditor.SetVREnabledDevicesOnTargetGroup
                (platformSettings.BuildTargetGroup, new string[] { platformSettings.XrTarget });
    
            try
            {
                PlayerSettings.stereoRenderingPath = (StereoRenderingPath)Enum.Parse(
                    typeof(StereoRenderingPath), platformSettings.StereoRenderingMode);
            }
            catch (System.Exception e)
            {
                throw new ArgumentException(
                    "Error trying to cast stereo rendering mode cmdline parameter to UnityEditor.StereoRenderingPath type.", e);
            }
        }
    }
#endif // XR_SDK