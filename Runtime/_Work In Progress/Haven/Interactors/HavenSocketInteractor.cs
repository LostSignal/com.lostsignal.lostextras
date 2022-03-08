//-----------------------------------------------------------------------
// <copyright file="HavenExclusiveSocketInteractor.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY && USING_UNITY_XR_INTERACTION_TOOLKIT

namespace Lost.Haven
{
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    [AddComponentMenu("Haven XR/Socket/HXR Socket Interactor")]
    public class HavenSocketInteractor : XRSocketInteractor
    {
#pragma warning disable 0649
        [SerializeField] private string acceptedType;
#pragma warning restore 0649

        public string AcceptedType => this.acceptedType;

        public override bool CanSelect(IXRSelectInteractable interactable)
        {
            var monoBehavior = interactable as MonoBehaviour;

            if (monoBehavior == null)
            {
                return false;
            }

            var socketTarget = monoBehavior.gameObject.GetComponent<HavenSocketTarget>();

            if (socketTarget == null)
            {
                return false;
            }

            return base.CanSelect(interactable) && socketTarget.CanSocket(this);
        }

        public override bool CanHover(IXRHoverInteractable interactable)
        {
            var monoBehavior = interactable as MonoBehaviour;

            if (monoBehavior == null)
            {
                return false;
            }

            var selectInteractable = monoBehavior.gameObject.GetComponent<IXRSelectInteractable>();
            return base.CanSelect(selectInteractable);
        }
    }
}

#endif
