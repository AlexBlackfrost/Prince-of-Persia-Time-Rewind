using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Health{
    [SerializeField] private float currentHealth;
    [field:SerializeField] public float MaxHealth { get; private set; }
    public float CurrentHealth {
        get {
            return currentHealth;
        }

        set {
            if(value != currentHealth) { 
                float previousHealth = currentHealth;
                currentHealth = value;
                HealthChanged01?.Invoke(previousHealth/MaxHealth, currentHealth / MaxHealth);
            }
        } 
    }

    public Action<float, float> HealthChanged01;
    public Action Dead;
    
    public void Init() {
        HealthChanged01?.Invoke(CurrentHealth/MaxHealth, 1);
        CurrentHealth = MaxHealth;
    }

    public void OnDamageReceived(float damageAmount, IDamageSource damageSource) {
        CurrentHealth = Math.Max(CurrentHealth - damageAmount, 0);
        if(CurrentHealth == 0) {
            Dead?.Invoke();
        }
    }
}