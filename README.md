# Unity CLI Project Setup

Provides a command line parser and options to set editor, build, player, and other Unity settings when running Unity from the command line.

The code used for the setup options in the package originally (2017) predate many of the corresponding CLI options that UTR now has. A few teams at the time needed an easy way to reuse test project under various configuration settings, so this package was created to help with this. In practice some of these same command line switches exist for UTR, but they are preserved here for backward compatibility.

## Section Links
- [Player and Build Settings Setup Options](#player-and-build-settings-setup-options)
- [Performance Test Metadata Options](#performance-test-metadata-options)
- [Examples using the Unity CLI Project Setup options](#examples-using-the-unity-cli-project-setup-options)

## User Guide 

The CLI Project Setup package is designed to be used this way.

1. Create a static Editor method in your project that can be passed to the editor from the CLI using the `-executemethod` option. In this static Editor method, create a CliProjectSetup object, then call the ParseCommandLineArgs and ConfigureFromCmdlineArgs like the example below.
```
using com.unity.cliprojectsetup;

public class Editor
{
    // Call this method using Unity's -executeMethod CLI command in the build jobs to parse setup args
    public static void Setup()
    {
        var cliProjectSetup = new CliProjectSetup();
        cliProjectSetup.ParseCommandLineArgs();
        cliProjectSetup.ConfigureFromCmdlineArgs();
    }
}
```

2. Add the Unity command line option `-executemethod Editor.Setup` to this invokation as well. This will ensure that the `Setup()` method in the package's `Editor` class is executed when the test project is opened, ensuring the CLI options you specified in step one are correctly applied.
3. Add the CLI options you need for your specific project configuration to your invokation of either Unity or UTR.

### Examples using the Unity CLI Project Setup options

**Using the CLI Project Setup options from a direct invokation of Unity**
```
Unity.exe --architecture=arm64  -colorspace=Linear -enabledxrtargets=OculusXRSDK -executemethod Editor.Setup -playergraphicsapi=OpenGLES3 -stereoRenderingMode=Multiview --platform=Android --scripting-backend=IL2CPP -projectPath D:\unity2\Tests\SRPTests\Projects\UniversalGraphicsTest_Foundation
```
In the example above, the following options are interpreted by the CLI Project Setup package

`-colorspace=Linear -enabledxrtargets=OculusXRSDK -playergraphicsapi=OpenGLES3 -stereoRenderingMode=Multiview`

while these options are interpreted by the Unity editor

`-executemethod Editor.Setup -playergraphicsapi=OpenGLES3 --platform=Android --scripting-backend=IL2CPP -projectPath D:\unity2\Tests\SRPTests\Projects\UniversalGraphicsTest_Foundation`

As noted earlier though, the `executemethod` option above runs a static Editor method from the CLI Project setup package that applies the settings.


**Using the CLI Project Setup options from an invokation of UTR, the build stage of a split-build-and-run**
```
utr --architecture=arm64 --artifacts_path=test-results --build-only --editor-location=.Editor --extra-editor-arg="-enabledxrtargets=OculusXRSDK" --extra-editor-arg="-executemethod" --extra-editor-arg="Editor.Setup" --extra-editor-arg="-playergraphicsapi=OpenGLES3" --extra-editor-arg="-colorspace=Linear"  --extra-editor-arg="-stereoRenderingMode=Multiview" --platform=Android --player-save-path=players --scripting-backend=IL2CPP --suite=playmode  --testproject=D:\unity2\Tests\SRPTests\Projects\UniversalGraphicsTest_Foundation 
```
In the example above, the following options are interpreted by the CLI Project Setup package

`--extra-editor-arg="-enabledxrtargets=OculusXRSDK" --extra-editor-arg="-playergraphicsapi=OpenGLES3" --extra-editor-arg="-colorspace=Linear"  --extra-editor-arg="-stereoRenderingMode=Multiview"`

while these options are interpreted by the Unity editor

`--architecture=arm64 --artifacts_path=test-results --build-only --editor-location=.Editor --extra-editor-arg="-executemethod" --extra-editor-arg="Editor.Setup" --platform=Android --player-save-path=players --scripting-backend=IL2CPP --suite=playmode  --testproject=D:\unity2\Tests\SRPTests\Projects\UniversalGraphicsTest_Foundation `

As noted earlier though, the `executemethod` option above runs a static Editor method from the CLI Project setup package that applies the settings.

### Player and Build Settings Setup Options
These options are used to adjust player and build settings before building a player for test. They are set in an Editor method discussed in the beginning of this [User Guide](#user-guide) near the end of the project opening phase.

| Option Name   | Description
|---------------|------------|
|`-scripting-backend=`</br>`-scriptingbackend=`|Scripting backend to use. Values: IL2CPP, Mono|
|`-playergraphicsapi=`|Graphics API based on GraphicsDeviceType. Values: <br><ul><li>GameCoreScarlett</li><li>OpenGL2</li><li>Direct3D9</li><li>Direct3D11</li><li>PlayStation3</li><li>Null</li><li>Xbox360</li><li>OpenGLES2</li><li>OpenGLES3</li><li>PlayStationVita</li><li>PlayStation4</li><li>XboxOne</li><li>PlayStationMobile</li><li>Metal</li><li>OpenGLCore</li><li>Direct3D12</li><li>N3DS</li><li>Vulkan</li><li>Switch</li><li>XboxOneD3D12</li><li>GameCoreXboxOne</li><li>GameCoreXboxSeries</li><li>PlayStation5</li><li>PlayStation5NGGC</li><li>WebGPU</li></ul>|
|`-colorspace=`|Colorspace to use. Values: Linear, Gamma|
|`-mtRendering-`|Disable multithreaded rendering. Enabled is default.|
|`-graphicsJobs`|Enable graphics jobs rendering. Disabled is default.|
|`-jobworkercount=`|Number of job workers to use. Default is 0 which utilizes all cores. Value range [0 , NumberOfCpuCores - 1]|
|`-apicompatibilitylevel=`|API compatibility to use. Default is NET_2_0. Values:</br><ul><li>NET_2_0</li><li>NET_2_0_Subset</li><li>NET_4_6</li><li>NET_Unity_4_8</li><li>NET_Web</li><li>NET_Micro</li><li>NET_Standard</li><li>NET_Standard_2_0</li></ul>|
|`-stripenginecode`|Enable Engine code stripping. Disabled is default.|
|`-managedstrippinglevel=`|Managed stripping level to use. Default is Disabled. Values:</br><ul><li>Disabled</li><li>Low</li><li>Medium</li><li>High</li><li>Minimal</li></ul>|
|`-scriptdebugging`|Enable scriptdebugging. Disabled is default.|
|`-addscenetobuild=`|Specify path to scene to add to the build, Path is relative to Assets folder. Use this option for each scene you want to add to the build.|
|`-openxrfeatures=`|Add array of feature names to enable for openxr. ex `[r:MockRuntime,OculusQuestFeature]` should be name of feature class. Add r: before the feature name to make it required. Required features will fail the job if not found|
|`-enabledxrtarget=`</br>`-enabledxrtargets=`|XR target to enable in player settings. Values: </br><ul><li>OpenXR</li><li>MockHMDXRSDK</li><li>OculusXRSDK</li><li>MagicLeapXRSDK</li><li>WMRXRSDK</li><li>PSVR2</li></ul>|
|`-stereorenderingmode=`</br>`-stereorenderingpath=`|When using an XR provider, the stereo rendering mode to enable. SinglePass is default. Values:</br><ul><li>None</li><li>MultiPass</li><li>SinglePass</li><li>Instancing</li><li>Multiview</li></ul>|
|`-simulationmode`|Enable Simulation modes for Windows MR in Editor.|
|`-enablefoveatedrendering`|Enable foveated rendering. Disabled is default.|
|`-androidtargetarchitecture=`|Android Target Architecture to use. ARM64 is the default value. Values: </br><ul><li>None</li><li>ARMv7</li><li>ARM64</li><li>X86</li><li>X86_64</li><li>All</li></ul> |
|`-vsync=`|Vsync value. 0 (off) is the default. Values: 0, 1, 2, 3, or 4|

### Performance Test Metadata Options
These options are used to add useful metadata to Unity tests that also use the Performance Testing Package. The metadata is often used to create additional pivots on the performance test data.

| Option Name   | Description
|---------------|------------|
|`-testsrev=`|Used to track the revision id of the tests being used.|
|`-testsrevdate=`|Used to track revision date of the tests being used.|
|`-testsbranch=`|Used to track the branch of the tests repo being used.|
|`-packageundertestname=`|Used to track the name of the package under test.|
|`-packageundertestversion=`|Used to track the version of the package under test.|
|`-packageundertestrev=`|Used to track the revision id of the package under test.|
|`-packageundertestrevdate=`|Used to track the revision date of the package under test.|
|`-packageundertestbranch=`|Used to track the repo branch of the package under test.|
|`-testprojectname=`|Used to track the name of the test project.|
|`-testprojectrevision=`|Used to track test project commit revision id|
|`-testprojectrevdate=`|Used to track the test project commit revision date|
|`-testprojectbranch=`|Used to track the repo branch of the test project.|
|`-joblink=`|URL pointing to test job in CI system.|
|`-rundevicealias=`|Specify an alias to use for the device you are running on.|

## Developer Guide  

In order to contribute to the `com.unity.cli-project-setup` package, do the following

1. Clone this git repository to your local machine  

2. Choose a test project to include the `com.unity.cli-project-setup` package in while developing and debugging it, then make the following updates to the project's manifest:

    1. Add the package to the dependencies section of the project manifest, using a local reference syntax like this but using the location that is specific to your machine  

        ```
        "com.unity.cli-project-setup": "file:D:/com.unity.cli-project-setup"
        ```

    2. The `com.unity.cli-project-setup` package requires the `com.unity.test.metadata-manager` package as a dependency, so you'll need to include this in one of three ways:  

        1. Add a local reference: Clone the `com.unity.test.metadata-manager` locally like you did for this package in the first step, and add a local dependency reference to it.  
                
            ```
            "com.unity.test.metadata-manager": "file:D:/com.unity.test.metadata-manager"
            ```

        2. If you are using the production/public Unity package repository, you'll need to add a scoped registry section to your project manifest as the `com.unity.test.metadata-manager` is published to the internal, upm-candidates package registry only. Here is an example of how to do this.  

            ```
            "scopedRegistries": [
              {
                "name": "Internal Candidate Registry",
                "url": "https://artifactory.prd.it.unity3d.com/artifactory/api/npm/upm-candidates",
                "scopes": [
                  "com.unity.test.metadata-manager"
                 ]
              }
            ],
            ```
        3. If you don't need to use the production/public Unity package repository, you can choose to use the internal package registry exclusively. Below is the manifest entry that will make this happen.  
        
            ```
            "registry": "https://artifactory.prd.cds.internal.unity3d.com/artifactory/api/npm/upm-candidates"
            ```  

    3. Add the MOQ mocking framework package to your project manifest.

        ```
        "nuget.moq": "2.0.0-pre.2"
        ```
        
    4. Add `com.unity.cli-project-setup` to the project manifest's "testables" section. This will ensure the unit tests can be seen and run from the editor's test runner tab  
        ```
        "testables": [
            "com.unity.cli-project-setup"
          ]
        ```

If you've setup up everything correctly, you should now see the tests from the EditMode tab of the Unity editor test runner tab.

## Testing changes
This package is used broadly across Graphics and XR testing. Following is a procedure for verifying changes and keeping broader support in mind.
- Ensure that you've followed the Developer Guide above and are able to run the com.unity.cli-project-setup editor tests with your changes in a Unity project. 

- Verify your own use cases for the changes you've made and add additional tests, or refactor existing ones if appropriate. All tests should pass before going to the next step.

- Run the [URP PR](https://unity-ci.cds.internal.unity3d.com/project/3/branch/trunk/jobDefinition/.yamato%2Fsrp%2Furp.yml%23urp_pr) job from the unity/unity repository. When you create a new run, use the CLI_PROJECT_SETUP_VERSION variable input field to enter a GitHub url path to your dev branch changeset like this, where `e04468fd4be965e4da3635d4ce0cf90e866bd326` is the git revision of your dev branch changes. This will enable you to run the tests with your dev branch to gain more confidence that a regression hasn't been introduced.

Example of a GitHub URL path to your dev branch below:

```
    com.unity.cli-project-setup@"ssh://git@github.cds.internal.unity3d.com/unity/com.unity.cli-project-setup.git#e04468fd4be965e4da3635d4ce0cf90e866bd326" 
```

- Run the [Pack and test all packages](https://unity-ci.cds.internal.unity3d.com/project/1166/branch/{{branchName}}/jobDefinition/.yamato%2Fupm-ci.yml%23all_package_ci) job for this repository on your branch. This will run the unit tests across several platforms and Unity version to catch potential regression.

- Verify that any other checks for this repo have passed on your PR.

- Update the changelog with a summary of your changes and increment the version appropropriately here as well as in the package.json indicating a new version.
