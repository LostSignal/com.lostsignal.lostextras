//-----------------------------------------------------------------------
// <copyright file="ValidationError.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

////
//// TODO [bgish]: Need to update this class to use the UpdateManager, and perhaps
////               make sure it doesn't update if the object isn't in view.
////
//// TODO [bgish]: Remove the Camera.main.transform.position reference
////

#if UNITY

namespace Lost
{
    using System.Runtime.CompilerServices;
    using UnityEngine;
    using UnityEngine.UI;

    public class WorldspaceHealthBar : MonoBehaviour
    {
#pragma warning disable 0649
        [Tooltip("Health component to track")]
        [SerializeField] private Health health;

        [Tooltip("Image component displaying health left")]
        [SerializeField] private Image healthBarImage;

        [Tooltip("The floating healthbar pivot transform")]
        [SerializeField] private Transform healthBarPivot;

        [Tooltip("Whether the health bar is visible when at full health or not")]
        [SerializeField] private bool hideIfHealthIsFull = true;
#pragma warning restore 0649

        private void Update()
        {
            // Update health bar value
            this.healthBarImage.fillAmount = this.health.currentHealth / this.health.maxHealth;

            // Rotate health bar to face the camera/player
            this.healthBarPivot.LookAt(Camera.main.transform.position);

            // Hide health bar if needed
            if (this.hideIfHealthIsFull)
            {
                this.healthBarPivot.gameObject.SetActive(this.healthBarImage.fillAmount != 1.0f);
            }   
        }
    }
}

#endif
