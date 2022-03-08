
#if UNITY

using Lost;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerWeaponsManager : MonoBehaviour
{
    public enum WeaponSwitchState
    {
        Up,
        Down,
        PutDownPrevious,
        PutUpNew,
    }

    [Tooltip("List of weapon the player will start with")]
    public List<WeaponController> startingWeapons = new List<WeaponController>();

    [Header("References")]
    [Tooltip("Secondary camera used to avoid seeing weapon go throw geometries")]
    public Camera weaponCamera;

    [Header("Misc")]
    [Tooltip("Speed at which the aiming animatoin is played")]
    public float aimingAnimationSpeed = 14.0f;

    [Tooltip("Field of view when not aiming")]
    public float defaultFOV = 60.0f;

    [Tooltip("Portion of the regular FOV to apply to the weapon camera")]
    public float weaponFOVMultiplier = 1.0f;

    [Tooltip("Delay before switching weapon a second time, to avoid recieving multiple inputs from mouse wheel")]
    public float weaponSwitchDelay = 0.2f;

    public GenericControllerInputs m_InputHandler;
    public GenericController m_PlayerCharacterController;

    public UnityAction<WeaponController> onSwitchedToWeapon;
    public UnityAction<WeaponController, int> onAddedWeapon;
    public UnityAction<WeaponController, int> onRemovedWeapon;

    // NOTE [bgish]: This should not be a direct reference, but a interface to the current weapon controller type (1st vs 3rd, etc)
    public FPSWeaponsManager fpsWeaponsManager;

    private WeaponController[] weaponSlots = new WeaponController[9]; // 9 available weapon slots
    private float timeStartedWeaponSwitch;
    private WeaponSwitchState weaponSwitchState;
    private int weaponSwitchNewWeaponIndex;

    public WeaponSwitchState State => this.weaponSwitchState;

    public bool IsAiming { get; private set; }

    public bool IsPointingAtEnemy { get; private set; }

    public int ActiveWeaponIndex { get; private set; }

    // Iterate on all weapon slots to find the next valid weapon to switch to
    public void SwitchWeapon(bool ascendingOrder)
    {
        int newWeaponIndex = -1;
        int closestSlotDistance = this.weaponSlots.Length;
        for (int i = 0; i < this.weaponSlots.Length; i++)
        {
            // If the weapon at this slot is valid, calculate its "distance" from the active slot index (either in ascending or descending order)
            // and select it if it's the closest distance yet
            if (i != this.ActiveWeaponIndex && this.GetWeaponAtSlotIndex(i) != null)
            {
                int distanceToActiveIndex = this.GetDistanceBetweenWeaponSlots(this.ActiveWeaponIndex, i, ascendingOrder);

                if (distanceToActiveIndex < closestSlotDistance)
                {
                    closestSlotDistance = distanceToActiveIndex;
                    newWeaponIndex = i;
                }
            }
        }

        // Handle switching to the new weapon index
        SwitchToWeaponIndex(newWeaponIndex);
    }

    // Switches to the given weapon index in weapon slots if the new index is a valid weapon that is different from our current one
    public void SwitchToWeaponIndex(int newWeaponIndex, bool force = false)
    {
        if (force || (newWeaponIndex != this.ActiveWeaponIndex && newWeaponIndex >= 0))
        {
            // Store data related to weapon switching animation
            this.weaponSwitchNewWeaponIndex = newWeaponIndex;
            this.timeStartedWeaponSwitch = Time.time;

            // Handle case of switching to a valid weapon for the first time (simply put it up without putting anything down first)
            if (this.GetActiveWeapon() == null)
            {
                //// m_WeaponMainLocalPosition = downWeaponPosition.localPosition;

                this.weaponSwitchState = WeaponSwitchState.PutUpNew;
                this.ActiveWeaponIndex = this.weaponSwitchNewWeaponIndex;

                WeaponController newWeapon = GetWeaponAtSlotIndex(weaponSwitchNewWeaponIndex);
                if (this.onSwitchedToWeapon != null)
                {
                    this.onSwitchedToWeapon.Invoke(newWeapon);
                }
            }
            // otherwise, remember we are putting down our current weapon for switching to the next one
            else
            {
                this.weaponSwitchState = WeaponSwitchState.PutDownPrevious;
            }
        }
    }

    public bool HasWeapon(WeaponController weaponPrefab)
    {
        // Checks if we already have a weapon coming from the specified prefab
        foreach (var w in this.weaponSlots)
        {
            if (w != null && w.sourcePrefab == weaponPrefab.gameObject)
            {
                return true;
            }
        }

        return false;
    }

    // Adds a weapon to our inventory
    public bool AddWeapon(WeaponController weaponPrefab)
    {
        // if we already hold this weapon type (a weapon coming from the same source prefab), don't add the weapon
        if (this.HasWeapon(weaponPrefab))
        {
            return false;
        }

        // search our weapon slots for the first free one, assign the weapon to it, and return true if we found one. Return false otherwise
        for (int i = 0; i < this.weaponSlots.Length; i++)
        {
            // only add the weapon if the slot is free
            if (this.weaponSlots[i] == null)
            {
                // spawn the weapon prefab as child of the weapon socket
                WeaponController weaponInstance = Instantiate(weaponPrefab);

                // Set owner to this gameObject so the weapon can alter projectile/damage logic accordingly
                weaponInstance.owner = gameObject;
                weaponInstance.sourcePrefab = weaponPrefab.gameObject;
                weaponInstance.ShowWeapon(false);

                this.weaponSlots[i] = weaponInstance;

                if (this.onAddedWeapon != null)
                {
                    this.onAddedWeapon.Invoke(weaponInstance, i);
                }

                return true;
            }
        }

        // Handle auto-switching to weapon if no weapons currently
        if (this.GetActiveWeapon() == null)
        {
            this.SwitchWeapon(true);
        }

        return false;
    }

    public bool RemoveWeapon(WeaponController weaponInstance)
    {
        // Look through our slots for that weapon
        for (int i = 0; i < this.weaponSlots.Length; i++)
        {
            // when weapon found, remove it
            if (this.weaponSlots[i] == weaponInstance)
            {
                this.weaponSlots[i] = null;

                if (this.onRemovedWeapon != null)
                {
                    this.onRemovedWeapon.Invoke(weaponInstance, i);
                }

                GameObject.Destroy(weaponInstance.gameObject);

                // Handle case of removing active weapon (switch to next weapon)
                if (i == this.ActiveWeaponIndex)
                {
                    this.SwitchWeapon(true);
                }

                return true;
            }
        }

        return false;
    }

    public WeaponController GetActiveWeapon()
    {
        return this.GetWeaponAtSlotIndex(this.ActiveWeaponIndex);
    }

    public WeaponController GetWeaponAtSlotIndex(int index)
    {
        // find the active weapon in our weapon slots based on our active weapon index
        if (index >= 0 && index < this.weaponSlots.Length)
        {
            return this.weaponSlots[index];
        }

        // if we didn't find a valid active weapon in our weapon slots, return null
        return null;
    }

    private void Start()
    {
        this.ActiveWeaponIndex = -1;
        this.weaponSwitchState = WeaponSwitchState.Down;
        this.onSwitchedToWeapon += this.OnWeaponSwitched;

        // Add starting weapons
        foreach (var weapon in this.startingWeapons)
        {
            this.AddWeapon(weapon);
        }

        this.SwitchWeapon(true);
    }

    private void Update()
    {
        // Shoot handling
        WeaponController activeWeapon = GetActiveWeapon();
        bool hasActiveWeapon = activeWeapon;

        if (hasActiveWeapon && this.weaponSwitchState == WeaponSwitchState.Up)
        {
            // handle aiming down sights
            this.IsAiming = this.m_InputHandler.aim;
        }

        // Weapon switch handling
        if (!this.IsAiming &&
            (hasActiveWeapon == false || !activeWeapon.isCharging) &&
            (this.weaponSwitchState == WeaponSwitchState.Up || this.weaponSwitchState == WeaponSwitchState.Down))
        {
            int switchWeaponInput = 0; // m_InputHandler.GetSwitchWeaponInput();
            if (switchWeaponInput != 0)
            {
                bool switchUp = switchWeaponInput > 0;
                SwitchWeapon(switchUp);
            }
            else
            {
                switchWeaponInput = 0; // m_InputHandler.GetSelectWeaponInput();
                if (switchWeaponInput != 0)
                {
                    if (GetWeaponAtSlotIndex(switchWeaponInput - 1) != null)
                        SwitchToWeaponIndex(switchWeaponInput - 1);
                }
            }
        }

        // Pointing at enemy handling
        this.IsPointingAtEnemy = false;
        if (activeWeapon)
        {
            if (Physics.Raycast(this.weaponCamera.transform.position, this.weaponCamera.transform.forward, out RaycastHit hit, 1000, -1, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.GetComponentInParent<EnemyController>())
                {
                    this.IsPointingAtEnemy = true;
                }
            }
        }
    }

    // Update various animated features in LateUpdate because it needs to override the animated arm position
    private void LateUpdate()
    {
        this.UpdateWeaponSwitching();
    }

    // Updates the animated transition of switching weapons
    private void UpdateWeaponSwitching()
    {
        // Calculate the time ratio (0 to 1) since weapon switch was triggered
        float switchingTimeFactor;

        if (this.weaponSwitchDelay == 0f)
        {
            switchingTimeFactor = 1f;
        }
        else
        {
            switchingTimeFactor = Mathf.Clamp01((Time.time - this.timeStartedWeaponSwitch) / this.weaponSwitchDelay);
        }

        // Handle transiting to new switch state
        if (switchingTimeFactor >= 1f)
        {
            if (this.weaponSwitchState == WeaponSwitchState.PutDownPrevious)
            {
                // Deactivate old weapon
                WeaponController oldWeapon = this.GetWeaponAtSlotIndex(this.ActiveWeaponIndex);
                if (oldWeapon != null)
                {
                    oldWeapon.ShowWeapon(false);
                }

                this.ActiveWeaponIndex = this.weaponSwitchNewWeaponIndex;
                switchingTimeFactor = 0f;

                // Activate new weapon
                WeaponController newWeapon = this.GetWeaponAtSlotIndex(ActiveWeaponIndex);
                if (this.onSwitchedToWeapon != null)
                {
                    this.onSwitchedToWeapon.Invoke(newWeapon);
                }

                if (newWeapon)
                {
                    this.timeStartedWeaponSwitch = Time.time;
                    this.weaponSwitchState = WeaponSwitchState.PutUpNew;
                }
                else
                {
                    // if new weapon is null, don't follow through with putting weapon back up
                    this.weaponSwitchState = WeaponSwitchState.Down;
                }
            }
            else if (this.weaponSwitchState == WeaponSwitchState.PutUpNew)
            {
                this.weaponSwitchState = WeaponSwitchState.Up;
            }
        }

        // Handle moving the weapon socket position for the animated weapon switching
        if (this.weaponSwitchState == WeaponSwitchState.PutDownPrevious)
        {
            this.fpsWeaponsManager.UpdatePuttingDown(switchingTimeFactor);
        }
        else if (this.weaponSwitchState == WeaponSwitchState.PutUpNew)
        {
            this.fpsWeaponsManager.UpdatePuttingUp(switchingTimeFactor);
        }
    }

    // Calculates the "distance" between two weapon slot indexes
    // For example: if we had 5 weapon slots, the distance between slots #2 and #4 would be 2 in ascending order, and 3 in descending order
    private int GetDistanceBetweenWeaponSlots(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
    {
        int distanceBetweenSlots;

        if (ascendingOrder)
        {
            distanceBetweenSlots = toSlotIndex - fromSlotIndex;
        }
        else
        {
            distanceBetweenSlots = -1 * (toSlotIndex - fromSlotIndex);
        }

        if (distanceBetweenSlots < 0)
        {
            distanceBetweenSlots = this.weaponSlots.Length + distanceBetweenSlots;
        }

        return distanceBetweenSlots;
    }

    private void OnWeaponSwitched(WeaponController newWeapon)
    {
        if (newWeapon != null)
        {
            newWeapon.ShowWeapon(true);
        }
    }
}

#endif
