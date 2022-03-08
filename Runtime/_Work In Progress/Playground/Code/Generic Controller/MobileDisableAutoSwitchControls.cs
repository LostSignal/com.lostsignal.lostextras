
#if UNITY

namespace Lost
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    //// The PlayerInput component has an auto-switch control scheme action that allows automatic changing of connected devices.
    //// IE: Switching from Keyboard to Gamepad in-game.
    //// When built to a mobile phone; in most cases, there is no concept of switching connected devices as controls are typically driven through what is on the device's hardware (Screen, Tilt, etc)
    //// In Input System 1.0.2, if the PlayerInput component has Auto Switch enabled, it will search the mobile device for connected devices; which is very costly and results in bad performance.
    //// This is fixed in Input System 1.1.
    //// For the time-being; this script will disable a PlayerInput's auto switch control schemes; when project is built to mobile.

    public class MobileDisableAutoSwitchControls : MonoBehaviour
    {
        #pragma warning disable 0649
        [Header("Target")]
        [SerializeField] private PlayerInput playerInput;
        #pragma warning restore 0649

        private void Start()
        {
            var platform = Application.platform;
            if (Application.isEditor == false && (platform == RuntimePlatform.Android || platform == RuntimePlatform.IPhonePlayer))
            {
                this.DisableAutoSwitchControls();
            }
        }

        private void DisableAutoSwitchControls()
        {
            this.playerInput.neverAutoSwitchControlSchemes = true;
        }
    }
}

#endif
