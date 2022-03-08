
#if UNITY

using UnityEngine;

public enum WeaponShootType
{
    Manual,
    Automatic,
    Charge,
}

[System.Serializable]
public struct CrosshairData
{
    [Tooltip("The image that will be used for this weapon's crosshair")]
    public Sprite crosshairSprite;

    [Tooltip("The size of the crosshair image")]
    public int crosshairSize;

    [Tooltip("The color of the crosshair image")]
    public Color crosshairColor;
}

[RequireComponent(typeof(AudioSource))]
public class WeaponController : MonoBehaviour
{
#pragma warning disable 0649
    [Header("Information")]
    [Tooltip("The name that will be displayed in the UI for this weapon")]
    [SerializeField] private string weaponName;

    [Tooltip("The image that will be displayed in the UI for this weapon")]
    [SerializeField] private Sprite weaponIcon;

    [Tooltip("Default data for the crosshair")]
    [SerializeField] private CrosshairData crosshairDataDefault;

    [Tooltip("Data for the crosshair when targeting an enemy")]
    [SerializeField] private CrosshairData crosshairDataTargetInSight;

    [Header("Internal References")]
    [Tooltip("The root object for the weapon, this is what will be deactivated when the weapon isn't active")]
    [SerializeField] private GameObject weaponRoot;

    [Tooltip("Tip of the weapon, where the projectiles are shot")]
    [SerializeField] private Transform weaponMuzzle;

    [Header("Shoot Parameters")]
    [Tooltip("The type of weapon wil affect how it shoots")]
    [SerializeField] private WeaponShootType shootType;

    [Tooltip("The projectile prefab")]
    [SerializeField] private ProjectileBase projectilePrefab;

    [Tooltip("Minimum duration between two shots")]
    [SerializeField] private float delayBetweenShots = 0.5f;

    [Tooltip("Angle for the cone in which the bullets will be shot randomly (0 means no spread at all)")]
    [SerializeField] private float bulletSpreadAngle = 0f;

    [Tooltip("Amount of bullets per shot")]
    [SerializeField] private int bulletsPerShot = 1;

    [Tooltip("Force that will push back the weapon after each shot")]
    [Range(0f, 2f)]
    [SerializeField] private float recoilForce = 1;

    [Tooltip("Ratio of the default FOV that this weapon applies while aiming")]
    [Range(0f, 1f)]
    [SerializeField] private float aimZoomRatio = 1f;

    [Tooltip("Translation to apply to weapon arm when aiming with this weapon")]
    [SerializeField] private Vector3 aimOffset;

    [Header("Ammo Parameters")]
    [Tooltip("Amount of ammo reloaded per second")]
    [SerializeField] private float ammoReloadRate = 1f;

    [Tooltip("Delay after the last shot before starting to reload")]
    [SerializeField] private float ammoReloadDelay = 2f;

    [Tooltip("Maximum amount of ammo in the gun")]
    [SerializeField] private float maxAmmo = 8;

    [Header("Charging parameters (charging weapons only)")]
    [Tooltip("Duration to reach maximum charge")]
    [SerializeField] private float maxChargeDuration = 2f;

    [Tooltip("Initial ammo used when starting to charge")]
    [SerializeField] private float ammoUsedOnStartCharge = 1f;

    [Tooltip("Additional ammo used when charge reaches its maximum")]
    [SerializeField] private float ammoUsageRateWhileCharging = 1f;

    [Header("Audio & Visual")]
    [Tooltip("Prefab of the muzzle flash")]
    [SerializeField] private GameObject muzzleFlashPrefab;

    [Tooltip("sound played when shooting")]
    [SerializeField] private AudioClip shootSFX;

    [Tooltip("Sound played when changing to this weapon")]
    [SerializeField] private AudioClip changeWeaponSFX;
#pragma warning restore 0649

    public string WeaponName => this.weaponName;
    public Vector3 AimOffset => this.aimOffset;
    public float AimZoomRatio => this.aimZoomRatio;
    public Transform WeaponMuzzle => this.weaponMuzzle;
    public Sprite WeaponIcon => this.weaponIcon;
    public CrosshairData CrosshairDataDefault => this.crosshairDataDefault;
    public CrosshairData CrosshairDataTargetInSight => this.crosshairDataTargetInSight;

    private Vector3 muzzleWorldVelocity;

    private float m_CurrentAmmo;
    private float m_LastTimeShot = Mathf.NegativeInfinity;
    private float m_TimeBeginCharge;
    private Vector3 m_LastMuzzlePosition;
    private AudioSource m_ShootAudioSource;
    private int useDownFrames = 0;

    public GameObject owner { get; set; }
    public GameObject sourcePrefab { get; set; }
    public bool isCharging { get; private set; }
    public float currentAmmoRatio { get; private set; }
    public bool isWeaponActive { get; private set; }
    public bool isCooling { get; private set; }
    public float currentCharge { get; private set;}

    public float GetAmmoNeededToShoot() => (shootType != WeaponShootType.Charge ? 1 : ammoUsedOnStartCharge) / maxAmmo;

    void Awake()
    {
        m_CurrentAmmo = maxAmmo;
        m_LastMuzzlePosition = weaponMuzzle.position;

        m_ShootAudioSource = GetComponent<AudioSource>();
        DebugUtility.HandleErrorIfNullGetComponent<AudioSource, WeaponController>(m_ShootAudioSource, this, gameObject);
    }

    void Update()
    {
        UpdateAmmo();

        UpdateCharge();

        if (Time.deltaTime > 0)
        {
            muzzleWorldVelocity = (weaponMuzzle.position - m_LastMuzzlePosition) / Time.deltaTime;
            m_LastMuzzlePosition = weaponMuzzle.position;
        }
    }

    void UpdateAmmo()
    {
        if (m_LastTimeShot + ammoReloadDelay < Time.time && m_CurrentAmmo < maxAmmo && !isCharging)
        {
            // reloads weapon over time
            m_CurrentAmmo += ammoReloadRate * Time.deltaTime;

            // limits ammo to max value
            m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo, 0, maxAmmo);

            isCooling = true;
        }
        else
        {
            isCooling = false;
        }

        if (maxAmmo == Mathf.Infinity)
        {
            currentAmmoRatio = 1f;
        }
        else
        {
            currentAmmoRatio = m_CurrentAmmo / maxAmmo;
        }
    }

    void UpdateCharge()
    {
        if (isCharging)
        {
            if (currentCharge < 1f)
            {
                float chargeLeft = 1f - currentCharge;

                // Calculate how much charge ratio to add this frame
                float chargeAdded = 0f;

                if (maxChargeDuration <= 0f)
                {
                    chargeAdded = chargeLeft;
                }

                chargeAdded = (1f / maxChargeDuration) * Time.deltaTime;
                chargeAdded = Mathf.Clamp(chargeAdded, 0f, chargeLeft);

                // See if we can actually add this charge
                float ammoThisChargeWouldRequire = chargeAdded * ammoUsageRateWhileCharging;

                //if (ammoThisChargeWouldRequire <= m_CurrentAmmo)
                {
                    // Use ammo based on charge added
                    UseAmmo(ammoThisChargeWouldRequire);

                    // set current charge ratio
                    currentCharge = Mathf.Clamp01(currentCharge + chargeAdded);
                }
            }
        }
    }

    public void ShowWeapon(bool show)
    {
        weaponRoot.SetActive(show);

        if (show && changeWeaponSFX)
        {
            m_ShootAudioSource.PlayOneShot(changeWeaponSFX);
        }

        isWeaponActive = show;
    }

    public void UseAmmo(float amount)
    {
        m_CurrentAmmo = Mathf.Clamp(m_CurrentAmmo - amount, 0f, maxAmmo);
        m_LastTimeShot = Time.time;
    }

    public void SetUse(bool use, out float recoilForce)
    {
        this.useDownFrames = use ? useDownFrames + 1 : 0;
        recoilForce = this.HandleShootInputs(use, useDownFrames > 1, use == false) ? this.recoilForce : 0.0f;
    }

    public bool HandleShootInputs(bool inputDown, bool inputHeld, bool inputUp)
    {
        switch (shootType)
        {
            case WeaponShootType.Manual:
                if (inputDown)
                {
                    return TryShoot();
                }
                return false;

            case WeaponShootType.Automatic:
                if (inputHeld)
                {
                    return TryShoot();
                }
                return false;

            case WeaponShootType.Charge:
                if (inputHeld)
                {
                    TryBeginCharge();
                }
                if (inputUp)
                {
                    return TryReleaseCharge();
                }
                return false;

            default:
                return false;
        }
    }

    bool TryShoot()
    {
        if (m_CurrentAmmo >= 1.0f && m_LastTimeShot + delayBetweenShots < Time.time)
        {
            HandleShoot();
            m_CurrentAmmo -= 1;

            return true;
        }

        return false;
    }

    bool TryBeginCharge()
    {
        if (!isCharging && m_CurrentAmmo >= ammoUsedOnStartCharge && m_LastTimeShot + delayBetweenShots < Time.time)
        {
            UseAmmo(ammoUsedOnStartCharge); 
            isCharging = true;

            return true;
        }

        return false;
    }

    bool TryReleaseCharge()
    {
        if (isCharging)
        {
            HandleShoot();

            currentCharge = 0f;
            isCharging = false;

            return true;
        }
        return false;
    }

    void HandleShoot()
    {
        // spawn all bullets with random direction
        for (int i = 0; i < bulletsPerShot; i++)
        {
            Vector3 shotDirection = GetShotDirectionWithinSpread(weaponMuzzle);
            ProjectileBase newProjectile = Instantiate(projectilePrefab, weaponMuzzle.position, Quaternion.LookRotation(shotDirection));
            newProjectile.Shoot(this.owner, this.muzzleWorldVelocity, this.currentCharge);
        }

        // muzzle flash
        if (muzzleFlashPrefab != null)
        {
            GameObject muzzleFlashInstance = Instantiate(muzzleFlashPrefab, weaponMuzzle.position, weaponMuzzle.rotation, weaponMuzzle.transform);
            Destroy(muzzleFlashInstance, 2f);
        }

        m_LastTimeShot = Time.time;

        // play shoot SFX
        if (shootSFX)
        {
            m_ShootAudioSource.PlayOneShot(shootSFX);
        }
    }

    public Vector3 GetShotDirectionWithinSpread(Transform shootTransform)
    {
        float spreadAngleRatio = bulletSpreadAngle / 180f;
        Vector3 spreadWorldDirection = Vector3.Slerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere, spreadAngleRatio);

        return spreadWorldDirection;
    }
}

#endif
