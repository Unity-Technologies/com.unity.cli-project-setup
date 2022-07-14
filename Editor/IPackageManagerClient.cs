using UnityEditor.PackageManager;

public interface IPackageManagerClient
{
    bool PackageUnderTestPresentInProject(string packageUnderTestName);
    PackageInfo GetPackageInfo(string packageName);
}