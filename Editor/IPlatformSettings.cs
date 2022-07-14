using System;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine.XR;

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
        XRSettings.StereoRenderingMode GetXrStereoRenderingPathMapping(StereoRenderingPath stereoRenderingPath);
    }
}