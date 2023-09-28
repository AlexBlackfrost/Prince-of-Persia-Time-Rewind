using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]public class SwordDamageSource : IDamageSource{
    public GameObject DamageApplier { get ; set; }

    public void ApplyDamageEffect(GameObject damageReceiver) {
        /* Note: if this system needed to be extended, it would be a good idea to turn CharacterMovement
        * into a completely separated and independent MonoBehavior component. This way, other classes
        * wouldn't need to access the player or enemy controller classes first. It would also fix the problem
        * of character controller not resolving collisions unless CharacterController.Move() was called in this
        * frame. However, I would use MonoBehavior's update function to update it, I would create a custom update
        * function that would be called from the controller classes to ensure that movement is performed after updating
        * state machine logic.
        */
        CharacterMovement characterMovement = null;
        PlayerController playerController = damageReceiver.GetComponent<PlayerController>();
        if (playerController != null) {
            characterMovement = playerController.CharacterMovement;
        } else {
            EnemyController enemyController = damageReceiver.GetComponent<EnemyController>();
            characterMovement = enemyController.CharacterMovement;
        }
        characterMovement.Velocity = Vector3.zero;
    }

}