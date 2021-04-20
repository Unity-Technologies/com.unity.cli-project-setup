# com.unity.cli-project-setup
Provides a command line parser and options to set editor, build, player, and other Unity settings when running Unity from the command line

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
