//-----------------------------------------------------------------------
// <copyright file="StrictModeSettings.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using Lost.BuildConfig;
    using UnityEditor;
    using UnityEngine;

    public class StrictModeSettings : BuildConfigSettings
    {
        #pragma warning disable 0649
        [SerializeField] private bool buildInStrictMode = true;
        #pragma warning restore 0649

        public override string DisplayName => "Build Strict Mode";

        public override bool IsInline => true;

        public override BuildPlayerOptions ChangeBuildPlayerOptions(BuildConfig.BuildConfig buildConfig, BuildPlayerOptions options)
        {
            if (this.buildInStrictMode)
            {
                options.options |= BuildOptions.StrictMode;
            }
            else
            {
                options.options &= ~BuildOptions.StrictMode;
            }

            return options;
        }
    }
}
