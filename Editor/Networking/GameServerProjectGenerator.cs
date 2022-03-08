//-----------------------------------------------------------------------
// <copyright file="GameServerProjectGenerator.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace Lost
{
    using Lost.BuildConfig;
    using UnityEditor;
    using UnityEngine;

    public class GameServerProjectGenerator : CSharpProjectGenerator
    {
        static GameServerProjectGenerator()
        {
            // Making sure all default assets are created
            EditorApplication.delayCall += () =>
            {
                GetInstance();
            };
        }

        public static GameServerProjectGenerator GetInstance()
        {
            const string GameServerEditorBuildSettingsId = "com.lostsignal.gameserver";

            if (EditorBuildSettings.TryGetConfigObject(GameServerEditorBuildSettingsId, out GameServerProjectGenerator gameServerGenerator) == false || !gameServerGenerator)
            {
                gameServerGenerator = LostCore.CreateScriptableObject<GameServerProjectGenerator>("a8857427ec41cb94985737f62f7e6383", "GameServerGenerator.asset");
                EditorBuildSettings.AddConfigObject(GameServerEditorBuildSettingsId, gameServerGenerator, true);
            }

            return gameServerGenerator;
        }
    }
}
