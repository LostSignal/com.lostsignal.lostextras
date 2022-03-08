//-----------------------------------------------------------------------
// <copyright file="HealthBar.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.Serialization;
    using UnityEngine.UI;

    public class HealthBar : MonoBehaviour, IValidate
    {
#pragma warning disable 0649
        [Tooltip("Image component dispplaying current health")]
        [SerializeField] private Image healthFillImage;

        [FormerlySerializedAs("m_PlayerHealth")]
        [SerializeField] private Health health;
#pragma warning restore 0649

        public void Validate(List<ValidationError> errors)
        {
            this.ValidateNotNull(errors, this.healthFillImage, nameof(this.healthFillImage));
            this.ValidateNotNull(errors, this.health, nameof(this.health));
        }

        private void Awake()
        {
            this.health.onHealthChanged += this.UpdateImageFill;
        }

        private void OnDestroy()
        {
            this.health.onHealthChanged -= this.UpdateImageFill;
        }

        private void UpdateImageFill()
        {
            this.healthFillImage.fillAmount = this.health.currentHealth / this.health.maxHealth;
        }
    }
}

#endif
