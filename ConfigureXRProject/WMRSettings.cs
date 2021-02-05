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
#if WMR_SDK
using UnityEngine.XR.WindowsMR;

namespace ConfigureXRProject
{
    class WMRSdkPlatformSettings : XrSdkPlatformSettings<WindowsMRSettings, WindowsMRLoader>
    {
        protected override string xrConfigName => "Unity.XR.WindowsMR.Settings";
        protected override string CmdlineParam => "WMRXRSDK";
    
        public override void SetRenderMode(PlatformSettings platformSettings, WindowsMRSettings xrSettings)
        {
            // No Implementation
        }
    }
}
#endif // WMR_SDK
#endif // XR_SDK