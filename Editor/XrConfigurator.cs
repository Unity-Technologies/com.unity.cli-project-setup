using System;
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

namespace com.unity.cliprojectsetup
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
#elif !UNITY_2020_1_OR_NEWER
            if (!string.IsNullOrEmpty(platformSettings.EnabledXrTarget))
            {
                ConfigureLegacyVr();
            }
#endif
#endif
        }

#if XR_SDK
        private void ConfigureXrSdk()
        {
#if !UNITY_2020_OR_NEWER
            PlayerSettings.virtualRealitySupported = false;
#endif

            // Create our own test version of xr general settings.
            var xrGeneralSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            var buildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();

            EnsureArgumentsNotNull(xrGeneralSettings, buildTargetSettings, managerSettings);

            xrGeneralSettings.Manager = managerSettings;

#if OCULUS_SDK
            if (platformSettings.XrTarget == "OculusXRSDK")
            {
                SetupLoader<OculusLoader>(xrGeneralSettings, buildTargetSettings, managerSettings);

                var oculusSettings = ScriptableObject.CreateInstance<OculusSettings>();

                if (oculusSettings == null)
                {
                    throw new ArgumentNullException(
                        $"Tried to instantiate an instance of {typeof(OculusSettings).Name} but it is null.");
                }

                if (platformSettings.BuildTarget == BuildTarget.Android)
                {
                    try
                    {
                        oculusSettings.m_StereoRenderingModeAndroid = (OculusSettings.StereoRenderingModeAndroid)Enum.Parse(
                            typeof(OculusSettings.StereoRenderingModeAndroid), platformSettings.StereoRenderingPath);
                    }
                    catch (Exception e)
                    {
                        throw new ArgumentException("Failed to parse stereo rendering mode for Android Oculus XR SDK", e);
                    }
                }
                else
                {
                    try
                    {
                        oculusSettings.m_StereoRenderingModeDesktop = (OculusSettings.StereoRenderingModeDesktop)Enum.Parse(
                            typeof(OculusSettings.StereoRenderingModeDesktop), platformSettings.StereoRenderingPath);
                    }
                    catch (Exception e)
                    {

                        throw new ArgumentException("Failed to parse stereo rendering mode for Desktop Oculus XR SDK.", e);
                    }
                }

                AssetDatabase.AddObjectToAsset(oculusSettings, xrsdkTestXrSettingsPath);

                AssetDatabase.SaveAssets();

                EditorBuildSettings.AddConfigObject("Unity.XR.Oculus.Settings", oculusSettings, true); 
            }
#endif

#if MOCKHMD_SDK
            if (platformSettings.XrTarget == "MockHMDXRSDK")
            {
                SetupLoader<MockHMDLoader>(xrGeneralSettings, buildTargetSettings, managerSettings);

                var mockSettings = ScriptableObject.CreateInstance<MockHMDBuildSettings>();

                if (mockSettings == null)
                {
                    throw new ArgumentNullException("Failed to create Mock HMD settings asset.");
                }

                try
                {
                    mockSettings.renderMode = (MockHMDBuildSettings.RenderMode)Enum.Parse(
                        typeof(MockHMDBuildSettings.RenderMode), platformSettings.StereoRenderingPath);
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Failed to parse stereo rendering mode for Mock HMD XR SDK", e);
                }

                AssetDatabase.AddObjectToAsset(mockSettings, xrsdkTestXrSettingsPath);

                AssetDatabase.SaveAssets();

                EditorBuildSettings.AddConfigObject("Unity.XR.MockHMD.Settings", mockSettings, true); 
            }
#endif

            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, buildTargetSettings, true);
        }

        private void SetupLoader<T>(XRGeneralSettings xrGeneralSettings,
            XRGeneralSettingsPerBuildTarget buildTargetSettings,
            XRManagerSettings managerSettings) where T : XRLoader
        {
            var loader = ScriptableObject.CreateInstance<T>();

            if (loader == null)
            {
                throw new ArgumentNullException(
                    $"Tried to instantiate an instance of {typeof(T).Name}, but it is null.");
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
#elif !UNITY_2020_1_OR_NEWER
        private void ConfigureLegacyVr()
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
#endif
    }

}