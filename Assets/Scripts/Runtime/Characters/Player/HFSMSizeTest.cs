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
        Test3();

    }
    private void Test1() {
        StateMachine sm = GetComponent<PlayerController>().rootStateMachine;
        int fps = 60;
        int seconds = 20;
        //var a = new TestStruct(2);
        //Dictionary<Type, object> dict = new Dictionary<Type, object>();
        //dict.Add(typeof(MoveState), a);


        List<StateMachine> list = new List<StateMachine>(fps * seconds);
        for (int i = 0; i < fps * seconds; i++) {
            //list[i] = (StateMachine)sm.Copy();
        }
    }

    public struct Test2Struct {
        public float floating;
        public float integer;
    }
    private void Test2() {
        //Dictionary<bool, bool> dictLow = new Dictionary<bool, bool>(1);
        DictionaryEntry entry = new DictionaryEntry(true, true);
        //Dictionary<int, Test2Struct> dict = new Dictionary<int, Test2Struct>(5);
    }

    public class Dummy {
        public float var1;
        public float var2;
        public float var3;
        public Dummy(float var1, float var2, float var3) {
            this.var1 = var1;
            this.var2 = var2;
            this.var3 = var3;
        }
    }

    public struct DummyStruct {
        public Dummy dummyObj;
        public DummyStruct(Dummy dummyObj) {
            //this.dummyObj = new Dummy(dummyObj.var1, dummyObj.var2, dummyObj.var3);
            this.dummyObj = dummyObj;
        }
    }

    private void Test3() {
        Dummy dummy1 = new Dummy(1, 2, 3);
        DummyStruct dummyStruct1 = new DummyStruct(dummy1);
        DummyStruct dummyStruct2 = new DummyStruct(dummy1);
        DummyStruct dummyStruct3 = new DummyStruct(dummy1);
        DummyStruct dummyStruct4 = new DummyStruct(dummy1);
        DummyStruct dummyStruct5 = new DummyStruct(dummy1);
        DummyStruct dummyStruct6 = new DummyStruct(dummy1);
        DummyStruct dummyStruct7 = new DummyStruct(dummy1);
        DummyStruct dummyStruct8 = new DummyStruct(dummy1);
        //Dummy dummy2 = dummy1;
        //Dummy dummy3 = dummy1;
    }
   

    private void Update() {
        
    }
}

