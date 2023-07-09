using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RewindableVariableBase<T> : IRewindable {
    private T value;
    private bool isModified;
    public bool IsModified {
        get {
            return isModified;
        }
        set {
            if(!isModified && value) {
                RewindController.Instance.IncreaseNumModifiedVariablesByOne();
            }
            isModified = value;
        } 
    }

    public RewindableVariableBase(T value) {
        this.value = value;
        IsModified = false;
        RewindController.Instance.Register(this);
    }

    public RewindableVariableBase() {
        this.value = default(T);
        IsModified = false;
        RewindController.Instance.Register(this);
    }

    public T Value {
        get {
            return value;
        }
        set {
            if (!EqualityComparer<T>.Default.Equals(this.value, value)) {
                this.value = value;
                IsModified = true;
            }
        }
    }


    public abstract object Record();
    public abstract void Rewind(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord);
    public abstract void OnRewindStart();
    public abstract void OnRewindStop(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord);
}