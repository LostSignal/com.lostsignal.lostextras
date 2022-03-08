
#if UNITY

using Lost;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileChargeParameters : MonoBehaviour, IValidate
{
#pragma warning disable 0649
    [SerializeField] private MinMaxFloat damage;
    [SerializeField] private MinMaxFloat radius;
    [SerializeField] private MinMaxFloat speed;
    [SerializeField] private MinMaxFloat gravityDownAcceleration;
    [SerializeField] private MinMaxFloat areaOfEffectDistance;
    [SerializeField] private ProjectileBase projectileBase;
    [SerializeField] private ProjectileStandard projectileStandard;
#pragma warning restore 0649

    public void Validate(List<ValidationError> errors)
    {
        this.ValidateNotNull(errors, this.projectileBase, nameof(this.projectileBase));
        this.ValidateNotNull(errors, this.projectileStandard, nameof(this.projectileStandard));
    }

    private void Awake()
    {
        this.projectileBase.onShoot += this.OnShoot;
    }

    private void OnShoot()
    {
        this.projectileStandard.Damage = this.damage.GetValueFromRatio(this.projectileBase.initialCharge);
        this.projectileStandard.Radius = this.radius.GetValueFromRatio(this.projectileBase.initialCharge);
        this.projectileStandard.Speed = this.speed.GetValueFromRatio(this.projectileBase.initialCharge);
        this.projectileStandard.GravityDownAcceleration = this.gravityDownAcceleration.GetValueFromRatio(this.projectileBase.initialCharge);
    }
}

#endif
