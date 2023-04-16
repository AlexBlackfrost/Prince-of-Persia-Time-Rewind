using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MyStruct {
    public int a;
    public OtherStruct other;
    public MyStruct(int a, OtherStruct other) {
        this.a = a;
        this.other = other;
    }
}

public struct OtherStruct {
    public int b;
    public OtherStruct(int b) {
        this.b = b;
    }
}

public class RefTest : MonoBehaviour{

    private void Start(){
        OtherStruct other = new OtherStruct(50);
        MyStruct myStruct = new MyStruct(5, other);
        Debug.Log("MyStruct.a before = " + myStruct.a);
        Debug.Log("OtherStruct.b before = " + myStruct.other.b);
        SomeFunction(ref myStruct);

        Debug.Log("MyStruct.a after = " + myStruct.a);
        Debug.Log("OtherStruct.b after = " + myStruct.other.b);
    }

    private void SomeFunction(ref MyStruct mystruct) {
        mystruct.a = 7;
        Debug.Log("SomeFunction OtherStruct.b before: " + mystruct.other.b);
        ref OtherStruct other = ref mystruct.other;
        //mystruct.other.b = 70;
        other.b = 70; 
        Debug.Log("SomeFunction OtherStruct.b after: " + mystruct.other.b);
    }
}