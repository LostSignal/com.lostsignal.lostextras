//-----------------------------------------------------------------------
// <copyright file="HybridBulletProjectile.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using UnityEngine;

    public class HybridBulletProjectile : MonoBehaviour, IPoolableObject
    {
#pragma warning disable 0649
        [SerializeField] private Transform myTransform;
        [SerializeField] private float speed = 200.0f;
#pragma warning restore 0649

        private Vector3 targetPosition;

        public void Shoot(Vector3 startPosition, Vector3 targetPosition)
        {
            this.myTransform.position = startPosition;
            this.myTransform.rotation = Quaternion.LookRotation((targetPosition - startPosition).normalized, Vector3.up);
            this.targetPosition = targetPosition;
        }

        private void Update()
        {
            Vector3 myPosition = this.myTransform.position;
            Vector3 moveVector = (this.targetPosition - myPosition).normalized * (this.speed * Time.deltaTime);
            Vector3 newPosition = myPosition + moveVector;

            float distanceBefore = Vector3.SqrMagnitude(myPosition - this.targetPosition);
            float distanceAfter = Vector3.SqrMagnitude(myPosition - newPosition);

            if (distanceAfter >= distanceBefore)
            {
                this.myTransform.position = this.targetPosition;
                DestoryBullet();
            }
            else
            {
                this.myTransform.position = newPosition;
            }
        }

        private void DestoryBullet()
        {
            PoolManager.DestroyGameObject(this.gameObject);
        }

        public void TakenFromPool()
        {
            this.enabled = true;
        }

        public void ReturnedToPool()
        {
            this.enabled = false;
        }
    }
}

#endif
