//-----------------------------------------------------------------------
// <copyright file="DestroyOnAwake.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using UnityEngine;

    // NOTE [bgish]: this class doesn't make sense, is it even used anymore?  should it be?
    public class DestroyOnAwake : MonoBehaviour
    {
        private void Awake()
        {
            PoolManager.DestroyImmediate(this.gameObject);
        }
    }
}

#endif
