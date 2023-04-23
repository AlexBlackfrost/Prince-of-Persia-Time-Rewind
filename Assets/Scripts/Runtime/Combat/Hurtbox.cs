using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour, IDamageable, IHittable, IShieldable {

    [SerializeField] private float isDamageableCooldown = 0.0f;
    public Action<float> DamageReceived;
    public Action HitReceived;

    private float isDamageableRemainingTime;
    private bool isShielded = false;


    public void Update() {
        isDamageableRemainingTime = Math.Max(isDamageableRemainingTime - Time.deltaTime, 0);            
    }


    public void Hit() {
        HitReceived?.Invoke();
    }

    public void ReceiveDamage(float amount) {
        DamageReceived?.Invoke(amount);
        isDamageableRemainingTime = isDamageableCooldown;
    }

    public bool IsShielded() {
        return isShielded;
    }

    public void SetIsShielded(bool isShielded) {
        this.isShielded = isShielded;
    }

    public bool CanBeDamaged() {
        return isDamageableRemainingTime <= 0;
    }
}