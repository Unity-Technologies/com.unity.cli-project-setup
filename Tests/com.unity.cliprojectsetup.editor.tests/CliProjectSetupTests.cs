using System;
using System.Linq;
using com.unity.cliprojectsetup;
#if NUGET_MOQ_AVAILABLE
using Moq;
#endif
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class CliProjectSetupTests
{
    [Test]
    public void VerifyAddTestScenesToBuild_OneScene()
    {
        // Arrange
        var testScenePath = "Assets/Scenes/Scene1.unity";
        var args = new[] { $"-addscenetobuild={testScenePath}" };
        var cliProjectSetupMock = new CliProjectSetupMock(args);
        cliProjectSetupMock.ParseCommandLineArgs();


        // Act
        cliProjectSetupMock.AddTestScenesToBuild();

        // Assert
        Assert.AreEqual(testScenePath, cliProjectSetupMock.Scenes[0].path);
    }

    [Test]
    public void VerifyAddTestScenesToBuild_MultipleScene()
    {
        // Arrange
        var testScenePath1 = "Assets/Scenes/Scene1.unity";
        var testScenePath2 = "Assets/Scenes/Scene2.unity";
        var args = new[] { $"-addscenetobuild={testScenePath1}", $"-addscenetobuild={testScenePath2}" };
        var cliProjectSetupMock = new CliProjectSetupMock(args);
        cliProjectSetupMock.ParseCommandLineArgs();


        // Act
        cliProjectSetupMock.AddTestScenesToBuild();

        // Assert
        Assert.AreEqual(2, cliProjectSetupMock.Scenes.Length);
        Assert.True(cliProjectSetupMock.Scenes.Select(s => s.path).Contains(testScenePath1));
        Assert.True(cliProjectSetupMock.Scenes.Select(s => s.path).Contains(testScenePath2));
    }

    [Test]
    public void VerifyAddTestScenesToBuild_NoScenePassedInCliArgs()
    {
        // Arrange
        var cliProjectSetupMock = new CliProjectSetupMock();
        cliProjectSetupMock.ParseCommandLineArgs();


        // Act
        cliProjectSetupMock.AddTestScenesToBuild();

        // Assert
        Assert.IsNull(cliProjectSetupMock.Scenes);
    }

    [Test]
    public void VerifyParseEnumThrowsExecptionForInvalidValue()
    {
        // Assert
        Assert.Throws<ArgumentException>(() => CliProjectSetup.ParseEnum<AndroidArchitecture>("notAnAdroidArchitectureValue"));
    }

#if NUGET_MOQ_AVAILABLE
    [Test]
    public void VerifyConfigureFromCmdlineArgsSubMethodCalls()
    {
        // Arrange
        var testScenePath1 = "Assets/Scenes/Scene1.unity";
        var testScenePath2 = "Assets/Scenes/Scene2.unity";
        var args = new[] { $"-addscenetobuild={testScenePath1}", $"-addscenetobuild={testScenePath2}" };
        var mockPlatformSettings = new Mock<PlatformSettings>();
        var mockCliProjectSetup = new Mock<CliProjectSetup>(args, mockPlatformSettings.Object);

        // Act
        mockCliProjectSetup.Object.ConfigureFromCmdlineArgs();

        // Assert
        mockCliProjectSetup.Verify(m => m.ParseCommandLineArgs(), Times.Once);
        mockCliProjectSetup.Verify(m => m.ConfigureSettings(), Times.Once);
        mockCliProjectSetup.Verify(m => m.AddTestScenesToBuild(), Times.Once);
        mockPlatformSettings.Verify(m => m.SerializeToAsset(), Times.Once);
    }

    [Test]
    public void VerifyAndroid_NoXR_BuildTargetSubMethodCalls()
    {
        // Arrange
        var testScenePath1 = "Assets/Scenes/Scene1.unity";
        var args = new[] { $"-addscenetobuild={testScenePath1}" };
        var mockPlatformSettings = new Mock<PlatformSettings>();
        mockPlatformSettings.Setup(m => m.BuildTarget).Returns(BuildTarget.Android);
        var mockCliProjectSetup = new Mock<CliProjectSetup>(args, mockPlatformSettings.Object)
        {
            CallBase = true
        };

        // Act
        mockCliProjectSetup.Object.ConfigureSettings();

        // Assert
        mockCliProjectSetup.Verify(m => m.ConfigureCrossplatformSettings(), Times.Once);
        mockCliProjectSetup.Verify(m => m.ConfigureAndroidSettings(), Times.Once);
        mockCliProjectSetup.Verify(m => m.ConfigureIosSettings(), Times.Never);
        mockCliProjectSetup.Verify(m => m.ConfigureXrSettings(), Times.Never);
    }

    [Test]
    public void VerifyAndroid_WithXR_BuildTargetSubMethodCalls()
    {
        // Arrange
        var testScenePath1 = "Assets/Scenes/Scene1.unity";
        var args = new[] { $"-addscenetobuild={testScenePath1}"};
        var mockPlatformSettings = new Mock<PlatformSettings>();
        mockPlatformSettings.Setup(m => m.BuildTarget).Returns(BuildTarget.Android);
        mockPlatformSettings.Setup(m => m.XrTarget).Returns("OculusXRSDK");
        mockPlatformSettings.Setup(m => m.StereoRenderingMode).Returns("Multiview");
        var mockCliProjectSetup = new Mock<CliProjectSetup>(args, mockPlatformSettings.Object)
        {
            CallBase = true
        };

        // Act
        mockCliProjectSetup.Object.ConfigureSettings();

        // Assert
        mockCliProjectSetup.Verify(m => m.ConfigureCrossplatformSettings(), Times.Once);
        mockCliProjectSetup.Verify(m => m.ConfigureAndroidSettings(), Times.Once);
        mockCliProjectSetup.Verify(m => m.ConfigureIosSettings(), Times.Never);
        mockCliProjectSetup.Verify(m => m.ConfigureXrSettings(), Times.Once);
    }

    [Test]
    public void VerifyiOS_NoXR_BuildTargetSubMethodCalls()
    {
        // Arrange
        var testScenePath1 = "Assets/Scenes/Scene1.unity";
        var args = new[] { $"-addscenetobuild={testScenePath1}" };
        var mockPlatformSettings = new Mock<PlatformSettings>();
        mockPlatformSettings.Setup(m => m.BuildTarget).Returns(BuildTarget.iOS);
        var mockCliProjectSetup = new Mock<CliProjectSetup>(args, mockPlatformSettings.Object)
        {
            CallBase = true
        };

        // Act
        mockCliProjectSetup.Object.ConfigureSettings();

        // Assert
        mockCliProjectSetup.Verify(m => m.ConfigureCrossplatformSettings(), Times.Once);
        mockCliProjectSetup.Verify(m => m.ConfigureAndroidSettings(), Times.Never);
        mockCliProjectSetup.Verify(m => m.ConfigureIosSettings(), Times.Once);
        mockCliProjectSetup.Verify(m => m.ConfigureXrSettings(), Times.Never);
    }

    [Test]
    public void VerifyAndroid_InvalidCombo_ARMv7_without_Mono2x()
    {
        // Arrange
        var testScenePath1 = "Assets/Scenes/Scene1.unity";
        var args = new[] { $"-addscenetobuild={testScenePath1}" };
        var mockPlatformSettings = new Mock<PlatformSettings>()
        {
            CallBase = true
        };
        mockPlatformSettings.Setup(m => m.BuildTarget).Returns(BuildTarget.Android);
        mockPlatformSettings.Setup(m => m.AndroidTargetArchitecture).Returns(AndroidArchitecture.ARMv7);
        mockPlatformSettings.Setup(m => m.ScriptingImplementation).Returns(ScriptingImplementation.IL2CPP);
        var mockCliProjectSetup = new Mock<CliProjectSetup>(args, mockPlatformSettings.Object)
        {
            CallBase = true
        };

        // Act
        mockCliProjectSetup.Object.ConfigureAndroidSettings();

        // Assert
        mockCliProjectSetup.Verify(m => m.SetScriptingImplementationToMono2x(), Times.Once);
        mockCliProjectSetup.Verify(m => m.SetPlayerSettingsAndroidScriptingBackend(), Times.Once);
        mockCliProjectSetup.Verify(m => m.SetPlayerSettingsAndroidTargetArchitectures(), Times.Once);
    }

    [Test]
    public void VerifyAndroid_InvalidCombo_Mono2x_without_ARMv7()
    {
        // Arrange
        var testScenePath1 = "Assets/Scenes/Scene1.unity";
        var args = new[] { $"-addscenetobuild={testScenePath1}" };
        var mockPlatformSettings = new Mock<PlatformSettings>()
        {
            CallBase = true
        };
        mockPlatformSettings.Setup(m => m.BuildTarget).Returns(BuildTarget.Android);
        mockPlatformSettings.Setup(m => m.AndroidTargetArchitecture).Returns(AndroidArchitecture.X86_64);
        mockPlatformSettings.Setup(m => m.ScriptingImplementation).Returns(ScriptingImplementation.Mono2x);
        var mockCliProjectSetup = new Mock<CliProjectSetup>(args, mockPlatformSettings.Object)
        {
            CallBase = true
        };

        // Act
        mockCliProjectSetup.Object.ConfigureAndroidSettings();

        // Assert
        mockCliProjectSetup.Verify(m => m.SetAndroidTargetArchitectureToARMv7(), Times.Once);
        mockCliProjectSetup.Verify(m => m.SetPlayerSettingsAndroidTargetArchitectures(), Times.Once);
    }

    [Test]
    public void VerifySetJobWorkerCount_ValidValue()
    {
        // Arrange
        var args = new[] { $"-jobworkercount=1" };
        var mockPlatformSettings = new Mock<PlatformSettings>();
        var mockCliProjectSetup = new Mock<CliProjectSetup>(args, mockPlatformSettings.Object)
        {
            CallBase = true
        };        
        mockCliProjectSetup.Object.ParseCommandLineArgs();

        // Act
        mockCliProjectSetup.Object.ConfigureCrossplatformSettings();

        // Assert
        mockCliProjectSetup.Verify(m => m.SetJobWorkerCount(), Times.Once);
    }

    [Test]
    public void VerifySetJobWorkerCount_InvalidValue()
    {
        // Arrange
        var args = new[] { $"-jobworkercount=1000" };
        var mockPlatformSettings = new PlatformSettings();
        var mockCliProjectSetup = new CliProjectSetup(args, mockPlatformSettings);
        mockCliProjectSetup.ParseCommandLineArgs();

        // Act/Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => mockCliProjectSetup.SetJobWorkerCount());
    }

    [Test]
    public void VerifySetJobWorkerCount_NotCalled()
    {
        // Arrange
        var args = new[] { $"-jobworkercount=-1" };
        var mockPlatformSettings = new Mock<PlatformSettings>();
        var mockCliProjectSetup = new Mock<CliProjectSetup>(args, mockPlatformSettings.Object)
        {
            CallBase = true
        };        
        mockCliProjectSetup.Object.ParseCommandLineArgs();

        // Act
        mockCliProjectSetup.Object.ConfigureCrossplatformSettings();

        // Assert
        mockCliProjectSetup.Verify(m => m.SetJobWorkerCount(), Times.Never);
    }
#endif
}