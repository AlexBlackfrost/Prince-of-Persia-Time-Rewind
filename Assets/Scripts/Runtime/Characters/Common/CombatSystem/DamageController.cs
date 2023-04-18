using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageController{
    public Action<float> DamageReceived;
    
    public DamageController(GameObject character) {
        Hurtbox[] hurtboxes = character.GetComponentsInChildren<Hurtbox>();
        foreach(Hurtbox hurtbox in hurtboxes) {
            hurtbox.TriggerEntered += OnHurtboxTriggerEntered;
        }
    }

    private void OnHurtboxTriggerEntered(Hurtbox hurtbox, Collider other) {
        // TODO conditions check if already triggered this frame, parry and blocks
        // TODO calculate damage, use scriptable objects for weapons stats I guess
        float someDamage = 1;
        DamageReceived.Invoke(someDamage);
    }
}