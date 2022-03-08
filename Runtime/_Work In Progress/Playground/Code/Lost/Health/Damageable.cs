//-----------------------------------------------------------------------
// <copyright file="Damageable.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using UnityEngine;

    public class Damageable : MonoBehaviour, IValidate
    {
        private static readonly Dictionary<int, Damageable> DamangeableColliders = new Dictionary<int, Damageable>();

        #pragma warning disable 0649
        [Tooltip("The Health component this component applies damage to.")]
        [SerializeField] private Health health;

        [Tooltip("Multiplier to apply to the received damage")]
        [SerializeField] private float damageMultiplier = 1f;

        [Range(0, 1)]
        [Tooltip("Multiplier to apply to self damage")]
        [SerializeField] private float sensibilityToSelfdamage = 0.5f;

        [Tooltip("The Colliders that can take damage")]
        [SerializeField] private List<Collider> colliders;
        #pragma warning restore 0649

        public Health Health
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => this.health;
        }

        public static bool TryGetDamageable(Collider collider, out Damageable damageable)
        {
            return DamangeableColliders.TryGetValue(collider.GetInstanceID(), out damageable);
        }

        public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource)
        {
            if (this.health)
            {
                float totalDamage = damage;

                // Skip the crit multiplier if it's from an explosion
                if (isExplosionDamage == false)
                {
                    totalDamage *= this.damageMultiplier;
                }

                // Potentially reduce damages if inflicted by self
                if (this.health.gameObject == damageSource)
                {
                    totalDamage *= this.sensibilityToSelfdamage;
                }

                // Apply the damages
                this.health.TakeDamage(totalDamage, damageSource);
            }
        }

        public void Validate(List<ValidationError> errors)
        {
            this.ValidateNotNull(errors, this.health, nameof(this.health));
            this.ValidateHasValues(errors, this.colliders, nameof(this.colliders));
        }

        private void OnValidate()
        {
            if (this.colliders == null)
            {
                this.colliders = new List<Collider>();
            }
        }

        private void OnEnable()
        {
            for (int i = 0; i < this.colliders.Count; i++)
            {
                DamangeableColliders.Add(this.colliders[i].GetInstanceID(), this);
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < this.colliders.Count; i++)
            {
                DamangeableColliders.Remove(this.colliders[i].GetInstanceID());
            }
        }
    }
}

#endif
