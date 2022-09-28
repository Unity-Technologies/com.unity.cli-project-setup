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
#if PSVR2_SDK
using UnityEngine.XR.PSVR2;

namespace ConfigureXRProject
{
    class PSVR2PlatformSettings : XrSdkPlatformSettings<PSVR2Settings, PSVR2Loader>
    {
        protected override string xrConfigName => "UnityEngine.XR.PSVR2.PSVR2Settings";
        protected override string CmdlineParam => "PSVR2";

        public override void SetRenderMode(PlatformSettings platformSettings)
        {
            try
            {
                xrSettings.StereoRenderingMode = (StereoRenderingMode)Enum.Parse(
                    typeof(StereoRenderingMode), platformSettings.StereoRenderingMode);
            }
            catch (Exception e)
            {

                throw new ArgumentException("Failed to parse stereo rendering mode for PSVR2.", e);
            }
        }
    }
}
#endif
#endif