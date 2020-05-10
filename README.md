# com.unity.cli-config-manager
Provides a command line parser and options to set editor, build, player, and other Unity settings when running Unity from the command line

# Methods
## public void ConfigureFromCmdlineArgs()

Parses command line args using args returned from `Environment.GetCommandLineArgs()` and matches up values based on Options below. Then sets the values to the appropriate build/player settings in the editor. Meant to be run prior to building the player or running a test.


# Options
Options Recognized from `Environment.GetCommandLineArgs()`

`-scriptingbackend=<ScriptingBackend>`

        IL2CPP is default. Values: IL2CPP, Mono
        
`-simulationmode=<SimulationMode>`

        Enable Simulation modes for Windows MR in Editor. Values: HoloLens, WindowsMR, Remoting
  
`-enabledxrtarget=<XrTargetToEnable>`

        XR target to enable in player settings. Values: Oculus, OpenVR, cardboard, daydream, MockHMD, OculusXRSDK
        
`-playergraphicsapi=<GraphicsApi>`

        Graphics API based on GraphicsDeviceType. Values: Direct3D11, OpenGLES2, OpenGLES3, PlayStation4, XboxOne, Metal, OpenGLCore, Direct3D12, Switch, XboxOneD3D12
        
`-colorspace=<ColorSpace>`

        Linear or Gamma color space. Default is Gamma. Values: Linear, Gamma
        
`-stereorenderingpath=<StereoRenderingPath>`
        
        Stereo rendering path to enable. 
                Legacy VR Values: MultiPass, SinglePass, Instancing
                Oculus XR SDK Desktop Values: MultiPass, SinglePassInstanced
                Oculus XR SDK Android Values: MultiPass, MultiView
                
`-mtRendering`

        Enable or disable multithreaded rendering. Enabled is default. 
        Append '-' to disable: -mtRendering-
        
`-graphicsJobs`

        Enable graphics jobs rendering. Disabled is default. 
        Append '-' to disable: -graphicsJobs-
         
`-minimumandroidsdkversion=`

        Minimum Android SDK Version (Integer) to use.
        
`-targetandroidsdkversion=`

        Target Android SDK Version (Integer) to use.
        
`-appleDeveloperTeamID=`

        Apple Developer Team ID. Use for deployment and running tests on iOS device.
        
`-iOSProvisioningProfileID=`

        iOS Provisioning Profile ID. Use for deployment and running tests on iOS device.
