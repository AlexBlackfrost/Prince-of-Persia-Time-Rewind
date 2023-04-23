using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Health{
    [field:SerializeField] public float MaxHealth { get; private set; }
    [field:SerializeField] public float CurrentHealth { get; set; }

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