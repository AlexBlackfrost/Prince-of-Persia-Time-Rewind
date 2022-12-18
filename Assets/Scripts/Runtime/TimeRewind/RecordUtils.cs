using Cinemachine;
using HFSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerRecord {
    public TransformRecord playerTransform;
    public CameraRecord cameraRecord;
    public AnimationRecord animationRecord;
    public StateMachine stateMachine;
    public float deltaTime;

    public PlayerRecord(TransformRecord playerTransform, CameraRecord cameraRecord, AnimationRecord animationRecord,
                        StateMachine stateMachine, float deltaTime) {

        this.playerTransform = playerTransform;
        this.cameraRecord = cameraRecord;
        this.animationRecord = animationRecord;
        this.stateMachine = stateMachine;
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
    public int shortNameHash;
    public float duration;
    public float normalizedTime;
    public TransformRecord[] pose;
    public AnimationParameter[] parameters;
    public bool isInTransition;
    public TransitionRecord transitionRecord;

    public AnimationRecord(bool applyRootMotion, TransformRecord[] pose, int shortNameHash,
                           float normalizedTime, float duration, AnimationParameter[] parameters) {
        this.applyRootMotion = applyRootMotion;
        this.pose = pose;
        this.shortNameHash = shortNameHash;
        this.normalizedTime = normalizedTime;
        this.duration = duration;
        this.parameters = parameters;
        this.isInTransition = false;
        transitionRecord = default(TransitionRecord);
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

    public static AnimationRecord RecordAnimationData(Animator animator, Transform[] bones) {
        TransformRecord[] pose = RecordPose(bones);
        AnimationParameter[] parameters = RecordAnimatorParameters(animator);
        int layer = 0;
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(layer);
        AnimationRecord animationRecord = new AnimationRecord(animator.applyRootMotion, pose, stateInfo.shortNameHash, 
                                   stateInfo.normalizedTime, stateInfo.length ,parameters);

        string output = "ShortNameHash: " + stateInfo.shortNameHash +
                        " NormalizedTime: " + stateInfo.normalizedTime;

        if (animator.IsInTransition(layer)) {
            AnimatorTransitionInfo transitionInfo = animator.GetAnimatorTransitionInfo(layer);
            AnimatorStateInfo nextStateInfo = animator.GetNextAnimatorStateInfo(layer);
            TransitionRecord transitionRecord = new TransitionRecord(nextStateInfo.shortNameHash,
                                                                     transitionInfo.normalizedTime,
                                                                     transitionInfo.duration,
                                                                     nextStateInfo.normalizedTime,
                                                                     nextStateInfo.length);
            
            animationRecord.isInTransition = true;
            animationRecord.transitionRecord = transitionRecord;


            output += " NextNameHash: " + nextStateInfo.shortNameHash +
                        " NextStateNormalizedTime: " + nextStateInfo.normalizedTime +
                        " TransitionDuration: " + transitionInfo.duration +
                        " TransitionNormalizedTime: " + transitionInfo.normalizedTime;
            
        }

        Debug.Log(output);
        return animationRecord;
    }

    public static PlayerRecord RecordPlayerData(Transform transform, Camera camera, StateMachine stateMachine, 
                                                Animator animator, Transform[] bones) {

        TransformRecord transformRecord = RecordTransformData(transform);
        CameraRecord cameraRecord = RecordCameraData(camera);
        AnimationRecord animationRecord = RecordAnimationData(animator, bones);
        StateMachine stateMachineCopy = (StateMachine) stateMachine.Copy();
        Debug.Log("Saving... " + stateMachineCopy.GetCurrentStateName());
        return new PlayerRecord(transformRecord, cameraRecord, animationRecord, stateMachineCopy, Time.deltaTime);
    }

    private static TransformRecord[] RecordPose(Transform[] bones) {
        TransformRecord[] pose = new TransformRecord[bones.Length];
        int index = 0;
        foreach (Transform bone in bones) {
            pose[index++] = RecordTransformData(bone);
        }
        return pose;
    }

    private static AnimationParameter[] RecordAnimatorParameters(Animator animator) {
        AnimationParameter[] parameters = new AnimationParameter[animator.parameterCount];
        int i = 0;
        foreach (AnimatorControllerParameter parameter in animator.parameters) {
            object value = null;
            switch (parameter.type) {
                case AnimatorControllerParameterType.Float:
                    value = animator.GetFloat(parameter.nameHash);
                    break;

                case AnimatorControllerParameterType.Int:
                    value = animator.GetInteger(parameter.nameHash);
                    break;

                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    value = animator.GetBool(parameter.nameHash);
                    break;
            }
            parameters[i++] = new AnimationParameter(parameter.type, parameter.nameHash, value);

        }
        return parameters;
    }
}
