using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour, IDamageable, IHittable, IShieldable {

    [SerializeField] private float isDamageableCooldown = 0.0f;
    public Action<float, IDamageSource> DamageReceived;
    public Action HitReceived;
    [field:SerializeField] public bool IsInvincible{ get; set;}

    [field:SerializeField, ReadOnly] public float IsDamageableRemainingTime { get; set; }

    public Action<GameObject> Parry { get; set; }

    [SerializeField, ReadOnly]private bool isShielded = false;



    public void Update() {
        IsDamageableRemainingTime = Math.Max(IsDamageableRemainingTime - Time.deltaTime, 0);            
    }

    public void Hit() {
        HitReceived?.Invoke();
    }

    public void ReceiveDamage(float amount, IDamageSource damageSource) {
        DamageReceived?.Invoke(amount, damageSource);
        IsDamageableRemainingTime = isDamageableCooldown;
    }

    public bool IsShielded() {
        return isShielded;
    }

    public void SetIsShielded(bool isShielded) {
        this.isShielded = isShielded;
    }

    public bool CanBeDamaged() {
        return IsDamageableRemainingTime <= 0 && !IsInvincible;
    }

    public Transform GetTransform() {
        return transform;
    }

}