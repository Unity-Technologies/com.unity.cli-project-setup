using System;
using System.IO;
using System.Linq;
using com.unity.cliprojectsetup;
using ConfigureProject;
#if ENABLE_VR && !XR_SDK
#if UNITY_EDITOR
using UnityEditor.XR;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.XR;

namespace ConfigureXRProject
{
    class LegacyVrPlatformSettings : XRPlatformSettings<PlatformSettings>
    {
        protected override void ConfigureXr(PlatformSettings platformSettings)
        {
            if (string.IsNullOrEmpty(platformSettings.XrTarget))
            {
                return;
            }

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
}
#endif