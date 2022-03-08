
#if UNITY

using Lost;
using UnityEngine;
using UnityEngine.Events;

public class ProjectileBase : MonoBehaviour
{
    public GameObject owner { get; private set; }
    public Vector3 initialPosition { get; private set; }
    public Vector3 initialDirection { get; private set; }
    public Vector3 inheritedMuzzleVelocity { get; private set; }
    public float initialCharge { get; private set; }

    public UnityAction onShoot;

    public void Shoot(GameObject owner, Vector3 inheritedWorldVelocity, float currentCharge)
    {
        this.owner = owner;
        this.initialPosition = transform.position;
        this.initialDirection = transform.forward;
        this.inheritedMuzzleVelocity = inheritedWorldVelocity;
        this.initialCharge = currentCharge;
        onShoot?.Invoke();
    }
}

#endif
