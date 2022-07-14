using System.Collections;
using System.Collections.Generic;
using com.unity.cliprojectsetup;
#if NUGET_MOQ_AVAILABLE
using Moq;
#endif
using NUnit.Framework;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.TestTools;

public class CliProjectSetupEditorTests
{
    private readonly string UnavailableMsg = "unavailable";
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
        platformSettings.PackageUnderTestName = @"com.unity.test-framework";

        // Act
        platformSettings.GetPackageUnderTestVersionInfo(currentSettings);

        // Assert
        Assert.AreNotEqual(UnavailableMsg, currentSettings.PackageUnderTestName, nameof(currentSettings.PackageUnderTestName));
        Assert.AreNotEqual(UnavailableMsg, currentSettings.PackageUnderTestRevision, nameof(currentSettings.PackageUnderTestRevision));
        Assert.AreNotEqual(UnavailableMsg, currentSettings.PackageUnderTestVersion, nameof(currentSettings.PackageUnderTestVersion));
    }

    [Test]
    public void VerifyGetPackageUnderTestVersionInfo_PackageUnderTestName_Empty()
    {
        // Arrange
        platformSettings.PackageUnderTestName = string.Empty;


        // Act
        platformSettings.GetPackageUnderTestVersionInfo(currentSettings);

        // Assert
        Assert.AreEqual(UnavailableMsg, currentSettings.PackageUnderTestRevision, nameof(currentSettings.PackageUnderTestRevision));
        Assert.AreEqual(UnavailableMsg, currentSettings.PackageUnderTestVersion, nameof(currentSettings.PackageUnderTestVersion));
        Assert.AreEqual(UnavailableMsg, currentSettings.PackageUnderTestRevisionDate, nameof(currentSettings.PackageUnderTestRevisionDate));
        Assert.AreEqual(UnavailableMsg, currentSettings.PackageUnderTestBranch, nameof(currentSettings.PackageUnderTestBranch));
    }

#if NUGET_MOQ_AVAILABLE
    [Test]
    public void VerifyGetPackageUnderTestVersionInfo_PackageUnderTestRevision_Empty()
    {
        // Arrange
        platformSettings.PackageUnderTestName = string.Empty;
        var testPackageName = "testPackage";
        platformSettings.PackageUnderTestName = testPackageName;
        platformSettings.PackageUnderTestRevision = UnavailableMsg;

        var mockPackageManagerClient = new Mock<IPackageManagerClient>();
        mockPackageManagerClient.Setup(pmc => pmc.PackageUnderTestPresentInProject(testPackageName)).Returns(true);


        // Act
        platformSettings.GetPackageUnderTestVersionInfo(currentSettings);

        // Assert
        Assert.AreEqual(UnavailableMsg, currentSettings.PackageUnderTestRevision, nameof(currentSettings.PackageUnderTestRevision));
    }

    [Test]
    public void VerifyGetPackageUnderTestVersionInfo_PackageUnderTestRevisionDate_Empty()
    {
        // Arrange
        platformSettings.PackageUnderTestRevisionDate = string.Empty;
        var testPackageName = "testPackage";
        platformSettings.PackageUnderTestName = testPackageName;
        platformSettings.PackageUnderTestRevision = UnavailableMsg;

        var mockPackageManagerClient = new Mock<IPackageManagerClient>();
        mockPackageManagerClient.Setup(pmc => pmc.PackageUnderTestPresentInProject(testPackageName)).Returns(true);


        // Act
        platformSettings.GetPackageUnderTestVersionInfo(currentSettings);

        // Assert
        Assert.AreEqual(UnavailableMsg, currentSettings.PackageUnderTestRevisionDate, nameof(currentSettings.PackageUnderTestRevisionDate));
    }

    [Test]
    public void VerifyGetPackageUnderTestVersionInfo_PackageUnderTestBranch_Empty()
    {
        // Arrange
        platformSettings.PackageUnderTestBranch = string.Empty;
        var testPackageName = "testPackage";
        platformSettings.PackageUnderTestName = testPackageName;
        platformSettings.PackageUnderTestRevision = UnavailableMsg;

        var mockPackageManagerClient = new Mock<IPackageManagerClient>();
        mockPackageManagerClient.Setup(pmc => pmc.PackageUnderTestPresentInProject(testPackageName)).Returns(true);


        // Act
        platformSettings.GetPackageUnderTestVersionInfo(currentSettings);

        // Assert
        Assert.AreEqual(UnavailableMsg, currentSettings.PackageUnderTestBranch, nameof(currentSettings.PackageUnderTestBranch));
    }
#endif
}