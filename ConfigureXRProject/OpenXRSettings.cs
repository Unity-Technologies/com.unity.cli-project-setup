using System;
using System.IO;
using System.Linq;
using com.unity.cliprojectsetup;
#if XR_SDK
#if UNITY_EDITOR
using UnityEditor.XR.Management;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.XR.Management;
#if OPENXR_SDK
using UnityEngine.XR.OpenXR;
using UnityEditor.XR.OpenXR.Features;

namespace ConfigureXRProject
{
    class OpenXRPlatformSettings : XrSdkPlatformSettings<OpenXRSettings, OpenXRLoader>
    {
        protected override string xrConfigName => UnityEngine.XR.OpenXR.Constants.k_SettingsKey;
        protected override string CmdlineParam => "OpenXR";

        protected override void CreateXRSettingsInstance()
        {
            FeatureHelpers.RefreshFeatures(EditorUserBuildSettings.selectedBuildTargetGroup);
            xrSettings = OpenXRSettings.ActiveBuildTargetInstance;
        }
    
        public override void SetRenderMode(PlatformSettings platformSettings)
        {
            try
            {
                xrSettings.renderMode = (OpenXRSettings.RenderMode)Enum.Parse(
                    typeof(OpenXRSettings.RenderMode), platformSettings.StereoRenderingMode);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Failed to parse stereo rendering mode for OpenXR", e);
            }
        }

        public override void ApplyLoaderSettings(XRGeneralSettingsPerBuildTarget buildTargetSettings)
        {
            EditorUtility.SetDirty(OpenXRSettings.ActiveBuildTargetInstance);
            AssetDatabase.SaveAssets();
        }
    }    
}
#endif
#endif