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
    public float deltaTime;

    public PlayerRecord(TransformRecord playerTransform, CameraRecord cameraRecord, AnimationRecord animationRecord,
                        StateMachineRecord stateMachineRecord, CharacterMovementRecord characterMovementRecord,
                        SwordRecord swordRecord, float deltaTime) {

        this.playerTransform = playerTransform;
        this.cameraRecord = cameraRecord;
        this.animationRecord = animationRecord;
        this.stateMachineRecord = stateMachineRecord;
        this.characterMovementRecord = characterMovementRecord;
        this.swordRecord = swordRecord;
        this.deltaTime = deltaTime;
    }
}

public struct EnemyRecord {
    public TransformRecord enemyTransform;
    public AnimationRecord animationRecord;

    public StateMachineRecord stateMachineRecord;
    public CharacterMovementRecord characterMovementRecord;
    public SwordRecord swordRecord;
    public float deltaTime;

    public EnemyRecord(TransformRecord enemyTransform, AnimationRecord animationRecord,
                        StateMachineRecord stateMachineRecord, CharacterMovementRecord characterMovementRecord,
                        SwordRecord swordRecord, float deltaTime) {

        this.enemyTransform = enemyTransform;
        this.animationRecord = animationRecord;
        this.stateMachineRecord = stateMachineRecord;
        this.characterMovementRecord = characterMovementRecord;
        this.swordRecord = swordRecord;
        this.deltaTime = deltaTime;
    }
}

public struct TransformRecord {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;

    public TransformRecord(Vector3 position, Quaternion rotation, Vector3 localScale) {
        this.position = position;
        this.rotation = rotation;
        this.localScale = localScale;
    }

    public override string ToString() {
        return "Position: " + position + "\nRotation: " + rotation + "\nScale: " + localScale + "\n";
    }
}

public struct CameraRecord {
    public TransformRecord cameraTransform;

    public CameraRecord(TransformRecord cameraTransform) {
        this.cameraTransform = cameraTransform;
    }
}

public struct AnimationRecord {
    public bool applyRootMotion;
    public AnimationParameter[] parameters;
    public AnimationLayerRecord[] animationLayerRecords;
    public AnimationRecord(AnimationParameter[] parameters, AnimationLayerRecord[] animationLayerRecords, bool applyRootMotion) {
        this.parameters = parameters;
        this.animationLayerRecords = animationLayerRecords;
        this.applyRootMotion = applyRootMotion;

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

public struct StateMachineRecord {
    public Type[] hierarchy;
    public object[] stateObjectRecords;

    public StateMachineRecord(Type[] hierarchy, object[] stateObjectRecords) {
        this.hierarchy = hierarchy;
        this.stateObjectRecords = stateObjectRecords;
    }
}

public struct AttackStateRecord {
    public int attackIndex;
    public bool comboEnabled;
    public bool rotationEnabled;
    public bool followedCombo;

    public AttackStateRecord(int attackIndex, bool comboEnabled, bool rotationEnabled, bool followedCombo) {
        this.attackIndex = attackIndex;
        this.comboEnabled = comboEnabled;
        this.rotationEnabled = rotationEnabled;
        this.followedCombo = followedCombo;
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

    public SwordRecord(bool sheathingEnabled, bool unsheathingEnabled, float unsheatheMotionTime, float animatorSwordLayerWeight,
                       float animatorSwordLayerTargetWeight, SwordState swordState, Transform swordSocket) {
        this.sheathingEnabled = sheathingEnabled;
        this.unsheathingEnabled = unsheathingEnabled;
        this.unsheatheMotionTime = unsheatheMotionTime;
        this.animatorSwordLayerWeight = animatorSwordLayerWeight;
        this.animatorSwordLayerTargetWeight = animatorSwordLayerTargetWeight;
        this.swordState = swordState;
        this.swordSocket = swordSocket;
    }
}

public static class RecordUtils {
    public static TransformRecord RecordTransformData(Transform transform) {
        return new TransformRecord(transform.position,
                                   transform.rotation,
                                   transform.localScale);
    }

    public static CameraRecord RecordCameraData(Camera camera) {
        TransformRecord transformRecord = RecordTransformData(camera.transform);
        return new CameraRecord(transformRecord);
    }


    public static PlayerRecord RecordPlayerData(Transform transform, Camera camera, StateMachine stateMachine, 
                                                Animator animator, CharacterMovement characterMovement, SwordRecord swordRecord) {

        /*TransformRecord transformRecord = RecordTransformData(transform);
        CameraRecord cameraRecord = RecordCameraData(camera);
        AnimationRecord animationRecord = RecordAnimationData(animator);
        StateMachineRecord stateMachineRecord = RecordStateMachineData(stateMachine);
        CharacterMovementRecord characterMovementRecord = RecordCharacterMovementData(characterMovement);
        StateMachine stateMachineCopy = (StateMachine) stateMachine.Copy();
        //Debug.Log("Saving... " + stateMachineCopy.GetCurrentStateName());
        return new PlayerRecord(transformRecord, cameraRecord, animationRecord, stateMachineRecord,
                                characterMovementRecord, swordRecord, Time.deltaTime);*/
        return default(PlayerRecord);
    }

    private static TransformRecord[] RecordPose(Transform[] bones) {
        TransformRecord[] pose = new TransformRecord[bones.Length];
        int index = 0;
        foreach (Transform bone in bones) {
            pose[index++] = RecordTransformData(bone);
        }
        return pose;
    }

    

    private static StateMachineRecord RecordStateMachineData(StateMachine stateMachine) {
        Type[] hierarchy = GetTypeHierarchy(stateMachine.CurrentStateObject);
        object[] stateObjectsRecords = GetStateObjectsRecords(stateMachine.CurrentStateObject);
        StateMachineRecord stateMachineRecord = new StateMachineRecord(hierarchy, stateObjectsRecords);

        return stateMachineRecord;
    }

    private static Type[] GetTypeHierarchy(StateObject stateObject) {
        return GetTypeHierarchyRecursive(stateObject, 0);
    }

    private static Type[] GetTypeHierarchyRecursive(StateObject stateObject, int depth) {
        if(stateObject is State) {
            Type[] hierarchy = new Type[depth + 1]; 
            hierarchy[depth] = stateObject.GetType();
            return hierarchy;
        } else {
            Type[] hierarchy = GetTypeHierarchyRecursive(((StateMachine)stateObject).CurrentStateObject, depth + 1);
            hierarchy[depth] = stateObject.GetType();
            return hierarchy;
        }
    }


    private static object[] GetStateObjectsRecords(StateObject stateObject) {
        return GetStateObjectsRecordsRecursive(stateObject, 0);
        
    }

    private static object[] GetStateObjectsRecordsRecursive(StateObject stateObject, int depth) {
        if (stateObject is State) {
            object[] stateObjectsRecords = new object[depth + 1];
            stateObjectsRecords[depth] = stateObject.RecordFieldsAndProperties();
            return stateObjectsRecords;
        } else {
            object[] stateObjectsRecords = GetStateObjectsRecordsRecursive(((StateMachine)stateObject).CurrentStateObject, depth+1);
            stateObjectsRecords[depth] = stateObject.RecordFieldsAndProperties();
            return stateObjectsRecords;
        }
    }

    private static CharacterMovementRecord RecordCharacterMovementData(CharacterMovement characterMovement) {
        return new CharacterMovementRecord(characterMovement.Velocity);
    }
}

