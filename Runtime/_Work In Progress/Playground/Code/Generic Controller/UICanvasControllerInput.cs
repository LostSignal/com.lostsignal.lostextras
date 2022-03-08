//-----------------------------------------------------------------------
// <copyright file="UICanvasControllerInput.cs" company="Lost Signal LLC">
//     Copyright (c) Lost Signal LLC. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

#if UNITY

namespace Lost
{
    using UnityEngine;
    using UnityEngine.Serialization;

    public class UICanvasControllerInput : MonoBehaviour
    {
        [Header("Output")]
        [FormerlySerializedAs("starterAssetsInputs")]
        public GenericControllerInputs genericControllerInputs;

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            genericControllerInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            genericControllerInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            genericControllerInputs.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            genericControllerInputs.SprintInput(virtualSprintState);
        }
    }
}

#endif
