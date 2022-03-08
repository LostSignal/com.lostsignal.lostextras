//-----------------------------------------------------------------------
// <copyright file="Pool.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public class Pool : MonoBehaviour, IOnManagersReady
    {
#pragma warning disable 0649
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialCapacity;

        [ReadOnly]
        [SerializeField] private List<GameObject> pooledGameObjects;
#pragma warning disable 0649

        private Dictionary<int, PooledObject> pooledObjects = new Dictionary<int, PooledObject>();
        private int pooId;

        internal int PoolId
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.pooId;
        }

        public void OnManagersReady()
        {
            Debug.Assert(this.prefab != null, "Give good notification", this);
            Debug.Assert(this.initialCapacity > 0, "Give good notification", this);
            Debug.Assert(this.pooledGameObjects?.Count == this.initialCapacity, "Give good notification", this);

            if (this.pooledGameObjects != null)
            {
                for (int i = 0; i < this.pooledGameObjects.Count; i++)
                {
                    if (this.pooledGameObjects[i] != null)
                    {
                        var pooledObject = this.pooledGameObjects[i].GetComponent<PooledObject>();

                        if (pooledObject != null)
                        {
                            this.pooledObjects.Add(pooledObject.gameObject.GetInstanceID(), pooledObject);
                        }
                        else
                        {
                            Debug.LogError("Give good notification", this);
                        }
                    }
                    else
                    {
                        Debug.LogError("Give good notification", this);
                    }
                }
            }

            this.pooId = this.prefab.GetInstanceID();

            PoolManager.Instance.RegisterPool(this);
        }

        internal bool Contains(GameObject pooledGameObject)
        {
            return this.pooledGameObjects?.Contains(pooledGameObject) ?? false;
        }

        internal GameObject GetPooledObject(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (this.pooledGameObjects.Count == 0)
            {
                Debug.LogError($"Pooled Prefab {this.prefab.name} ran out of objects in it's pool, creating more at runtime.  Increase your starting capacity from {this.initialCapacity} to {this.initialCapacity * 2}.", this.prefab);
                var newPooledGameObject = GameObject.Instantiate(this.prefab);
                var newPooledObject = newPooledGameObject.GetOrAddComponent<PooledObject>();
                newPooledObject.Initialize(this);
                newPooledObject.ReturnToPool();

                this.pooledObjects.Add(newPooledGameObject.GetInstanceID(), newPooledObject);
            }

            int lastIndex = this.pooledGameObjects.Count - 1;
            GameObject pooledGameObject = this.pooledGameObjects[lastIndex];
            this.pooledGameObjects.RemoveAt(lastIndex);

            var pooledObject = this.pooledObjects[pooledGameObject.GetInstanceID()];
            pooledObject.TakenFromPool(position, rotation, parent);
            return pooledGameObject;
        }

        internal void ReturnToPooledObject(PooledObject pooledObject)
        {
            #if UNITY_EDITOR
            if (this.pooledGameObjects.Contains(pooledObject.gameObject))
            {
                Debug.LogError($"Pooled Object {pooledObject.name} has already been returned to the pool.  There is a bug!", pooledObject);
                return;
            }
            #endif

            this.pooledGameObjects.Add(pooledObject.gameObject);
        }

        private void Awake() => ManagersReady.Register(this);

        private void OnDestroy()
        {
            if (PoolManager.Instance)
            {
                PoolManager.Instance.UnregisterPool(this);
            }
        }

        #if UNITY_EDITOR

        private void OnValidate()
        {
            if (UnityEditor.BuildPipeline.isBuildingPlayer == false)
            {
                UnityEditor.EditorApplication.delayCall += this.SetupPool;
            }
        }

        private void SetupPool()
        {
            if (Application.isPlaying)
            {
                return;
            }

            if (this.pooledGameObjects == null)
            {
                this.pooledGameObjects = new List<GameObject>();
            }

            if (this.initialCapacity != this.pooledGameObjects.Count ||
                this.initialCapacity != this.transform.childCount ||
                this.initialCapacity != this.pooledGameObjects.Count)
            {
                this.transform.DestroyAllChildrenImmediate();
                this.pooledGameObjects.Clear();

                for (int i = 0; i < this.initialCapacity; i++)
                {
                    var pooledGameObject = UnityEditor.PrefabUtility.InstantiatePrefab(this.prefab, this.transform) as GameObject;
                    pooledGameObject.transform.Reset();
                    pooledGameObject.name += $" ({i})";

                    var pooled = pooledGameObject.GetOrAddComponent<PooledObject>();
                    pooled.Initialize(this);
                    pooled.ReturnToPool();
                }
            }
        }

        #endif
    }
}

#endif
