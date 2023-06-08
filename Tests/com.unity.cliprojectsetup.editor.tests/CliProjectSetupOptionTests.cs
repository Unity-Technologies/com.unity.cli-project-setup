using com.unity.cliprojectsetup;
#if NUGET_MOQ_AVAILABLE
using Moq;
#endif
using NUnit.Framework;
using UnityEditor;
using UnityEngine.Rendering;

public class CliProjectSetupOptions
{
        [Test]
    public void VerifyOptionsLowerCased_NoMatch_NoChange()
    {
        // Arrange
        var expArgs = new[] { "-a=Arg1", "-b=arg2", "-c=ARG3" };
        var cliProjectSetup = new CliProjectSetup(expArgs);
        cliProjectSetup.ParseCommandLineArgs();

        // Act
        cliProjectSetup.EnsureOptionsLowerCased();

        // Assert
        Assert.AreEqual(new[] { "-a=Arg1", "-b=arg2", "-c=ARG3" }, cliProjectSetup.CliArgs);
    }

    [Test]
    public void VerifyOptionsLowerCased_SingleMatch_ChangeToLowercase()
    {
        // Arrange
        var inputArgs = new[] { "-a=Arg1", "-B=arg2", "-c=arg3" };
        var expTransformedArgs = new[] { "-a=Arg1", "-b=arg2", "-c=arg3" };
        var cliProjectSetup = new CliProjectSetup(inputArgs);
        cliProjectSetup.ParseCommandLineArgs();

        // Act
        cliProjectSetup.EnsureOptionsLowerCased();

        // Assert
        Assert.AreEqual(expTransformedArgs, cliProjectSetup.CliArgs);
    }

    [Test]
    public void VerifyOptionsLowerCased_MultipleMatches_ChangeToLowercase()
    {
        // Arrange
        var inputArgs = new[] { "-A=Arg1", "-B=arg2", "-c=ARG3", "-d=Arg4" };
        var expTransformedArgs = new[] { "-a=Arg1", "-b=arg2", "-c=ARG3", "-d=Arg4" };
        var cliProjectSetup = new CliProjectSetup(inputArgs);
        cliProjectSetup.ParseCommandLineArgs();

        // Act
        cliProjectSetup.EnsureOptionsLowerCased();

        // Assert
        Assert.AreEqual(expTransformedArgs, cliProjectSetup.CliArgs);
    }

    [Test]
    public void VerifyOption_SetScriptingBackend()
    {
        // Arrange
        var testScenePath1 = "Assets/Scenes/Scene1.unity";
        var args = new[] { $"-addscenetobuild={testScenePath1}" };
        var mockPlatformSettings = new Mock<PlatformSettings>()
        {
            CallBase = true
        };
        mockPlatformSettings.Setup(m => m.ScriptingImplementation).Returns(ScriptingImplementation.IL2CPP);
        var mockCliProjectSetup = new Mock<CliProjectSetup>(args, mockPlatformSettings.Object)
        {
            CallBase = true
        };

        // Act
        mockCliProjectSetup.Object.ConfigureCrossplatformSettings();

        // Assert
        mockCliProjectSetup.Verify(m => m.TrySetScriptingBackend(), Times.Once);
    }

    [Test]
    public void VerifyOption_DoNotSetScriptingBackend()
    {
        // Arrange
        var testScenePath1 = "Assets/Scenes/Scene1.unity";
        var args = new[] { $"-addscenetobuild={testScenePath1}" };
        var mockPlatformSettings = new Mock<PlatformSettings>()
        {
            CallBase = true
        };

        var mockCliProjectSetup = new Mock<CliProjectSetup>(args, mockPlatformSettings.Object)
        {
            CallBase = true
        };

        // Act
        mockCliProjectSetup.Object.ConfigureCrossplatformSettings();

        // Assert
        mockCliProjectSetup.Verify(m => m.SetScriptingBackend(), Times.Never);
    }

    [Test]
    public void VerifyOption_SetApiCompatabilityLevel()
    {
        // Arrange
        var testScenePath1 = "Assets/Scenes/Scene1.unity";
        var args = new[] { $"-addscenetobuild={testScenePath1}" };
        var mockPlatformSettings = new Mock<PlatformSettings>()
        {
            CallBase = true
        };

        mockPlatformSettings.Setup(m => m.ApiCompatibilityLevel).Returns(ApiCompatibilityLevel.NET_4_6);
        var mockCliProjectSetup = new Mock<CliProjectSetup>(args, mockPlatformSettings.Object)
        {
            CallBase = true
        };

        // Act
        mockCliProjectSetup.Object.ConfigureCrossplatformSettings();

        // Assert
        mockCliProjectSetup.Verify(m => m.SetApiCompatabilityLevel(), Times.Once);
    }

    [Test]
    public void VerifyOption_DoNotSetApiCompatabilityLevel()
    {
        // Arrange
        var testScenePath1 = "Assets/Scenes/Scene1.unity";
        var args = new[] { $"-addscenetobuild={testScenePath1}" };
        var mockPlatformSettings = new Mock<PlatformSettings>()
        {
            CallBase = true
        };
        var mockCliProjectSetup = new Mock<CliProjectSetup>(args, mockPlatformSettings.Object)
        {
            CallBase = true
        };

        // Act
        mockCliProjectSetup.Object.ConfigureCrossplatformSettings();

        // Assert
        mockCliProjectSetup.Verify(m => m.SetApiCompatabilityLevel(), Times.Never);
    }

    [Test]
    public void VerifyOption_MtRenderingIsTrueByDefault()
    {
        // Arrange
        var cliProjectSetup = new CliProjectSetup(new string[] { });

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.IsTrue(cliProjectSetup.PlatformSettings.MtRendering);
    }

    [Test]
    public void VerifyOption_MtRenderingSetToFalse()
    {
        // Arrange
        var args = new[] { $"-mtRendering-" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.IsFalse(cliProjectSetup.PlatformSettings.MtRendering);
    }

    [Test]
    public void VerifyOption_GraphicsJobsIsFalseByDefault()
    {
        // Arrange
        var cliProjectSetup = new CliProjectSetup(new string[] { });

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.IsFalse(cliProjectSetup.PlatformSettings.GraphicsJobs);
    }

    [Test]
    public void VerifyOption_GraphicsJobsSetToTrue()
    {
        // Arrange
        var args = new[] { $"-graphicsJobs" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.IsTrue(cliProjectSetup.PlatformSettings.GraphicsJobs);
    }

    [Test]
    public void VerifyOption_ScriptingBackend_Il2Cpp()
    {
        // Arrange
        var args = new[] { $"-scripting-backend=il2cpp" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("IL2CPP", cliProjectSetup.PlatformSettings.ScriptingImplementation.ToString());
    }

    [Test]
    public void VerifyOption_ScriptingBackend_Il2Cpp_AlternateCliOption()
    {
        // Arrange
        var args = new[] { $"-scriptingbackend=il2cpp" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("IL2CPP", cliProjectSetup.PlatformSettings.ScriptingImplementation.ToString());
    }

    [Test]
    public void VerifyOption_ScriptingBackend_Mono()
    {
        // Arrange
        var args = new[] { $"-scripting-backend=mono" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("Mono2x", cliProjectSetup.PlatformSettings.ScriptingImplementation.ToString());
    }

    [Test]
    public void VerifyOption_ScriptingBackend_NotSet()
    {
        // Arrange
        var cliProjectSetup = new CliProjectSetup(new string[] { });

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual(string.Empty, cliProjectSetup.PlatformSettings.ScriptingImplementation.ToString());
    }

    [Test]
    public void VerifyOption_PlayerGraphicsApi_Metal()
    {
        // Arrange
        var args = new[] { $"-playergraphicsapi=Metal" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("Metal", cliProjectSetup.PlatformSettings.PlayerGraphicsApi.ToString());
    }

    [Test]
    public void VerifyOption_PlayerGraphicsApi_Default()
    {
        // Arrange
        var cliProjectSetup = new CliProjectSetup(new string[] { });

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual((GraphicsDeviceType)0, cliProjectSetup.PlatformSettings.PlayerGraphicsApi);
    }

    [Test]
    public void VerifyOption_Colorspace_Linear()
    {
        // Arrange
        var args = new[] { $"-colorspace=Linear" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("Linear", cliProjectSetup.PlatformSettings.ColorSpace.ToString());
    }

    [Test]
    public void VerifyOption_Colorspace_Gamma()
    {
        // Arrange
        var args = new[] { $"-colorspace=Gamma" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("Gamma", cliProjectSetup.PlatformSettings.ColorSpace.ToString());
    }

    [Test]
    public void VerifyOption_TestsRev()
    {
        // Arrange
        var args = new[] { $"-testsrev=1" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("1", cliProjectSetup.PlatformSettings.TestsRevision);
    }

    [Test]
    public void VerifyOption_TestsRevDate()
    {
        // Arrange
        var args = new[] { $"-testsrevdate=12345" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("12345", cliProjectSetup.PlatformSettings.TestsRevisionDate);
    }

    [Test]
    public void VerifyOption_TestsBranch()
    {
        // Arrange
        var args = new[] { $"-testsbranch=myDevBranch" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("myDevBranch", cliProjectSetup.PlatformSettings.TestsBranch);
    }

    [Test]
    public void VerifyOption_EnableBurstByDefault()
    {
        // Arrange
        var cliProjectSetup = new CliProjectSetup(new string[] { });

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.IsTrue(cliProjectSetup.PlatformSettings.EnableBurst);
    }

    [Test]
    public void VerifyOption_DisableBurst()
    {
        // Arrange
        var args = new[] { $"-enableburst-" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.IsFalse(cliProjectSetup.PlatformSettings.EnableBurst);
    }

    [Test]
    public void VerifyOption_PackageUnderTestName()
    {
        // Arrange
        var args = new[] { $"-packageundertestname=myPackageName" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("myPackageName", cliProjectSetup.PlatformSettings.PackageUnderTestName);
    }

    [Test]
    public void VerifyOption_PackageUnderTestVersion()
    {
        // Arrange
        var args = new[] { $"-packageundertestversion=12.0.3" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("12.0.3", cliProjectSetup.PlatformSettings.PackageUnderTestVersion);
    }

    [Test]
    public void VerifyOption_PackageUnderTestRevision()
    {
        // Arrange
        var args = new[] { $"-packageundertestrev=45890" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("45890", cliProjectSetup.PlatformSettings.PackageUnderTestRevision);
    }

    [Test]
    public void VerifyOption_PackageUnderTestRevisionDate()
    {
        // Arrange
        var args = new[] { $"-packageundertestrevdate=45890" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("45890", cliProjectSetup.PlatformSettings.PackageUnderTestRevisionDate);
    }

    [Test]
    public void VerifyOption_PackageUnderTestBranch()
    {
        // Arrange
        var args = new[] { $"-packageundertestbranch=myPackageBranch" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("myPackageBranch", cliProjectSetup.PlatformSettings.PackageUnderTestBranch);
    }

    [Test]
    public void VerifyOption_TestProjectName()
    {
        // Arrange
        var args = new[] { $"-testprojectname=myTestProjectName" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("myTestProjectName", cliProjectSetup.PlatformSettings.TestProjectName);
    }

    [Test]
    public void VerifyOption_TestProjectRevision()
    {
        // Arrange
        var args = new[] { $"-testprojectrevision=1.2.3" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("1.2.3", cliProjectSetup.PlatformSettings.TestProjectRevision);
    }

    [Test]
    public void VerifyOption_TestProjectRevisionDate()
    {
        // Arrange
        var args = new[] { $"-testprojectrevdate=45891" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("45891", cliProjectSetup.PlatformSettings.TestProjectRevisionDate);
    }

    [Test]
    public void VerifyOption_TestProjectBranch()
    {
        // Arrange
        var args = new[] { $"-testprojectbranch=myTestProjectBranch" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("myTestProjectBranch", cliProjectSetup.PlatformSettings.TestProjectBranch);
    }

    [Test]
    public void VerifyOption_JobLink()
    {
        // Arrange
        var args = new[] { $"-joblink=http://myjoblink" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("http://myjoblink", cliProjectSetup.PlatformSettings.JobLink);
    }

    [Test]
    public void VerifyOption_JobWorkerCount()
    {
        // Arrange
        var args = new[] { $"-jobworkercount=4" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual(4, cliProjectSetup.PlatformSettings.JobWorkerCount);
    }

    [Test]
    public void VerifyOption_JobWorkerCount_HandleEmptyStringValue()
    {
        // Arrange
        var args = new[] { $"-jobworkercount=" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual(-1, cliProjectSetup.PlatformSettings.JobWorkerCount);
    }

    [Test]
    public void VerifyOption_ApiCompatibilityLevel()
    {
        // Arrange
        var args = new[] { $"-apicompatibilitylevel=NET_2_0_Subset" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("NET_2_0_Subset", cliProjectSetup.PlatformSettings.ApiCompatibilityLevel.ToString());
    }

    [Test]
    public void VerifyOption_StripEngineCodeDisabledByDefault()
    {
        // Arrange
        var cliProjectSetup = new CliProjectSetup(new string[] { });

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.IsFalse(cliProjectSetup.PlatformSettings.StripEngineCode);
    }

    [Test]
    public void VerifyOption_StripEngineCode()
    {
        // Arrange
        var args = new[] { $"-stripenginecode" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.IsTrue(cliProjectSetup.PlatformSettings.StripEngineCode);
    }

    [Test]
    public void VerifyOption_ManagedStrippingLevelDefault()
    {
        // Arrange
        var cliProjectSetup = new CliProjectSetup(new string[] { });

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("Disabled", cliProjectSetup.PlatformSettings.ManagedStrippingLevel.ToString());
    }

    [Test]
    public void VerifyOption_ManagedStrippingLevel()
    {
        // Arrange
        var args = new[] { $"-managedstrippinglevel=High" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("High", cliProjectSetup.PlatformSettings.ManagedStrippingLevel.ToString());
    }

    [Test]
    public void VerifyOption_ScriptDebuggingDisabledByDefault()
    {
        // Arrange
        var cliProjectSetup = new CliProjectSetup(new string[] { });

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.IsFalse(cliProjectSetup.PlatformSettings.ScriptDebugging);
    }

    [Test]
    public void VerifyOption_ScriptDebugging()
    {
        // Arrange
        var args = new[] { $"-scriptdebugging" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.IsTrue(cliProjectSetup.PlatformSettings.ScriptDebugging);
    }

    [Test]
    public void VerifyOption_AddSceneToBuild()
    {
        // Arrange
        var testScenePath1 = "Assets/Scenes/Scene1.unity";
        var args = new[] { $"-addscenetobuild={testScenePath1}" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.True(cliProjectSetup.ScenesToAddToBuild.Contains(testScenePath1));
    }

    [Test]
    public void VerifyOption_AddMutlipleScenesToBuild()
    {
        // Arrange
        var testScenePath1 = "Assets/Scenes/Scene1.unity";
        var testScenePath2 = "Assets/Scenes/Scene2.unity";
        var args = new[] { $"-addscenetobuild={testScenePath1}", $"-addscenetobuild={testScenePath2}" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.True(cliProjectSetup.ScenesToAddToBuild.Contains(testScenePath1));
        Assert.True(cliProjectSetup.ScenesToAddToBuild.Contains(testScenePath2));
    }

    [Test]
    public void VerifyOption_OpenXRFeatures()
    {
        // Arrange
        var args = new[] { $"-openxrfeatures=r:MockRuntime,OculusQuestFeature" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("r:MockRuntime,OculusQuestFeature", cliProjectSetup.PlatformSettings.OpenXRFeatures);
    }

    [Test]
    public void VerifyOption_RunDeviceAlias()
    {
        // Arrange
        var args = new[] { $"-rundevicealias=Quest" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("Quest", cliProjectSetup.PlatformSettings.RunDeviceAlias);
    }

    [Test]
    public void VerifyOption_XrTarget()
    {
        // Arrange
        var args = new[] { $"-enabledxrtarget=OculusXRSDK" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("OculusXRSDK", cliProjectSetup.PlatformSettings.XrTarget);
    }

    [Test]
    public void VerifyOption_XrTarget_Alternate()
    {
        // Arrange
        var args = new[] { $"-enabledxrtargets=OculusXRSDK" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("OculusXRSDK", cliProjectSetup.PlatformSettings.XrTarget);
    }

    [Test]
    public void VerifyOption_SimulationMode()
    {
        // Arrange
        var args = new[] { $"-simulationmode=HoloLens" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("HoloLens", cliProjectSetup.PlatformSettings.SimulationMode);
    }

    [Test]
    public void VerifyOption_DeviceRuntimeVersion()
    {
        // Arrange
        var args = new[] { $"-deviceruntimeversion=2.8.9" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("deviceruntimeversion|2.8.9", cliProjectSetup.PlatformSettings.DeviceRuntimeVersion);
    }

    [Test]
    public void VerifyOption_FfrLevel()
    {
        // Arrange
        var args = new[] { $"-ffrlevel=2" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("ffrlevel|2", cliProjectSetup.PlatformSettings.FfrLevel);
    }

    [Test]
    public void VerifyOption_AndroidTargetArchitecture()
    {
        // Arrange
        var args = new[] { $"-androidtargetarchitecture=ARMv7" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("ARMv7", cliProjectSetup.PlatformSettings.AndroidTargetArchitecture.ToString());
    }

    [Test]
    public void VerifyOption_Vsync()
    {
        // Arrange
        var args = new[] { $"-vsync=2" };
        var cliProjectSetup = new CliProjectSetup(args);

        // Act
        cliProjectSetup.ParseCommandLineArgs();

        // Assert
        Assert.AreEqual("2", cliProjectSetup.PlatformSettings.Vsync);
    }
}
