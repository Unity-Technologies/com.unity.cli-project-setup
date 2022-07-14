using System.Linq;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

public class PackageManagerClient : IPackageManagerClient
{
    private readonly ListRequest listRequest;

    public PackageManagerClient()
    {

        listRequest = Client.List(true);
        while (!listRequest.IsCompleted)
        {
        }
    }

    public bool PackageUnderTestPresentInProject(string packageUnderTestName)
    {
        return !string.IsNullOrEmpty(packageUnderTestName) && listRequest.Result.Any(r => r.name.Equals(packageUnderTestName));
    }

    public PackageInfo GetPackageInfo(string packageName)
    {
        PackageInfo packageInfo = null;

        if (listRequest.Result.Any(r => r.name.Equals(packageName)))
        {
            packageInfo = listRequest.Result.First(r => r.name.Equals(packageName));
        }

        return packageInfo;
    }
}
