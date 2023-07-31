using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RewindableVariableBase<T> : IRewindable {
#if UNITY_EDITOR
    public string Name { get ; set; }
#endif   
    [SerializeField]protected T value;
    public virtual T Value {
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
    public bool OnlyExecuteOnRewindStop { get ; set; }
    public bool RecordedAtLeastOnce { get; set; }
    public int MaxFramesWithoutBeingRecorded { get; protected set; }
    public int FramesWithoutBeingRecorded { get; set; }
    protected bool isModified;
    public virtual bool IsModified {
        get {
            return isModified;
        }
        set {
            if(!isModified && value  && FramesWithoutBeingRecorded < MaxFramesWithoutBeingRecorded && RecordedAtLeastOnce ) { 
                /* rewind controller already takes care of counting variables that haven't been recorded since the last @MaxFramesWithoutBeingRecorded,
                 * so don't increase the number of modified variables or it will be counted twice */
                TimeRewindController.Instance.IncreaseNumModifiedVariablesThisFrameBy1();
            }
            isModified = value;
        } 
    }


    public RewindableVariableBase(T value, int maxFramesWithoutBeingRecorded = 10, bool onlyExecuteOnRewindStop = false) {
        this.value = value;
        this.OnlyExecuteOnRewindStop = onlyExecuteOnRewindStop;
        this.MaxFramesWithoutBeingRecorded = maxFramesWithoutBeingRecorded;
        IsModified = false;
        RecordedAtLeastOnce = false;
        int id = TimeRewindController.Instance.Register(this);
 
    }

    public RewindableVariableBase(int maxFramesWithoutBeingRecorded = 10, bool onlyExecuteOnRewindStop = false) {
        this.value = default(T);
        this.OnlyExecuteOnRewindStop = onlyExecuteOnRewindStop;
        this.MaxFramesWithoutBeingRecorded = maxFramesWithoutBeingRecorded;
        IsModified = false;
        RecordedAtLeastOnce = false;
        TimeRewindController.Instance.Register(this);
    }

    public abstract object Record();
    public abstract void Rewind(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord);
    public abstract void OnRewindStart();
    public abstract void OnRewindStop(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord);
}