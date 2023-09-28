using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HazardDamageSource : IDamageSource{

    [SerializeField] private float knockback = 10;

    public GameObject DamageApplier { get; set; }

    public void ApplyDamageEffect(GameObject damageReceiver) {
        PlayerController playerController = damageReceiver.GetComponent<PlayerController>();
        if (playerController != null) {
            Vector3 knockbackVelocity = -damageReceiver.transform.forward* knockback;
            playerController.CharacterMovement.Velocity += knockbackVelocity;
        }
    }
}