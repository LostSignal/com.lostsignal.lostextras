//-----------------------------------------------------------------------
// <copyright file="DestoryGameObjectOnBuild.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using UnityEngine;

    public class DestoryGameObjectOnBuild : MonoBehaviour
    {
#pragma warning disable 0649
        [EnumFlag]
        [SerializeField] private DevicePlatform platformsToKeep;
#pragma warning restore 0649

#if UNITY_EDITOR
        [EditorEvents.OnProcessSceneBuild]
        private static void OnProcessScene()
        {
            foreach (var destoryOnBuild in GameObject.FindObjectsOfType<DestoryGameObjectOnBuild>())
            {
                if (destoryOnBuild && destoryOnBuild.ShouldDestroyObject())
                {
                    GameObject.DestroyImmediate(destoryOnBuild.gameObject);
                }
            }
        }

        private void Awake()
        {
            if (this.ShouldDestroyObject())
            {
                GameObject.Destroy(this.gameObject);
            }
        }

        private bool ShouldDestroyObject()
        {
            return (Platform.CurrentDevicePlatform & this.platformsToKeep) == 0;
        }

#endif
    }
}

#endif
