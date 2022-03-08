//-----------------------------------------------------------------------
// <copyright file="PooledObject.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [DisallowMultipleComponent]
    public class PooledObject : MonoBehaviour, IOnManagersReady
    {
#pragma warning disable 0649
        [ReadOnly] [SerializeField] private Pool myPool;
        [ReadOnly] [SerializeField] private Transform myTransform;
        [ReadOnly] [SerializeField] private List<MonoBehaviour> myPoolableComponents;
#pragma warning disable 0649

        public void OnManagersReady()
        {
            Debug.Assert(this.myPool, "PooledObject doesn't have a valid Pool!", this);
            Debug.Assert(this.myTransform != null, "PooledObject doesn't have a valid Transform!", this);
            Debug.Assert(this.myPoolableComponents != null, "PooledObject doesn't have a valid poolable components.", this);
        }

        internal void TakenFromPool(Vector3 position, Quaternion rotation, Transform parent)
        {
            for (int i = 0; i < this.myPoolableComponents.Count; i++)
            {
                if (this.myPoolableComponents[i] is IPoolableObject poolableObject)
                {
                    poolableObject.TakenFromPool();
                }
                else
                {
                    Debug.LogError("Give a good descripting", this);
                }
            }

            this.ActivateObject(position, rotation, parent);
        }

        internal void ReturnToPool()
        {
            this.myPool.ReturnToPooledObject(this);

            for (int i = 0; i < this.myPoolableComponents.Count; i++)
            {
                if (this.myPoolableComponents[i] is IPoolableObject poolableObject)
                {
                    poolableObject.ReturnedToPool();
                }
                else
                {
                    Debug.LogError("Give a good descripting", this);
                }
            }

            this.DeactivateObject();
        }

        internal void Initialize(Pool pool)
        {
            if (this.myPool != pool)
            {
                this.myPool = pool;
            }

            if (this.myTransform != this.transform)
            {
                this.myTransform = this.transform;
            }

            var components = this.GetComponentsInChildren<MonoBehaviour>(true).Where(x => x is IPoolableObject).ToList();

            if (this.myPoolableComponents == null || this.myPoolableComponents.SequenceEqual(components) == false)
            {
                this.myPoolableComponents = components;
            }
        }

        private void Awake() => ManagersReady.Register(this);

        private void DeactivateObject()
        {
            this.gameObject.SetActive(false);

            if (Application.isPlaying && this.myTransform.parent != null)
            {
                this.myTransform.parent = null;
            }
        }

        private void ActivateObject(Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (parent != null)
            {
                this.myTransform.SetParent(parent);
            }

            this.myTransform.SetPositionAndRotation(position, rotation);
            this.gameObject.SetActive(true);
        }

        private void OnValidate()
        {
            this.Initialize(this.myPool);
        }
    }
}

#endif
