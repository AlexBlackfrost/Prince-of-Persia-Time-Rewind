using Cinemachine;
using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerRecord {
    public TransformRecord playerTransform;
    public CameraRecord cameraRecord;
    public AnimationRecord animationRecord;
    public StateMachineRecord stateMachineRecord;
    public CharacterMovementRecord characterMovementRecord;
    public SwordRecord swordRecord;
    public HealthRecord healthRecord;
    public HurtboxRecord hurtboxRecord;
    public float deltaTime;

    public PlayerRecord(TransformRecord playerTransform, CameraRecord cameraRecord, AnimationRecord animationRecord,
                        StateMachineRecord stateMachineRecord, CharacterMovementRecord characterMovementRecord,
                        SwordRecord swordRecord, HealthRecord healthRecord, HurtboxRecord hurtboxRecord,
                        float deltaTime) {

        this.playerTransform = playerTransform;
        this.cameraRecord = cameraRecord;
        this.animationRecord = animationRecord;
        this.stateMachineRecord = stateMachineRecord;
        this.characterMovementRecord = characterMovementRecord;
        this.swordRecord = swordRecord;
        this.healthRecord = healthRecord;
        this.hurtboxRecord = hurtboxRecord;
        this.deltaTime = deltaTime;
    }
}

public struct EnemyRecord {
    public TransformRecord enemyTransform;
    public AnimationRecord animationRecord;

    public StateMachineRecord stateMachineRecord;
    public CharacterMovementRecord characterMovementRecord;
    public SwordRecord swordRecord;
    public HealthRecord healthRecord;
    public HurtboxRecord hurtboxRecord;
    public EnemyAIRecord enemyAIRecord;
    public float deltaTime;

    public EnemyRecord(TransformRecord enemyTransform, AnimationRecord animationRecord,
                        StateMachineRecord stateMachineRecord, CharacterMovementRecord characterMovementRecord,
                        SwordRecord swordRecord, HealthRecord healthRecord, HurtboxRecord hurtboxRecord,
                        EnemyAIRecord enemyAIRecord, float deltaTime) {

        this.enemyTransform = enemyTransform;
        this.animationRecord = animationRecord;
        this.stateMachineRecord = stateMachineRecord;
        this.characterMovementRecord = characterMovementRecord;
        this.swordRecord = swordRecord;
        this.healthRecord = healthRecord;
        this.hurtboxRecord = hurtboxRecord;
        this.enemyAIRecord = enemyAIRecord;
        this.deltaTime = deltaTime;
    }
}

public struct TransformRecord {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;
    public float deltaTime;
    public TransformRecord(Vector3 position, Quaternion rotation, Vector3 localScale) {
        this.position = position;
        this.rotation = rotation;
        this.localScale = localScale;
        deltaTime = 0;
    }

    public override string ToString() {
        return "Position: " + position + "\nRotation: " + rotation + "\nScale: " + localScale + "\n";
    }
}

public struct CameraRecord {
    public TransformRecord cameraTransform;
    public float deltaTime;
    public CameraRecord(TransformRecord cameraTransform) {
        this.cameraTransform = cameraTransform;
        deltaTime = 0;
    }
}

public struct AnimationRecord {
    public bool applyRootMotion;
    public AnimationParameter[] parameters;
    public AnimationLayerRecord[] animationLayerRecords;
    public float deltaTime;
    public AnimationRecord(AnimationParameter[] parameters, AnimationLayerRecord[] animationLayerRecords, bool applyRootMotion) {
        this.parameters = parameters;
        this.animationLayerRecords = animationLayerRecords;
        this.applyRootMotion = applyRootMotion;
        deltaTime = 0;
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
    public float deltaTime;
    public CharacterMovementRecord(Vector3 velocity) {
        this.velocity = velocity;
        deltaTime = 0;
    }
}

public struct SwordRecord {
    public bool sheathingEnabled;
    public bool unsheathingEnabled;
    public float animatorSwordLayerTargetWeight;
    public SwordState swordState;
    public Transform swordSocket;
    public bool hitboxEnabled;
    public float attackCooldownRemainingTime;
    public float deltaTime;

    public SwordRecord(bool sheathingEnabled, bool unsheathingEnabled, float animatorSwordLayerTargetWeight, 
                       SwordState swordState, Transform swordSocket, bool hitboxEnabled, float attackCooldownRemainingTime) {

        this.sheathingEnabled = sheathingEnabled;
        this.unsheathingEnabled = unsheathingEnabled;
        this.animatorSwordLayerTargetWeight = animatorSwordLayerTargetWeight;
        this.swordState = swordState;
        this.swordSocket = swordSocket;
        this.hitboxEnabled = hitboxEnabled;
        this.attackCooldownRemainingTime = attackCooldownRemainingTime;
        deltaTime = 0;
    }
}

public struct HealthRecord {
    public float currentHealth;
    public float deltaTime;

    public HealthRecord(float currentHealth) {
        this.currentHealth = currentHealth;
        deltaTime = 0;
    }
}

public struct HurtboxRecord {
    public float isDamageableRemainingTime;
    public bool isShielded;
    public bool isInvincible;
    public float deltaTime;

    public HurtboxRecord(float isDamageableRemainingTime, bool isShielded, bool isInvincible) {
        this.isDamageableRemainingTime = isDamageableRemainingTime;
        this.isShielded = isShielded;
        this.isInvincible = isInvincible;
        deltaTime = 0;
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

