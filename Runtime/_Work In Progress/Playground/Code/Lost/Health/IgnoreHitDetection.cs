//-----------------------------------------------------------------------
// <copyright file="IgnoreHitDetection.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System.Collections.Generic;
    using UnityEngine;

    public class IgnoreHitDetection : MonoBehaviour, IValidate
    {
        private static readonly Dictionary<int, IgnoreHitDetection> IgnoreHitDetectionColliders = new Dictionary<int, IgnoreHitDetection>();

#pragma warning disable 0649
        [Tooltip("The Colliders that ignore hit detection")]
        [SerializeField] private List<Collider> colliders;
#pragma warning restore 0649

        public static bool TryGetIgnoreHitDection(Collider collider, out IgnoreHitDetection ignoreHitDetection)
        {
            return IgnoreHitDetectionColliders.TryGetValue(collider.GetInstanceID(), out ignoreHitDetection);
        }

        public void Validate(List<ValidationError> errors)
        {
            this.ValidateHasValues(errors, this.colliders, nameof(this.colliders));
        }

        private void OnEnable()
        {
            for (int i = 0; i < this.colliders.Count; i++)
            {
                IgnoreHitDetectionColliders.Add(this.colliders[i].GetInstanceID(), this);
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < this.colliders.Count; i++)
            {
                IgnoreHitDetectionColliders.Remove(this.colliders[i].GetInstanceID());
            }
        }
    }
}

#endif
