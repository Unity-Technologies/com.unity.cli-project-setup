
using System.IO;
#if UNITY_EDITOR
#if XR_SDK
using UnityEditor.XR.Management;
using System;
#endif
using UnityEditor;
#endif
using UnityEngine;
#if XR_SDK
using UnityEngine.XR.Management;
#endif
#if OCULUS_SDK
using Unity.XR.Oculus;
#endif
#if MOCKHMD_SDK
using Unity.XR.MockHMD;
#endif

namespace com.unity.cliconfigmanager
{
    public class XrConfigurator
    {
#if XR_SDK
        private readonly string xrsdkTestXrSettingsPath = "Assets/XR/Settings/Test Settings.asset";
#endif

        private readonly PlatformSettings platformSettings;

        public XrConfigurator(PlatformSettings platformSettings)
        {
            this.platformSettings = platformSettings;
        }
#if UNITY_EDITOR
        public void ConfigureXr()
        {
#if XR_SDK
            ConfigureXrSdk();
#else
            ConfigureLegacyVr();
#endif
        }

#if XR_SDK
        private void ConfigureXrSdk()
        {
            PlayerSettings.virtualRealitySupported = false;

            // Create our own test version of xr general settings.
            var xrGeneralSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            var buildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();

            xrGeneralSettings.Manager = managerSettings;
            EnsureArgumentsNotNull(xrGeneralSettings, buildTargetSettings, managerSettings);
#if OCULUS_SDK
            SetupLoader<OculusLoader>(xrGeneralSettings, buildTargetSettings, managerSettings);
#endif
#if MOCKHMD_SDK
            PlayerSettings.stereoRenderingPath = platformSettings.StereoRenderingPath;
            SetupLoader<MockHMDLoader>(xrGeneralSettings, buildTargetSettings, managerSettings);
#endif
            
#if OCULUS_SDK
            var settings = ConfigureOculusSettings();
#endif
            AssetDatabase.SaveAssets();
            
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, buildTargetSettings, true);
#if OCULUS_SDK
            EditorBuildSettings.AddConfigObject("Unity.XR.Oculus.Settings", settings, true);
#endif
        }

#if OCULUS_SDK
        private OculusSettings ConfigureOculusSettings()
        {
            var settings = ScriptableObject.CreateInstance<OculusSettings>();
            if (settings == null)
            {
                throw new ArgumentNullException(
                    $"Tried to instantiate an instance of {typeof(OculusSettings).Name} but it is null.");
            }

            AssetDatabase.AddObjectToAsset(settings, xrsdkTestXrSettingsPath);

            if (platformSettings.BuildTarget == BuildTarget.Android)
            {
                settings.m_StereoRenderingModeAndroid = platformSettings.StereoRenderingModeAndroid;
            }
            else
            {
                settings.m_StereoRenderingModeDesktop = platformSettings.StereoRenderingModeDesktop;
            }

            return settings;
        }
#endif

        private void SetupLoader<T>(XRGeneralSettings xrGeneralSettings,
            XRGeneralSettingsPerBuildTarget buildTargetSettings,
            XRManagerSettings managerSettings) where T : XRLoader
        {
            var loader = ScriptableObject.CreateInstance<T>();
            loader.name = loader.GetType().Name;

            if (loader == null)
            {
                throw new ArgumentNullException(
                    $"Tried to instantiate an instance of {typeof(T).Name}, but it is null.");
            }

            xrGeneralSettings.Manager.loaders.Add(loader);

            buildTargetSettings.SetSettingsForBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup,
                xrGeneralSettings);

            EnsureXrGeneralSettingsPathExists(xrsdkTestXrSettingsPath);

            AssetDatabase.CreateAsset(buildTargetSettings, xrsdkTestXrSettingsPath);

            AssetDatabase.AddObjectToAsset(xrGeneralSettings, xrsdkTestXrSettingsPath);
            AssetDatabase.AddObjectToAsset(managerSettings, xrsdkTestXrSettingsPath);
            AssetDatabase.AddObjectToAsset(loader, xrsdkTestXrSettingsPath);
        }

        private void EnsureArgumentsNotNull(XRGeneralSettings xrGeneralSettings,
            XRGeneralSettingsPerBuildTarget buildTargetSettings, XRManagerSettings managerSettings)
        {
            EnsureArgumentNotNull(xrGeneralSettings);
            EnsureArgumentNotNull(buildTargetSettings);
            EnsureArgumentNotNull(managerSettings);
        }

        private void EnsureXrGeneralSettingsPathExists(string testXrGeneralSettingsPath)
        {
            var settingsPath = Path.GetDirectoryName(testXrGeneralSettingsPath);
            if (!Directory.Exists(settingsPath))
            {
                Directory.CreateDirectory(testXrGeneralSettingsPath);
            }
        }

        private static void EnsureArgumentNotNull(object arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }
        }
#else
        private void ConfigureLegacyVr()
        {
            PlayerSettings.virtualRealitySupported = true;
            PlayerSettings.stereoRenderingPath = platformSettings.StereoRenderingPath;
            UnityEditorInternal.VR.VREditor.SetVREnabledDevicesOnTargetGroup(platformSettings.BuildTargetGroup,
                new string[] {platformSettings.XrTarget});
        }
#endif
#endif
    }

}