using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Health{
    [field:SerializeField] public float MaxHealth { get; private set; }

    [SerializeField] private float serializedCurrentHealth;
    private RewindableVariable<float> currentHealth;
    public float CurrentHealth {
        get {
            if (currentHealth == null) {
                currentHealth = new RewindableVariable<float>(value: serializedCurrentHealth);
            }
            return currentHealth.Value;
        }

        set {
            if (currentHealth == null) {
                currentHealth = new RewindableVariable<float>(value: serializedCurrentHealth);
            }
            float previousHealth = currentHealth.Value;
            currentHealth.Value = value;
            serializedCurrentHealth = value;
            HealthChanged?.Invoke(previousHealth, currentHealth.Value);
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