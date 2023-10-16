using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TimeRewinder<RecordType> : MonoBehaviour{
    protected int recordFPS = 120;
    protected int recordMaxSeconds = 30;
    protected CircularStack<RecordType> records;

    protected void Awake(){
        records = new CircularStack<RecordType>(recordFPS*recordMaxSeconds);
    }

    public void Push(RecordType record) {
        records.Push(record);
    }

    public RecordType Pop() {
        return records.Pop();
    }

    public int Count() {
        return records.Count;
    }

    public RecordType Peek() { 
        return records.Peek(); 
    }

    public float GetRecordedDataRatio01() {
        return (float)records.Count / records.Size();
    }
}