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
- Ensure that you've followed the Developer Guide above and are able to run the com.unity.cli-project-setup editor tests with your changes in a Unity project. 

- Verify your own use cases for the changes you've made and add additional tests, or refactor existing ones if appropriate. All tests should pass before going to the next step.

- Run the [URP PR](https://unity-ci.cds.internal.unity3d.com/project/3/branch/trunk/jobDefinition/.yamato%2Fsrp%2Furp.yml%23urp_pr) job from the unity/unity repository. When you create a new run, use the CLI_PROJECT_SETUP_VERSION variable input field to enter a github url path to your dev branch changeset like this, where `e04468fd4be965e4da3635d4ce0cf90e866bd326` is the git revision of your dev branch changes. This will enable you to run the tests with your dev branch to gain more confidence that a regression hasn't been introduced.

```
    com.unity.cli-project-setup@"ssh://git@github.cds.internal.unity3d.com/unity/com.unity.cli-project-setup.git#e04468fd4be965e4da3635d4ce0cf90e866bd326" 
```

- Run the [Pack and test all packages](https://unity-ci.cds.internal.unity3d.com/project/1166/branch/{{branchName}}/jobDefinition/.yamato%2Fupm-ci.yml%23all_package_ci) job for this repository on your branch. This will run the unit tests across several platforms and Unity version to catch potential regression.

- Verify that any other checks for this repo have passed on your PR.

- Update the changelog with a summary of your changes and increment the version appropropriately here as well as in the package.json indicating a new version.
