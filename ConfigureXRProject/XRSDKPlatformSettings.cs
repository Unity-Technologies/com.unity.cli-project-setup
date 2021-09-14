using System;
using System.IO;
using System.Linq;
using com.unity.cliprojectsetup;
using ConfigureProject;
#if XR_SDK
#if UNITY_EDITOR
using UnityEditor.XR.Management;
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.XR.Management;

namespace ConfigureXRProject
{
    public abstract class XrSdkPlatformSettings<T, U> : XRPlatformSettings<PlatformSettings>
    where T : ScriptableObject
    where U : XRLoader
    {
        private static readonly string xrsdkTestXrSettingsPath = "Assets/XR/Settings/Test Settings.asset";

        protected T xrSettings;
        protected abstract string xrConfigName { get; }
        protected abstract string CmdlineParam { get; }

        protected virtual void CreateXRSettingsInstance()
        {
            xrSettings = ScriptableObject.CreateInstance<T>();
        }

        protected override void ConfigureXr(PlatformSettings platformSettings)
        {
            if (!string.Equals(CmdlineParam, platformSettings.XrTarget, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
#if !UNITY_2020_OR_NEWER
            PlayerSettings.virtualRealitySupported = false;
#endif

            // Create our own test version of xr general settings.
            var xrGeneralSettings = ScriptableObject.CreateInstance<XRGeneralSettings>();
            var managerSettings = ScriptableObject.CreateInstance<XRManagerSettings>();
            var buildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();

            EnsureArgumentsNotNull(xrGeneralSettings, buildTargetSettings, managerSettings);


            xrGeneralSettings.Manager = managerSettings;

            SetupLoader(xrGeneralSettings, buildTargetSettings, managerSettings);

            CreateXRSettingsInstance();

            if (xrSettings == null)
            {
                throw new ArgumentNullException(
                    $"Tried to instantiate an instance of {typeof(T).Name} but it is null.");
            }

            SetRenderMode(platformSettings);

            ApplyLoaderSettings(buildTargetSettings);

            // Add xrSettings to Preloaded assets so XR Mgmt can see it.
            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, buildTargetSettings, true);
        }

        public virtual void ApplyLoaderSettings(XRGeneralSettingsPerBuildTarget buildTargetSettings)
        {
            AssetDatabase.AddObjectToAsset(xrSettings, xrsdkTestXrSettingsPath);
            AssetDatabase.SaveAssets();
            EditorBuildSettings.AddConfigObject(xrConfigName, xrSettings, true);
        }

        public abstract void SetRenderMode(PlatformSettings platformSettings);

        private static void SetupLoader(XRGeneralSettings xrGeneralSettings,
            XRGeneralSettingsPerBuildTarget buildTargetSettings,
            XRManagerSettings managerSettings)
        {
            var loader = ScriptableObject.CreateInstance<U>();

            if (loader == null)
            {
                throw new ArgumentNullException(
                    $"Tried to instantiate an instance of {typeof(U).Name}, but it is null.");
            }

            loader.name = loader.GetType().Name;

            xrGeneralSettings.Manager.loaders.Add(loader);

            buildTargetSettings.SetSettingsForBuildTarget(EditorUserBuildSettings.selectedBuildTargetGroup,
                xrGeneralSettings);

            EnsureXrGeneralSettingsPathExists(xrsdkTestXrSettingsPath);

            AssetDatabase.CreateAsset(buildTargetSettings, xrsdkTestXrSettingsPath);

            AssetDatabase.AddObjectToAsset(xrGeneralSettings, xrsdkTestXrSettingsPath);
            AssetDatabase.AddObjectToAsset(managerSettings, xrsdkTestXrSettingsPath);
            AssetDatabase.AddObjectToAsset(loader, xrsdkTestXrSettingsPath);
        }

        private static void EnsureXrGeneralSettingsPathExists(string testXrGeneralSettingsPath)
        {
            var settingsPath = Path.GetDirectoryName(testXrGeneralSettingsPath);
            if (!AssetDatabase.IsValidFolder(settingsPath))
            {
                // The parent folder must already exist before creating the folder
                // Thus itereate through each folder in the hierarchy and create each individually
                // Ref: https://docs.unity3d.com/2021.2/Documentation/ScriptReference/AssetDatabase.CreateFolder.html 
                var folders = settingsPath.Split(Path.DirectorySeparatorChar);
                for (int i=0; i < folders.Length; i++)
                {
                    if (!AssetDatabase.IsValidFolder(folders[i]))
                    {
                        var parentFolder = string.Join(Path.DirectorySeparatorChar.ToString(), folders, 0, i);
                        if ( string.IsNullOrEmpty(AssetDatabase.CreateFolder(parentFolder, folders[i])) )
                        {
                            throw new Exception(string.Format("Failed to create folder {0}/{1}", parentFolder, folders[i]));
                        }
                    }
                }
            }
        }

        private static void EnsureArgumentsNotNull(XRGeneralSettings xrGeneralSettings,
            XRGeneralSettingsPerBuildTarget buildTargetSettings, XRManagerSettings managerSettings)
        {
            EnsureArgumentNotNull(xrGeneralSettings);
            EnsureArgumentNotNull(buildTargetSettings);
            EnsureArgumentNotNull(managerSettings);
        }

        private static void EnsureArgumentNotNull(object arg)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(nameof(arg));
            }
        }
    }
}
#endif // XR_SDK