using System;
using System.Collections.Generic;
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

        private List<string> OpenXrFeatures;

        protected override void CreateXRSettingsInstance()
        {
            FeatureHelpers.RefreshFeatures(EditorUserBuildSettings.selectedBuildTargetGroup);
            xrSettings = OpenXRSettings.ActiveBuildTargetInstance;
        }

        protected override bool IsXrTarget(string xrTarget)
        {
            if (xrTarget.StartsWith(CmdlineParam, StringComparison.OrdinalIgnoreCase))
            {
                OpenXrFeatures = xrTarget.Split(new char[]{'.'}, StringSplitOptions.RemoveEmptyEntries).ToList();
                return true;
            }

            return false;
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
            FeatureHelpers.RefreshFeatures(BuildTargetGroup.Standalone);
            var msController = OpenXRSettings.ActiveBuildTargetInstance
                .GetFeature<UnityEngine.XR.OpenXR.Features.Interactions.MicrosoftMotionControllerProfile>();

            if (OpenXrFeatures.Contains("MockRT"))
            {
                Debug.Log("Enabling Mock Runtime Feature");

                var feature = OpenXRSettings.Instance.GetFeatures().Where(f => f.GetType().Name == "MockRuntime").FirstOrDefault();
                feature.enabled = true;
                feature.GetType().GetField("ignoreValidationErrors").SetValue(feature, true);
            }
            else
            {
                if (msController)
                {
                    msController.enabled = true;
                    EditorUtility.SetDirty(OpenXRSettings.ActiveBuildTargetInstance);
                }
            }
            EditorUtility.SetDirty(OpenXRSettings.ActiveBuildTargetInstance);
            AssetDatabase.SaveAssets();
        }
    }    
}
#endif
#endif