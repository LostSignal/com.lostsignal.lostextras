//-----------------------------------------------------------------------
// <copyright file="BulletProjectile.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using UnityEngine;

    public class BulletProjectile : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private Rigidbody bulletRigidbody;
        [SerializeField] private float speed = 100;
#pragma warning restore 0649

        private Vector3 startPosition;

        private void OnTriggerEnter(Collider other)
        {
            this.DestoryBullet();
        }

        public void Shoot()
        {
            this.startPosition = this.transform.position;
            this.bulletRigidbody.velocity = this.transform.forward * this.speed;
        }

        private void Update()
        {
            if (Vector3.SqrMagnitude(this.startPosition - this.transform.position) > 200.0f * 200.0f)
            {
                this.DestoryBullet();
            }
        }

        private void DestoryBullet()
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}

#endif
