
#if UNITY

using Lost;
using UnityEngine;

public class FPSWeaponsManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Secondary camera used to avoid seeing weapon go throw geometries")]
    public Camera weaponCamera;

    [Tooltip("Parent transform where all weapon will be added in the hierarchy")]
    public Transform weaponParentSocket;

    [Tooltip("Position for weapons when active but not actively aiming")]
    public Transform defaultWeaponPosition;

    [Tooltip("Position for weapons when aiming")]
    public Transform aimingWeaponPosition;

    [Tooltip("Position for innactive weapons")]
    public Transform downWeaponPosition;

    [Header("Weapon Bob")]
    [Tooltip("Frequency at which the weapon will move around in the screen when the player is in movement")]
    public float bobFrequency = 10.0f;

    [Tooltip("How fast the weapon bob is applied, the bigger value the fastest")]
    public float bobSharpness = 10.0f;

    [Tooltip("Distance the weapon bobs when not aiming")]
    public float defaultBobAmount = 0.05f;

    [Tooltip("Distance the weapon bobs when aiming")]
    public float aimingBobAmount = 0.02f;

    [Header("Weapon Recoil")]
    [Tooltip("This will affect how fast the recoil moves the weapon, the bigger the value, the fastest")]
    public float recoilSharpness = 50.0f;

    [Tooltip("Maximum distance the recoil can affect the weapon")]
    public float maxRecoilDistance = 0.2f;

    [Tooltip("How fast the weapon goes back to it's original position after the recoil is finished")]
    public float recoilRestitutionSharpness = 10.0f;

    [Header("Misc")]
    [Tooltip("Speed at which the aiming animatoin is played")]
    public float aimingAnimationSpeed = 14.0f;

    [Tooltip("Field of view when not aiming")]
    public float defaultFOV = 60.0f;

    [Tooltip("Portion of the regular FOV to apply to the weapon camera")]
    public float weaponFOVMultiplier = 1.0f;

    [Tooltip("Delay before switching weapon a second time, to avoid recieving multiple inputs from mouse wheel")]
    public float weaponSwitchDelay = 0.2f;

    [Tooltip("Layer to set FPS weapon gameObjects to")]
    public LayerMask FPSWeaponLayer;

    public PlayerWeaponsManager playerWeaponsManager;
    public GenericControllerInputs inputHandler;
    public GenericController playerCharacterController;

    private float weaponBobFactor;
    private Vector3 lastCharacterPosition;
    private Vector3 weaponMainLocalPosition;
    private Vector3 weaponBobLocalPosition;
    private Vector3 weaponRecoilLocalPosition;
    private Vector3 accumulatedRecoil;

    // NOTE [bgish]: I feel like this needs to go away, but not sure best way to do that yet
    public void UpdatePuttingDown(float progress)
    {
        this.weaponMainLocalPosition = Vector3.Lerp(this.defaultWeaponPosition.localPosition, this.downWeaponPosition.localPosition, progress);
    }

    // NOTE [bgish]: I feel like this needs to go away, but not sure best way to do that yet
    public void UpdatePuttingUp(float progress)
    {
        this.weaponMainLocalPosition = Vector3.Lerp(this.downWeaponPosition.localPosition, this.defaultWeaponPosition.localPosition, progress);
    }

    private void Awake()
    {
        this.SetFOV(this.defaultFOV);
        this.playerWeaponsManager.onAddedWeapon += this.OnAddedWeapon;
    }

    private void Update()
    {
        // shoot handling
        WeaponController activeWeapon = this.playerWeaponsManager.GetActiveWeapon();

        if (activeWeapon && this.playerWeaponsManager.State == PlayerWeaponsManager.WeaponSwitchState.Up)
        {
            // handle shooting
            activeWeapon.SetUse(this.inputHandler.shoot, out float recoilForce);

            // Handle accumulating recoil
            if (recoilForce > 0.0f)
            {
                this.accumulatedRecoil += Vector3.back * recoilForce;
                this.accumulatedRecoil = Vector3.ClampMagnitude(this.accumulatedRecoil, this.maxRecoilDistance);
            }
        }
    }

    // Update various animated features in LateUpdate because it needs to override the animated arm position
    private void LateUpdate()
    {
        this.UpdateWeaponAiming();
        this.UpdateWeaponBob();
        this.UpdateWeaponRecoil();

        // Set final weapon socket position based on all the combined animation influences
        this.weaponParentSocket.localPosition = this.weaponMainLocalPosition + this.weaponBobLocalPosition + this.weaponRecoilLocalPosition;
    }

    // Updates weapon position and camera FoV for the aiming transition
    private void UpdateWeaponAiming()
    {
        if (this.playerWeaponsManager.State == PlayerWeaponsManager.WeaponSwitchState.Up)
        {
            WeaponController activeWeapon = this.playerWeaponsManager.GetActiveWeapon();
            if (this.playerWeaponsManager.IsAiming && activeWeapon)
            {
                this.weaponMainLocalPosition = Vector3.Lerp(this.weaponMainLocalPosition, this.aimingWeaponPosition.localPosition + activeWeapon.AimOffset, this.aimingAnimationSpeed * Time.deltaTime);
                this.SetFOV(Mathf.Lerp(this.playerCharacterController.Camera.fieldOfView, activeWeapon.AimZoomRatio * this.defaultFOV, this.aimingAnimationSpeed * Time.deltaTime));
            }
            else
            {
                this.weaponMainLocalPosition = Vector3.Lerp(this.weaponMainLocalPosition, this.defaultWeaponPosition.localPosition, this.aimingAnimationSpeed * Time.deltaTime);
                this.SetFOV(Mathf.Lerp(this.playerCharacterController.Camera.fieldOfView, this.defaultFOV, this.aimingAnimationSpeed * Time.deltaTime));
            }
        }
    }

    // Updates the weapon bob animation based on character speed
    private void UpdateWeaponBob()
    {
        if (Time.deltaTime > 0f)
        {
            Vector3 playerCharacterVelocity = (this.playerCharacterController.transform.position - this.lastCharacterPosition) / Time.deltaTime;

            // calculate a smoothed weapon bob amount based on how close to our max grounded movement velocity we are
            float characterMovementFactor = 0f;
            if (this.playerCharacterController.IsGrounded)
            {
                characterMovementFactor = Mathf.Clamp01(playerCharacterVelocity.magnitude / this.playerCharacterController.MaxSpeed);
            }

            this.weaponBobFactor = Mathf.Lerp(this.weaponBobFactor, characterMovementFactor, this.bobSharpness * Time.deltaTime);

            // Calculate vertical and horizontal weapon bob values based on a sine function
            float bobAmount = this.playerWeaponsManager.IsAiming ? this.aimingBobAmount : this.defaultBobAmount;
            float frequency = this.bobFrequency;
            float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * this.weaponBobFactor;
            float vBobValue = ((Mathf.Sin(Time.time * frequency * 2f) * 0.5f) + 0.5f) * bobAmount * this.weaponBobFactor;

            // Apply weapon bob
            this.weaponBobLocalPosition.x = hBobValue;
            this.weaponBobLocalPosition.y = Mathf.Abs(vBobValue);

            this.lastCharacterPosition = this.playerCharacterController.transform.position;
        }
    }

    // Updates the weapon recoil animation
    private void UpdateWeaponRecoil()
    {
        // if the accumulated recoil is further away from the current position, make the current position move towards the recoil target
        if (this.weaponRecoilLocalPosition.z >= this.accumulatedRecoil.z * 0.99f)
        {
            this.weaponRecoilLocalPosition = Vector3.Lerp(this.weaponRecoilLocalPosition, this.accumulatedRecoil, this.recoilSharpness * Time.deltaTime);
        }
        // otherwise, move recoil position to make it recover towards its resting pose
        else
        {
            this.weaponRecoilLocalPosition = Vector3.Lerp(this.weaponRecoilLocalPosition, Vector3.zero, this.recoilRestitutionSharpness * Time.deltaTime);
            this.accumulatedRecoil = this.weaponRecoilLocalPosition;
        }
    }

    private void OnAddedWeapon(WeaponController weaponInstance, int slotIndex)
    {
        weaponInstance.transform.SetParent(weaponParentSocket);
        weaponInstance.transform.localPosition = Vector3.zero;
        weaponInstance.transform.localRotation = Quaternion.identity;

        //// NOTE [bgish]: Disabling this for now, till I figure out the best way to handle the two cameras issue
        //// // Assign the first person layer to the weapon
        //// int layerIndex = Mathf.RoundToInt(Mathf.Log(FPSWeaponLayer.value, 2)); // This function converts a layermask to a layer index
        //// foreach (Transform t in weaponInstance.gameObject.GetComponentsInChildren<Transform>(true))
        //// {
        ////     t.gameObject.layer = layerIndex;
        //// }
    }

    // Sets the FOV of the main camera and the weapon camera simultaneously
    private void SetFOV(float fov)
    {
        this.playerCharacterController.Camera.fieldOfView = fov;
        this.weaponCamera.fieldOfView = fov * this.weaponFOVMultiplier;
    }
}

#endif
