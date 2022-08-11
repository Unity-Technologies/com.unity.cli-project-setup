using System;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
#if ENABLE_VR
using UnityEngine.XR;
#endif

namespace com.unity.cliprojectsetup
{
    public interface IPlatformSettings
    {
        BuildTargetGroup BuildTargetGroup { get; }
        BuildTarget BuildTarget { get; }
        void SerializeToAsset();
        void GetPackageUnderTestVersionInfo(CurrentSettings settings);
        string GetPackageUnderTestBranch(string version);
        string GetPackageUnderTestRevisionDate(DateTime? datePublished);
        string TryGetRevisionFromPackageJson(string packageName);
#if ENABLE_VR
        XRSettings.StereoRenderingMode GetXrStereoRenderingPathMapping(StereoRenderingPath stereoRenderingPath);
#endif
    }
}