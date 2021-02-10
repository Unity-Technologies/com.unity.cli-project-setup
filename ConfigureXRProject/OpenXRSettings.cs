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

namespace ConfigureXRProject
{
    class OpenXRPlatformSettings : XrSdkPlatformSettings<OpenXRSettings, OpenXRLoader>
    {
        protected override string xrConfigName => "UnityEngine.XR.OpenXR.Settings";
        protected override string CmdlineParam => "OpenXR";
    
        public override void SetRenderMode(PlatformSettings platformSettings, OpenXRSettings xrSettings)
        {
            try
            {
                openxrSettings.renderMode = (OpenXRSettings.RenderMode)Enum.Parse(
                    typeof(OpenXRSettings.RenderMode), platformSettings.StereoRenderingPath);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Failed to parse stereo rendering mode for OpenXR", e);
            }
        }
    }    
}
#endif
#endif