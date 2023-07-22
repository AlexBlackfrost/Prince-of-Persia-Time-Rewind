using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindableHashSet<T> : RewindableVariableBase<HashSet<T>> {

    public RewindableHashSet(bool onlyExecuteOnRewindStop) : base(onlyExecuteOnRewindStop:onlyExecuteOnRewindStop) {
        Value = new HashSet<T>();
    }

    public RewindableHashSet() : base() {
        Value = new HashSet<T>();
    }

    public void Clear() {
        Value.Clear();
        //IsModified = true;
    }

    public bool Contains(T item) {
        return Value.Contains(item);
    }

    public void Add(T item) {
        Value.Add(item);
        IsModified = true;
    }

    public override void OnRewindStart() { }

    public override void OnRewindStop(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        Rewind(previousRecord, nextRecord, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
    }

    public override object Record() {
        T[] array = new T[Value.Count];
        Value.CopyTo(array);
        return array;
    }

    public override void Rewind(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        T[] items = (T[])previousRecord;
        Value.Clear();
        foreach(T item in items) {
            Value.Add(item);
        }
    }


}