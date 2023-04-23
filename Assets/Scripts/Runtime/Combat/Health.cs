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
            float previousHealth = currentHealth;
            currentHealth = value;
            HealthChanged?.Invoke(previousHealth, currentHealth);
        } 
    }

    public Action<float, float> HealthChanged;
    public Action Dead;
    
    public void Init() {
        CurrentHealth = MaxHealth;
    }

    public void OnDamageReceived(float damageAmount) {
        CurrentHealth = Math.Max(CurrentHealth - damageAmount, 0);
        if(CurrentHealth == 0) {
            Dead?.Invoke();
        }
    }
}