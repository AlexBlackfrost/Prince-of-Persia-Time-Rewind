using HFSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TransformData {
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 localScale;
    public float deltaTime;

    public TransformData(Vector3 position, Quaternion rotation, Vector3 localScale, float deltaTime) {
        this.position = position;
        this.rotation = rotation;
        this.localScale = localScale;
        this.deltaTime = deltaTime;
    }

    public override string ToString() {
        return "Position: " + position + "\nRotation: " + rotation + "\nScale: " + localScale + "\n";
    }
}

public class TimeRewinder : MonoBehaviour {
    public Stack<StateMachine> stateMachineStack;
    public Stack<Dictionary<string, TransformData>> rigTransformStack;
    public Stack<TransformData> transformStack;

    private void Awake() {
        stateMachineStack = new Stack<StateMachine>();
        rigTransformStack = new Stack<Dictionary<string, TransformData>>();
        transformStack = new Stack<TransformData>();
    }

    

    public void DebugStateMachineStack() {
        while (stateMachineStack.Peek() != null) {
            StateMachine root = stateMachineStack.Pop();
            Debug.Log(root.GetCurrentStateName());
        }
    }

    public void DebugTransformDataStack() {
        if(rigTransformStack.Peek() != null) {
            Dictionary<string, TransformData> rigTransformData = rigTransformStack.Pop();
            foreach( KeyValuePair<string, TransformData>  boneTransformData in rigTransformData) {
                Debug.Log(boneTransformData.Key + ": " +boneTransformData.Value.ToString());
            }  
        }
    }
    
}

