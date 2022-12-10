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

    public AnimationRecord(bool applyRootMotion) {
        this.applyRootMotion = applyRootMotion;
    }
}

public struct TransitionRecord {
    public int currentStateNameHash;
    public int nextStateNameHash;
    public float normalizedTime;
    public float currentStateNormalizedTime;

    public TransitionRecord(int currentStateNameHash, int nextStateNameHash, float normalizedTime, float currentStateNormalizedTime) {
        this.currentStateNameHash = currentStateNameHash;
        this.nextStateNameHash = nextStateNameHash;
        this.normalizedTime = normalizedTime;
        this.currentStateNormalizedTime = currentStateNormalizedTime;
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

    public static AnimationRecord RecordAnimationData(Animator animator) {
        return new AnimationRecord(animator.applyRootMotion);
    }

    public static PlayerRecord RecordPlayerData(Transform transform, Camera camera, StateMachine stateMachine, Animator animator) {
        TransformRecord transformRecord = RecordTransformData(transform);
        CameraRecord cameraRecord = RecordCameraData(camera);
        AnimationRecord animationRecord = RecordAnimationData(animator);
        StateMachine stateMachineCopy = (StateMachine) stateMachine.Copy();
        Debug.Log("Saving... " + stateMachineCopy.GetCurrentStateName());
        return new PlayerRecord(transformRecord, cameraRecord, animationRecord, stateMachineCopy, Time.deltaTime);
    }
}

