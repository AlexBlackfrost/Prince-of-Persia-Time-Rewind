using Cinemachine;
using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public struct CameraRecord {
    public TransformRecord cameraTransform;

    public CameraRecord(TransformRecord cameraTransform) {
        this.cameraTransform = cameraTransform;
    }
}

public struct AnimationRecord {
    public float deltaTime;
    public bool applyRootMotion;
   // public AnimationParameter[] parameters;
    public AnimationLayerRecord[] animationLayerRecords;
    public AnimationRecord(/*AnimationParameter[] parameters,*/ AnimationLayerRecord[] animationLayerRecords, bool applyRootMotion, float deltaTime) {
        //this.parameters = parameters;
        this.animationLayerRecords = animationLayerRecords;
        this.applyRootMotion = applyRootMotion;
        this.deltaTime = deltaTime;

    }
}
public struct AnimationLayerRecord {
    public int layer;
    public float layerWeight;
    public int shortNameHash;
    public float duration;
    public float normalizedTime;
    public bool isInTransition;
    public TransitionRecord transitionRecord;
    public TransitionRecord interruptedTransition;
    public bool IsInterruptingCurrentStateTransition;

    public AnimationLayerRecord(int layer, float layerWeight, int shortNameHash,  float normalizedTime, float duration ) {
        this.layer = layer;
        this.layerWeight = layerWeight;
        this.shortNameHash = shortNameHash;
        this.normalizedTime = normalizedTime;
        this.duration = duration;
        this.isInTransition = false;
        transitionRecord = default(TransitionRecord);
        interruptedTransition = default(TransitionRecord);
        IsInterruptingCurrentStateTransition = false;
    }
}

public struct AnimationParameter {
    public AnimatorControllerParameterType type;
    public int nameHash;
    public object value;

    public AnimationParameter(AnimatorControllerParameterType type, int nameHash, object value) {
        this.type = type;
        this.nameHash = nameHash;
        this.value = value;
    }
}

public struct TransitionRecord {
    public int nextStateNameHash;
    public float normalizedTime;
    public float transitionDuration;
    public float nextStateNormalizedTime;
    public float nextStateDuration;

    public TransitionRecord(int nextStateNameHash, float normalizedTime, float transitionDuration, 
                            float nextStateNormalizedTime, float nextStateDuration) {
        this.nextStateNameHash = nextStateNameHash;
        this.normalizedTime = normalizedTime;
        this.transitionDuration = transitionDuration;
        this.nextStateNormalizedTime = nextStateNormalizedTime;
        this.nextStateDuration = nextStateDuration;
    }
}




public struct AttackStateRecord {
    public int attackIndex;
    public bool comboEnabled;
    public bool rotationEnabled;
    public bool followedCombo;
    public IHittable[] alreadyHitObjects;
    public Transform closestAttackTarget;

    public AttackStateRecord(int attackIndex, bool comboEnabled, bool rotationEnabled, bool followedCombo, IHittable[] alreadyHitObjects, Transform attackTarget) {
        this.attackIndex = attackIndex;
        this.comboEnabled = comboEnabled;
        this.rotationEnabled = rotationEnabled;
        this.followedCombo = followedCombo;
        this.alreadyHitObjects = alreadyHitObjects;
        this.closestAttackTarget = attackTarget;
    }
}

public struct StrafeStateRecord {
    public float strafeSideAnimationVelocity;
    public float strafeForwardAnimationVelocity;

    public StrafeStateRecord(float strafeSideAnimationVelocity, float strafeForwardAnimationVelocity) {
        this.strafeSideAnimationVelocity = strafeSideAnimationVelocity;
        this.strafeForwardAnimationVelocity = strafeForwardAnimationVelocity;
    }
}

public struct AIAttackStateRecord {
    public IHittable[] alreadyHitObjects;

    public AIAttackStateRecord(IHittable[] alreadyHitObjects) {
        this.alreadyHitObjects = alreadyHitObjects;
    }
}

public struct RollStateRecord {
    public float rollElapsedTime;
    public RollStateRecord(float rollElapsedTime) {
        this.rollElapsedTime = rollElapsedTime;
    }
}

public struct CharacterMovementRecord {
    public Vector3 velocity;
    public CharacterMovementRecord(Vector3 velocity) {
        this.velocity = velocity;
    }
}

public struct SwordRecord {
    public bool sheathingEnabled;
    public bool unsheathingEnabled;
    public float unsheatheMotionTime;
    public float animatorSwordLayerWeight;
    public float animatorSwordLayerTargetWeight;
    public SwordState swordState;
    public Transform swordSocket;
    public bool hitboxEnabled;
    public float attackCooldownRemainingTime;

    public SwordRecord(bool sheathingEnabled, bool unsheathingEnabled, float unsheatheMotionTime, float animatorSwordLayerWeight,
                       float animatorSwordLayerTargetWeight, SwordState swordState, Transform swordSocket, bool hitboxEnabled, 
                       float attackCooldownRemainingTime) {

        this.sheathingEnabled = sheathingEnabled;
        this.unsheathingEnabled = unsheathingEnabled;
        this.unsheatheMotionTime = unsheatheMotionTime;
        this.animatorSwordLayerWeight = animatorSwordLayerWeight;
        this.animatorSwordLayerTargetWeight = animatorSwordLayerTargetWeight;
        this.swordState = swordState;
        this.swordSocket = swordSocket;
        this.hitboxEnabled = hitboxEnabled;
        this.attackCooldownRemainingTime = attackCooldownRemainingTime;
    }
}

public struct HealthRecord {
    public float currentHealth;

    public HealthRecord(float currentHealth) {
        this.currentHealth = currentHealth;
    }
}

public struct HurtboxRecord {
    public float isDamageableRemainingTime;
    public bool isShielded;
    public bool isInvincible;

    public HurtboxRecord(float isDamageableRemainingTime, bool isShielded, bool isInvincible) {
        this.isDamageableRemainingTime = isDamageableRemainingTime;
        this.isShielded = isShielded;
        this.isInvincible = isInvincible;
    }
}

public struct EnemyAIRecord {
    public bool hasBeenAttacked;
    public bool receivedTooMuchDamageRecently;

    public EnemyAIRecord(bool hasBeenAttacked, bool damagedTooOften) {
        this.hasBeenAttacked = hasBeenAttacked;
        this.receivedTooMuchDamageRecently = damagedTooOften;
    }
}

public static class RecordUtils { }

