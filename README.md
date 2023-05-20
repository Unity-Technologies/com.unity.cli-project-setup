# com.unity.cli-project-setup

Provides a command line parser and options to set editor, build, player, and other Unity settings when running Unity from the command line

## Developer Guide  

In order to contribute to the com.unity.cli-project-setup package, do the following

1. Clone this git repository to your local machine  

2. Choose a test project to include the com.unity.cli-project-setup package in while developing and debugging it.

    1. Add the package to the dependencies section of the project manifest, using a local reference syntax like this but using the location that is specific to your machine  

        ```
        "com.unity.cli-project-setup": "file:D:/com.unity.cli-project-setup"
        ```

    2. The com.unity.cli-project-setup package requires the com.unity.test.metadata-manager package as a dependency, so you'll need to include this in one of three ways:  

        1. Add a local reference: Clone the com.unity.test.metadata-manager locally like you did for this package in the first step, and add a local dependency reference to it.  
                
        ```
        "com.unity.test.metadata-manager": "file:D:/com.unity.test.metadata-manager"
        ```

        2. Add a scoped registry section to your project manifest: The com.unity.test.metadata-manager is published to the internal, upm-candidates package registry. If your project needs to use the production (non-internal) Unity package registry, you'll need to add a scopedRegistry section to your project manifest in order to access it  

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
        3. Update the project manifest to use the internal package registry exlusively: If using the internal, upm-candidates, registry is acceptable for your project, just ensure you have this entry in the project manifest.  
        
        ```
        "registry": "https://artifactory.prd.cds.internal.unity3d.com/artifactory/api/npm/upm-candidates"
        ```  

        4. Add the MOQ mocking framework package to your project manifest.

        ```
        "nuget.moq": "2.0.0-pre.2"
        ```
        
3. Add com.unity.cli-project-setup to the project manifest's "testables" section. This will ensure the unit tests can be seen and run from the editor's test runner tab  
```
"testables": [
    "com.unity.cli-project-setup"
  ]
```

If you've setup up everything correctly, you should now see the tests from the EditMode tab of the Unity editor test runner tab.

## Testing changes
This package is used broadly across Graphics and XR testing. Following is a procedure for verifying changes and keeping broader support in mind.
- Verify your own use cases.
- Verify that the "Pack and test all packages" yamato job passes for your branch.
- Verify that any other checks for this repo have passed on your PR.
- Update the changelog and package.json to indicate a new version. To start, also postfix "-preview.1" to your candidate version.  Run the "Publish CliProjectSetup" yamato job.
- Create branches for the packages [unity.graphictests.performance.universal](https://github.cds.internal.unity3d.com/unity/unity.graphictests.performance.universal) and [com.unity.testing.graphics-performance](https://github.cds.internal.unity3d.com/unity/com.unity.testing.graphics-performance).  Change the package.json for each package to indicate dependency on your candidate version of this package.
- In the Graphics repo, make a new branch and edit [lines 36 and 37 of this file](https://github.com/Unity-Technologies/Graphics/blob/master/.yamato/config/universal_perf_boatattack.metafile) to point to the branches in the previous step (eg add #your-branch-name to the end of the git url).
- Clone the [Graphics repository](https://github.com/Unity-Technologies/Graphics)
- Clone the [gfx-sdet-tools](https://github.cds.internal.unity3d.com/unity/gfx-sdet-tools) repository and [run the yml generator script](https://github.cds.internal.unity3d.com/unity/gfx-sdet-tools/tree/master/yml-generator#running-the-script) targeting the Graphics repo (or ask a graphics SDET for help).
- Run the "VikingVillage_URP PR Job - trunk" yamato job on my Graphics repo branch. Verify all tests pass
- If all tests pass, remove the ".1" or "preview.1" postfix to match the previously released version's convention. Run the "Publish CliProjectSetup" yamato job.
- Merge your PR
- Create PRs for the packages [unity.graphictests.performance.universal](https://github.cds.internal.unity3d.com/unity/unity.graphictests.performance.universal) and [com.unity.testing.graphics-performance](https://github.cds.internal.unity3d.com/unity/com.unity.testing.graphics-performance). Change the package.json for each package to indicate dependency on your newly published version of this package.
