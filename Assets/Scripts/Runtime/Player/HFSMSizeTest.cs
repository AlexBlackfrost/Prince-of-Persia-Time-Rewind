using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HFSMSizeTest : MonoBehaviour {
    // Start is called before the first frame update
    public struct HFSMRecord {
        Type[] hierarchy;
        float time;
        Dictionary<Type, (byte, object)> values;
        public HFSMRecord(Type[] hierarchy, float time, Dictionary<Type, (byte, object)> values) {
            this.hierarchy = hierarchy;
            this.time = time;
            this.values = values;
        }
    }
    public struct TestStruct {
        public int a;
        public TestStruct(int a) {
            this.a = a;
        }
    }
    private void Start() {
        StateMachine sm = GetComponent<PlayerController>().rootStateMachine;
        int fps = 60;
        int seconds = 20;
        //var a = new TestStruct(2);
        //Dictionary<Type, object> dict = new Dictionary<Type, object>();
        //dict.Add(typeof(MoveState), a);
        

        List<StateMachine> list = new List<StateMachine>(fps*seconds);
        for (int i = 0; i < fps*seconds; i++) {
            //list[i] = (StateMachine)sm.Copy();
        }

    }

    // Update is called once per frame
    private void Update() {
        
    }
}

