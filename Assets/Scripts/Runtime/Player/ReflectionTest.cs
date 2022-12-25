using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


public class RewindableObject {
    public object value;
    public RewindableObject(object value) {
        this.value = value;
    }
}

public class Int : RewindableObject {
    public Int(int value) : base(value) {

    }
}

public struct Record {
    public object[] objects;
    public bool[] bitmask;
    public Record(object[] objects, bool[] bitmask) {
        this.objects = objects;
        this.bitmask = bitmask;
    }
}

public class ReflectionTest : MonoBehaviour {
    private object integer;
    public float floating;
    private bool boolean;

    private void Start() {
        InitIndexes();
    }

    private Dictionary<int, RewindableObject> variables;
    private Dictionary<string, int> variableIndexes;
    private void InitIndexes() {
        integer = 5;
        List<object> objects = new List<object>();
        objects.Add(integer);
        
        FieldInfo[] fields = typeof(ReflectionTest).GetFields(BindingFlags.DeclaredOnly |
                                                              BindingFlags.NonPublic|
                                                              BindingFlags.Public|
                                                              BindingFlags.Instance);
        for (int i = 0; i < fields.Length; i++) {
            Debug.Log("Property: " + fields[i].Name);
            variableIndexes[fields[i].Name] = i;
        }
    }

    private void Restore(Record record) {
        int objIndex = 0;
        for(int i = 0; i < record.bitmask.Length; i++) {
            if (record.bitmask[i]) {
                if(variables[i].GetType() == typeof(RewindableObject)) {
                    RewindableObject intWrapper = (RewindableObject)variables[i];
                    intWrapper.value = (int) record.objects[objIndex++];
                }

                // other data types

            }
        }
    }

    private Record Save() {
        List<object> objects = new List<object>();
        bool[] bitmask = new bool[objects.Count];
        foreach(KeyValuePair<int,RewindableObject> entry in variables) {
            //if(variable) is different then
            objects.Add(entry.Value.value);
            bitmask[entry.Key] = true;
            //else bitmask[entry.Key] = false;
        }

        
        Record record = new Record(objects.ToArray(), bitmask);
        return record;
    }


}

