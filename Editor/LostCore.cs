//-----------------------------------------------------------------------
// <copyright file="LostCore.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System.IO;
    using Lost.Addressables;
    using Lost.BuildConfig;
    using UnityEditor;
    using UnityEngine;

    [InitializeOnLoad]
    public static class LostCore
    {
        private static readonly string OldEditorAppConfigBuildSettingsId = "com.lostsignal.appconfig";
        private static readonly string EditorBuildConfigsBuildSettingsId = "com.lostsignal.buildconfigs";

        private static readonly string LostLibraryAssetsPath = "Assets/Editor/com.lostsignal.lostlibrary";
        private static readonly string BuildConfigsAssetName = "BuildConfigs.asset";

        static LostCore()
        {
            // Making sure all default assets are created
            EditorApplication.delayCall += () =>
            {
                var buildConfigs = BuildConfigs;
            };
        }

        public static EditorBuildConfigs BuildConfigs
        {
            get
            {
                // Moving build configs if was once using the old system
                if (EditorBuildSettings.TryGetConfigObject(OldEditorAppConfigBuildSettingsId, out EditorBuildConfigs editorBuildConfigs))
                {
                    EditorBuildSettings.AddConfigObject(EditorBuildConfigsBuildSettingsId, editorBuildConfigs, false);
                    EditorBuildSettings.RemoveConfigObject(OldEditorAppConfigBuildSettingsId);
                }

                if (EditorBuildSettings.TryGetConfigObject(EditorBuildConfigsBuildSettingsId, out editorBuildConfigs) == false || !editorBuildConfigs)
                {
                    editorBuildConfigs = CreateEditorBuildConfigs();
                    EditorBuildSettings.AddConfigObject(EditorBuildConfigsBuildSettingsId, editorBuildConfigs, true);
                    EditorBuildConfigFileBuilder.GenerateBuildConfigsFile();
                }

                if (editorBuildConfigs)
                {
                    if (editorBuildConfigs.BuildConfigs == null || editorBuildConfigs.BuildConfigs.Count == 0)
                    {
                        Debug.LogError("BuildConfigs doesn't have any valid configs in it's list.");
                    }
                    else if (editorBuildConfigs.DefaultBuildConfig == null)
                    {
                        Debug.LogError("BuildConfigs doesn't have a valid default config.");
                    }
                    else if (editorBuildConfigs.RootBuildConfig == null)
                    {
                        Debug.LogError("EditorBuildConfig doesn't have a valid root config.");
                    }
                }

                return editorBuildConfigs;
            }
        }

        public static T CreateScriptableObject<T>(string guid, string assetName)
            where T : UnityEngine.Object
        {
            string assetPath = GetAssetPath(assetName);

            if (System.IO.File.Exists(assetPath) == false)
            {
                AssetDatabase.CopyAsset(UnityEditor.AssetDatabase.GUIDToAssetPath(guid), assetPath);
            }

            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        private static EditorBuildConfigs CreateEditorBuildConfigs()
        {
            string editorBuildConfigAssetPath = GetAssetPath(BuildConfigsAssetName);

            EditorBuildConfigs editorBuildConfigs;

            if (File.Exists(editorBuildConfigAssetPath) == false)
            {
                var rootConfig = new BuildConfig.BuildConfig
                {
                    Name = "Root",
                };

                AddSetting<BundleIdentifierSetting>(rootConfig);
                //// AddSetting<BootloaderSettings>(rootConfig);
                AddSetting<BuildPlayerContentSettings>(rootConfig);
                AddSetting<CloudBuildSetBuildNumber>(rootConfig);

                var devConfig = new BuildConfig.BuildConfig
                {
                    Name = "Dev",
                    IsDefault = true,
                    ParentId = rootConfig.Id,
                };

                AddSetting<DevelopmentBuildSetting>(devConfig).IsDevelopmentBuild = true;
                //// AddSetting<PlayFabSettings>(devConfig).IsDevelopmentEnvironment = true;

                var liveConfig = new BuildConfig.BuildConfig()
                {
                    Name = "Live",
                    ParentId = rootConfig.Id,
                };

                AddSetting<DevelopmentBuildSetting>(liveConfig).IsDevelopmentBuild = false;
                //// AddSetting<PlayFabSettings>(liveConfig).IsDevelopmentEnvironment = false;

                // Creating the AppConfigs scriptable object
                editorBuildConfigs = ScriptableObject.CreateInstance<EditorBuildConfigs>();
                editorBuildConfigs.BuildConfigs.Add(rootConfig);
                editorBuildConfigs.BuildConfigs.Add(devConfig);
                editorBuildConfigs.BuildConfigs.Add(liveConfig);

                CreateAsset(editorBuildConfigs, editorBuildConfigAssetPath);
            }
            else
            {
                editorBuildConfigs = AssetDatabase.LoadAssetAtPath<EditorBuildConfigs>(editorBuildConfigAssetPath);
            }

            return editorBuildConfigs;

            static T AddSetting<T>(BuildConfig.BuildConfig config)
                where T : BuildConfigSettings, new()
            {
                var newSettings = new T();
                config.Settings.Add(newSettings);
                return newSettings;
            }
        }

        private static void CreateAsset(UnityEngine.Object asset, string path)
        {
            AssetDatabase.CreateAsset(asset, path);
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
        }

        private static string GetAssetPath(string assetName)
        {
            // Making sure EditorAppConfig path exists
            string assetPath = Path.Combine(LostLibraryAssetsPath, assetName);
            string assetDirectory = Path.GetDirectoryName(assetPath);

            if (Directory.Exists(assetDirectory) == false)
            {
                Directory.CreateDirectory(assetDirectory);
            }

            return assetPath;
        }

        //// TODO [bgish]: Need to make a button somewhere in Bootloader Settings for creating these assets.  Should also
        ////               prompt user for the location instead of hard coding it.
        //// private static void CreateBootloaderAndManagers()
        //// {
        ////     var bootloader = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<Bootloader>(AssetDatabase.GUIDToAssetPath("e64035672fd9d3848956e0518ca53808")));
        ////     CreateAsset(bootloader, $"Assets/Resources/Bootloader.prefab");
        ////
        ////     var managers = GameObject.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ebe6a31cc5c4ac74ab8dae1375be0b50")));
        ////     CreateAsset(managers, $"Assets/Resources/Managers.prefab");
        //// }
    }
}
