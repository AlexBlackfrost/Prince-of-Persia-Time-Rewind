using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class Health : ISerializationCallbackReceiver{
    [field:SerializeField] public float MaxHealth { get; private set; }

    [SerializeField] private float serializedCurrentHealth;

    public Action<float, float> HealthChanged;
    public Action Dead;

    public float CurrentHealth {
        get {
            return currentHealth.Value;
        }

        set {
            float previousHealth = CurrentHealth;
            currentHealth.Value = value;
            HealthChanged?.Invoke(previousHealth, currentHealth.Value);
        } 
    }

    private RewindableVariable<float> currentHealth;
    public void OnBeforeSerialize() {
        if(currentHealth != null) {
            serializedCurrentHealth = currentHealth.Value;
        }
    }

    public void OnAfterDeserialize() {
        if (currentHealth != null) {
            currentHealth.Value = serializedCurrentHealth;
        }
    }
    
    public void Init() {
        currentHealth = new RewindableVariable<float>(value: serializedCurrentHealth, interpolationEnabled:false);
        CurrentHealth = MaxHealth;
    }

    public void OnDamageReceived(float damageAmount) {
        CurrentHealth = Math.Max(CurrentHealth - damageAmount, 0);
        if(CurrentHealth == 0) {
            Dead?.Invoke();
        }
    }

    
}