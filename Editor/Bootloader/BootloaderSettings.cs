#pragma warning disable

//-----------------------------------------------------------------------
// <copyright file="BootloaderBuildConfig.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using System;
    using System.Collections.Generic;
    using Lost.BuildConfig;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    [BuildConfigSettingsOrder(8)]
    public class BootloaderSettings : BuildConfigSettings
    {
        #pragma warning disable 0649
        [SerializeField] private string bootloaderResourcePath = Bootloader.DefaultBootloaderResourcePath;
        [SerializeField] private BootloaderConfigLocation bootloaderConfigLocation;
        
        [ShowIf("bootloaderConfigLocation", BootloaderConfigLocation.RuntimeConfigSettings)]
        [SerializeField] private BootloaderConfig bootloaderConfig;

        [Header("Scenes To Ignore")]
        [Tooltip("A ';' delimited list of scene names that should not startup the bootloader process.")]
        [SerializeField] private string scenesToIgnore;
        #pragma warning restore 0649

        public override string DisplayName => "Bootloader Settings";

        public override bool IsInline => false;

        public override void GetRuntimeConfigSettings(BuildConfig.BuildConfig buildConfig, Dictionary<string, string> runtimeConfigSettings)
        {
            var settings = buildConfig.GetSettings<BootloaderSettings>();

            if (settings == null)
            {
                return;
            }

            // Bootloader Resource Path
            runtimeConfigSettings.Add(Bootloader.BootloaderResourcePath, settings.bootloaderResourcePath);

            // Bootloader Location
            runtimeConfigSettings.Add(Bootloader.BootloaderConfigLocation, ((int)settings.bootloaderConfigLocation).ToString());

            // Bootloader Config
            if (settings.bootloaderConfigLocation == BootloaderConfigLocation.RuntimeConfigSettings)
            {
                runtimeConfigSettings.Add(Bootloader.BootloaderConfig, JsonUtil.Serialize(settings.bootloaderConfig));
            }

            // Ignored Scenes
            runtimeConfigSettings.Add(Bootloader.BootloaderIgnoreSceneNames, settings.scenesToIgnore);
        }

        public override void DrawSettings(BuildConfigSettings settings, SerializedProperty settingsSerializedProperty, float width)
        {
            base.DrawSettings(settings, settingsSerializedProperty, width);
        }
    }
}
