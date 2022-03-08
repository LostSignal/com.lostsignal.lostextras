
#if UNITY

namespace Lost
{
    using UnityEngine;

    public class GenericControllerUIInputs : MonoBehaviour
    {

        [Header("Output")]
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
