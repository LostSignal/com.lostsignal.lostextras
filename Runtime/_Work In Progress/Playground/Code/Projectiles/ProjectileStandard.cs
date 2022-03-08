
#if UNITY

using Lost;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ProjectileBase))]
public class ProjectileStandard : MonoBehaviour
{
#pragma warning disable 0649
    [Header("General")]
    [Tooltip("Radius of this projectile's collision detection")]
    [SerializeField] private float radius = 0.01f;

    [Tooltip("Transform representing the root of the projectile (used for accurate collision detection)")]
    [SerializeField] private Transform root;

    [Tooltip("Transform representing the tip of the projectile (used for accurate collision detection)")]
    [SerializeField] private Transform tip;

    [Tooltip("LifeTime of the projectile")]
    [SerializeField] private float maxLifeTime = 5f;

    [Tooltip("VFX prefab to spawn upon impact")]
    [SerializeField] private GameObject impactVFX;

    [Tooltip("LifeTime of the VFX before being destroyed")]
    [SerializeField] private float impactVFXLifetime = 5f;

    [Tooltip("Offset along the hit normal where the VFX will be spawned")]
    [SerializeField] private float impactVFXSpawnOffset = 0.1f;

    [Tooltip("Clip to play on impact")]
    [SerializeField] private AudioClip impactSFXClip;

    [Tooltip("Layers this projectile can collide with")]
    [SerializeField] private LayerMask hittableLayers = -1;

    [Header("Movement")]
    [Tooltip("Speed of the projectile")]
    [SerializeField] private float speed = 20f;

    [Tooltip("Downward acceleration from gravity")]
    [SerializeField] private float gravityDownAcceleration = 0f;

    [Tooltip("Distance over which the projectile will correct its course to fit the intended trajectory (used to drift projectiles towards center of screen in First Person view). At values under 0, there is no correction")]
    [SerializeField] private float trajectoryCorrectionDistance = -1;

    [Tooltip("Determines if the projectile inherits the velocity that the weapon's muzzle had when firing")]
    [SerializeField] private bool inheritWeaponVelocity = false;

    [Header("Damage")]
    [Tooltip("Damage of the projectile")]
    [SerializeField] private float damage = 40f;

    [Tooltip("Area of damage. Keep empty if you don<t want area damage")]
    [SerializeField] private DamageArea areaOfDamage;

    [Header("Debug")]
    [Tooltip("Color of the projectile radius debug view")]
    [SerializeField] private Color radiusColor = Color.cyan * 0.2f;
#pragma warning restore 0649

    private List<Collider> m_IgnoredColliders = new List<Collider>();
    private Vector3 m_ConsumedTrajectoryCorrectionVector;
    private Vector3 m_TrajectoryCorrectionVector;
    private Vector3 m_LastRootPosition;
    private Vector3 m_Velocity;

    private ProjectileBase m_ProjectileBase;
    private bool m_HasTrajectoryOverride;
    private float m_ShootTime;

    const QueryTriggerInteraction k_TriggerInteraction = QueryTriggerInteraction.Collide;

    public float Damage
    {
        get => this.damage;
        set => this.damage = value;
    }

    public float Radius
    {
        get => this.radius;
        set => this.radius = value;
    }

    public float Speed
    {
        get => this.speed;
        set => this.speed = value;
    }

    public float GravityDownAcceleration
    {
        get => this.gravityDownAcceleration;
        set => this.gravityDownAcceleration = value;
    }

    private void OnEnable()
    {
        m_ProjectileBase = GetComponent<ProjectileBase>();
        DebugUtility.HandleErrorIfNullGetComponent<ProjectileBase, ProjectileStandard>(m_ProjectileBase, this, gameObject);

        m_ProjectileBase.onShoot += OnShoot;

        Destroy(gameObject, maxLifeTime);
    }

    private void OnShoot()
    {
        m_ShootTime = Time.time;
        m_LastRootPosition = root.position;
        m_Velocity = transform.forward * speed;
        transform.position += m_ProjectileBase.inheritedMuzzleVelocity * Time.deltaTime;

        // Ignore colliders of owner
        m_IgnoredColliders.Clear();
        m_ProjectileBase.owner.GetComponentsInChildren<Collider>(m_IgnoredColliders);

        // Handle case of player shooting (make projectiles not go through walls, and remember center-of-screen trajectory)
        FPSWeaponsManager playerWeaponsManager = m_ProjectileBase.owner.GetComponent<FPSWeaponsManager>();
        if (playerWeaponsManager)
        {
            m_HasTrajectoryOverride = true;

            Vector3 cameraToMuzzle = (m_ProjectileBase.initialPosition - playerWeaponsManager.weaponCamera.transform.position);

            m_TrajectoryCorrectionVector = Vector3.ProjectOnPlane(-cameraToMuzzle, playerWeaponsManager.weaponCamera.transform.forward);
            if (trajectoryCorrectionDistance == 0)
            {
                transform.position += m_TrajectoryCorrectionVector;
                m_ConsumedTrajectoryCorrectionVector = m_TrajectoryCorrectionVector;
            }
            else if (trajectoryCorrectionDistance < 0)
            {
                m_HasTrajectoryOverride = false;
            }

            if (Physics.Raycast(playerWeaponsManager.weaponCamera.transform.position, cameraToMuzzle.normalized, out RaycastHit hit, cameraToMuzzle.magnitude, hittableLayers, k_TriggerInteraction))
            {
                if (this.IsHitValid(hit))
                {
                    this.OnHit(hit.point, hit.normal, hit.collider);
                }
            }
        }
    }

    void Update()
    {
        // Move
        transform.position += m_Velocity * Time.deltaTime;
        if (inheritWeaponVelocity)
        {
            transform.position += m_ProjectileBase.inheritedMuzzleVelocity * Time.deltaTime;
        }

        // Drift towards trajectory override (this is so that projectiles can be centered 
        // with the camera center even though the actual weapon is offset)
        if (m_HasTrajectoryOverride && m_ConsumedTrajectoryCorrectionVector.sqrMagnitude < m_TrajectoryCorrectionVector.sqrMagnitude)
        {
            Vector3 correctionLeft = m_TrajectoryCorrectionVector - m_ConsumedTrajectoryCorrectionVector;
            float distanceThisFrame = (root.position - m_LastRootPosition).magnitude;
            Vector3 correctionThisFrame = (distanceThisFrame / trajectoryCorrectionDistance) * m_TrajectoryCorrectionVector;
            correctionThisFrame = Vector3.ClampMagnitude(correctionThisFrame, correctionLeft.magnitude);
            m_ConsumedTrajectoryCorrectionVector += correctionThisFrame;

            // Detect end of correction
            if (m_ConsumedTrajectoryCorrectionVector.sqrMagnitude == m_TrajectoryCorrectionVector.sqrMagnitude)
            {
                m_HasTrajectoryOverride = false;
            }

            transform.position += correctionThisFrame;
        }

        // Orient towards velocity
        transform.forward = m_Velocity.normalized;

        // Gravity
        if (gravityDownAcceleration > 0)
        {
            // add gravity to the projectile velocity for ballistic effect
            m_Velocity += Vector3.down * gravityDownAcceleration * Time.deltaTime;
        }

        // Hit detection
        {
            RaycastHit closestHit = new RaycastHit();
            closestHit.distance = Mathf.Infinity;
            bool foundHit = false;

            // Sphere cast
            Vector3 displacementSinceLastFrame = tip.position - m_LastRootPosition;

            int count = Physics.SphereCastNonAlloc(
                m_LastRootPosition,
                radius,
                displacementSinceLastFrame.normalized,
                Lost.Caching.RaycastHitsCache,
                displacementSinceLastFrame.magnitude,
                hittableLayers,
                k_TriggerInteraction);

            for (int i = 0; i < count; i++)
            {
                RaycastHit hit = Lost.Caching.RaycastHitsCache[i];

                if (this.IsHitValid(hit) && hit.distance < closestHit.distance)
                {
                    foundHit = true;
                    closestHit = hit;
                }
            }

            if (foundHit)
            {
                // Handle case of casting while already inside a collider
                if (closestHit.distance <= 0f)
                {
                    closestHit.point = root.position;
                    closestHit.normal = -transform.forward;
                }

                this.OnHit(closestHit.point, closestHit.normal, closestHit.collider);
            }
        }

        m_LastRootPosition = root.position;
    }

    private bool IsHitValid(RaycastHit hit)
    {
        // Ignore hits with an ignore component
        if (IgnoreHitDetection.TryGetIgnoreHitDection(hit.collider, out IgnoreHitDetection ignoreHitDetection))
        {
            return false;
        }

        // Ignore hits with triggers that don't have a Damageable component
        if (hit.collider.isTrigger && Damageable.TryGetDamageable(hit.collider, out Damageable damageable) == false)
        {
            return false;
        }

        // Ignore hits with specific ignored colliders (self colliders, by default)
        if (m_IgnoredColliders.Contains(hit.collider))
        {
            return false;
        }

        return true;
    }

    private void OnHit(Vector3 point, Vector3 normal, Collider collider)
    {
        // damage
        if (areaOfDamage)
        {
            // area damage
            areaOfDamage.InflictDamageInArea(damage, point, hittableLayers, k_TriggerInteraction, m_ProjectileBase.owner);
        }
        else
        {
            // point damage
            Damageable damageable = collider.GetComponent<Damageable>();
            if (damageable)
            {
                damageable.InflictDamage(damage, false, m_ProjectileBase.owner);
            }
        }

        // impact vfx
        if (impactVFX)
        {
            GameObject impactVFXInstance = Instantiate(impactVFX, point + (normal * impactVFXSpawnOffset), Quaternion.LookRotation(normal));
            if (impactVFXLifetime > 0)
            {
                Destroy(impactVFXInstance.gameObject, impactVFXLifetime);
            }
        }

        // impact sfx
        if (impactSFXClip)
        {
            AudioUtility.CreateSFX(impactSFXClip, point, AudioUtility.AudioGroups.Impact, 1f, 3f);
        }

        // Self Destruct
        Destroy(this.gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = radiusColor;
        Gizmos.DrawSphere(transform.position, radius);
    }
}

#endif
