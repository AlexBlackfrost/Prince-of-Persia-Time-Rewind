using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour, IDamageable, IHittable, IShieldable {

    [SerializeField] private float isDamageableCooldown = 0.0f;
    public Action<float> DamageReceived;
    public Action HitReceived;

    [SerializeField, ReadOnly]private bool isShieldedDebugOnly = false;

    private RewindableVariable<bool> isInvincible;
    private RewindableVariable<bool> isShielded;
    private RewindableVariable<float> isDamageableRemainingTime;

    public bool IsInvincible {
        get {
            return isInvincible.Value;
        }
        set {
            isInvincible.Value = value;
        }
    }

    public bool IsShielded {
        get {
            return isShielded.Value;
        }
        set {
            isShielded.Value = value;
            isShieldedDebugOnly = value;
        }
    }

    public float IsDamageableRemainingTime {
        get {
            return isDamageableRemainingTime.Value;
        }
        set {
            isDamageableRemainingTime.Value = value;
        }
    }

    private void Awake() {
        isDamageableRemainingTime = new RewindableVariable<float>(0.0f, onlyExecuteOnRewindStop: true);
        isShielded = new RewindableVariable<bool>(value:false, onlyExecuteOnRewindStop:true);
        isInvincible = new RewindableVariable<bool>(value:false, onlyExecuteOnRewindStop: true);
#if UNITY_EDITOR
        isDamageableRemainingTime.Name = "IsDamageableRemainingTime" + gameObject.name;
        isInvincible.Name = "IsInvincible" + gameObject.name;
        isShielded.Name = "IsShielded" + gameObject.name;
#endif
    }

    private void Update() {
        isDamageableRemainingTime.Value = Math.Max(isDamageableRemainingTime.Value - Time.deltaTime, 0);            
    }

    public void Hit() {
        HitReceived?.Invoke();
    }

    public void ReceiveDamage(float amount) {
        DamageReceived?.Invoke(amount);
        isDamageableRemainingTime.Value = isDamageableCooldown;
    }

    public bool CanBeDamaged() {
        return isDamageableRemainingTime.Value <= 0 && !isInvincible.Value;
    }

    public Transform GetTransform() {
        return transform;
    }

    bool IShieldable.IsShielded() {
        return IsShielded;
    }
}