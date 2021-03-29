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
#if MOCKHMD_SDK
using Unity.XR.MockHMD;

namespace ConfigureXRProject
{
    class MockHMDSdkPlatformSettings : XrSdkPlatformSettings<MockHMDBuildSettings, MockHMDLoader>
    {
        protected override string xrConfigName => "Unity.XR.MockHMD.Settings";
        protected override string CmdlineParam => "MockHMDXRSDK";
    
        public override void SetRenderMode(PlatformSettings platformSettings)
        {
            try
            {
                xrSettings.renderMode = (MockHMDBuildSettings.RenderMode)Enum.Parse(
                    typeof(MockHMDBuildSettings.RenderMode), platformSettings.StereoRenderingMode);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Failed to parse stereo rendering mode for Mock HMD XR SDK", e);
            }
        }
    }
}
#endif // MOCKHMD_SDK
#endif // XR_SDK