using com.unity.cliprojectsetup;
using UnityEditor;

public class CliProjectSetupMock : CliProjectSetup
{

    public CliProjectSetupMock(string[] args = null)
        : base(args)
    {
    }

    public EditorBuildSettingsScene[] Scenes;

    protected override void SetEditorBuildSettingsScenes(EditorBuildSettingsScene[] buildSettingScenes)
    {
        Scenes = buildSettingScenes;
    }
}