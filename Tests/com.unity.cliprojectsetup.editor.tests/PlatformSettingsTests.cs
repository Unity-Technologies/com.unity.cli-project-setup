using com.unity.cliprojectsetup;
#if NUGET_MOQ_AVAILABLE
using Moq;
#endif
using NUnit.Framework;

public class PlaformSettingsTests
{
    private readonly string unavailableMsg = "unavailable";
    private CurrentSettings currentSettings;
    private PlatformSettings platformSettings;

    [SetUp]
    public void SetUp()
    {
        currentSettings = new CurrentSettings();
        platformSettings = new PlatformSettings();
    }

    [Test]
    public void VerifyGetPackageUnderTestVersionInfo_PackageUnderTestRevision_NotEmpty()
    {
        // Arrange
        platformSettings.PackageUnderTestName = @"com.unity.cli-project-setup";

        // Act
        platformSettings.GetPackageUnderTestVersionInfo(currentSettings);

        // Assert
        Assert.AreNotEqual(unavailableMsg, currentSettings.PackageUnderTestName, nameof(currentSettings.PackageUnderTestName));
        Assert.AreNotEqual(unavailableMsg, currentSettings.PackageUnderTestVersion, nameof(currentSettings.PackageUnderTestVersion));
    }

    [Test]
    public void VerifyGetPackageUnderTestVersionInfo_PackageUnderTestName_Empty()
    {
        // Arrange
        platformSettings.PackageUnderTestName = string.Empty;


        // Act
        platformSettings.GetPackageUnderTestVersionInfo(currentSettings);

        // Assert
        Assert.AreEqual(unavailableMsg, currentSettings.PackageUnderTestRevision, nameof(currentSettings.PackageUnderTestRevision));
        Assert.AreEqual(unavailableMsg, currentSettings.PackageUnderTestVersion, nameof(currentSettings.PackageUnderTestVersion));
        Assert.AreEqual(unavailableMsg, currentSettings.PackageUnderTestRevisionDate, nameof(currentSettings.PackageUnderTestRevisionDate));
        Assert.AreEqual(unavailableMsg, currentSettings.PackageUnderTestBranch, nameof(currentSettings.PackageUnderTestBranch));
    }

#if NUGET_MOQ_AVAILABLE
    [Test]
    public void VerifyGetPackageUnderTestVersionInfo_PackageUnderTestRevision_Empty()
    {
        // Arrange
        platformSettings.PackageUnderTestName = string.Empty;
        var testPackageName = "testPackage";
        platformSettings.PackageUnderTestName = testPackageName;
        platformSettings.PackageUnderTestRevision = unavailableMsg;

        var mockPackageManagerClient = new Mock<IPackageManagerClient>();
        mockPackageManagerClient.Setup(pmc => pmc.PackageUnderTestPresentInProject(testPackageName)).Returns(true);


        // Act
        platformSettings.GetPackageUnderTestVersionInfo(currentSettings);

        // Assert
        Assert.AreEqual(unavailableMsg, currentSettings.PackageUnderTestRevision, nameof(currentSettings.PackageUnderTestRevision));
    }

    [Test]
    public void VerifyGetPackageUnderTestVersionInfo_PackageUnderTestRevisionDate_Empty()
    {
        // Arrange
        platformSettings.PackageUnderTestRevisionDate = string.Empty;
        var testPackageName = "testPackage";
        platformSettings.PackageUnderTestName = testPackageName;
        platformSettings.PackageUnderTestRevision = unavailableMsg;

        var mockPackageManagerClient = new Mock<IPackageManagerClient>();
        mockPackageManagerClient.Setup(pmc => pmc.PackageUnderTestPresentInProject(testPackageName)).Returns(true);


        // Act
        platformSettings.GetPackageUnderTestVersionInfo(currentSettings);

        // Assert
        Assert.AreEqual(unavailableMsg, currentSettings.PackageUnderTestRevisionDate, nameof(currentSettings.PackageUnderTestRevisionDate));
    }

    [Test]
    public void VerifyGetPackageUnderTestVersionInfo_PackageUnderTestBranch_Empty()
    {
        // Arrange
        platformSettings.PackageUnderTestBranch = string.Empty;
        var testPackageName = "testPackage";
        platformSettings.PackageUnderTestName = testPackageName;
        platformSettings.PackageUnderTestRevision = unavailableMsg;

        var mockPackageManagerClient = new Mock<IPackageManagerClient>();
        mockPackageManagerClient.Setup(pmc => pmc.PackageUnderTestPresentInProject(testPackageName)).Returns(true);


        // Act
        platformSettings.GetPackageUnderTestVersionInfo(currentSettings);

        // Assert
        Assert.AreEqual(unavailableMsg, currentSettings.PackageUnderTestBranch, nameof(currentSettings.PackageUnderTestBranch));
    }
#endif
}