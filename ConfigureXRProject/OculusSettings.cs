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
#if OCULUS_SDK
using Unity.XR.Oculus;

namespace ConfigureXRProject
{
    class OculusSdkPlatformSettings : XrSdkPlatformSettings<OculusSettings, OculusLoader>
    {
        protected override string xrConfigName => "Unity.XR.Oculus.Settings";
        protected override string CmdlineParam => "OculusXRSDK";

        public override void SetRenderMode(PlatformSettings platformSettings)
        {
            if (platformSettings.BuildTarget == BuildTarget.Android)
            {
                try
                {
                    xrSettings.m_StereoRenderingModeAndroid = (OculusSettings.StereoRenderingModeAndroid)Enum.Parse(
                        typeof(OculusSettings.StereoRenderingModeAndroid), platformSettings.StereoRenderingMode);
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
                    xrSettings.m_StereoRenderingModeDesktop = (OculusSettings.StereoRenderingModeDesktop)Enum.Parse(
                        typeof(OculusSettings.StereoRenderingModeDesktop), platformSettings.StereoRenderingMode);
                }
                catch (Exception e)
                {

                    throw new ArgumentException("Failed to parse stereo rendering mode for Desktop Oculus XR SDK.", e);
                }
            }
        }
    }    
}
#endif
#endif