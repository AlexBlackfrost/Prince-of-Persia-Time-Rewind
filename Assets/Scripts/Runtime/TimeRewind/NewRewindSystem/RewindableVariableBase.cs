using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RewindableVariableBase<T> : IRewindable {
    protected T value;
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
    public bool LimitMaxFramesWithoutBeingRecorded { get; set; }
    public bool HasBeenRecordedAtLeastOnce { get; set; }
    public int MaxFramesWithoutBeingRecorded { get; private set; }
    public int FramesWithoutBeingRecorded { get; set; }
    private bool isModified;
    public virtual bool IsModified {
        get {
            return isModified;
        }
        set {
            if(!isModified && value && FramesWithoutBeingRecorded < MaxFramesWithoutBeingRecorded ) { 
                /* rewind controller already takes care of counting variables that haven't been recorded since the last @MaxFramesWithoutBeingRecorded,
                 * so don't increase the number of modified variables or it will be counted twice */
                RewindController.Instance.IncreaseNumModifiedVariablesThisFrameBy1();
            }
            isModified = value;
        } 
    }


    public RewindableVariableBase(T value, int maxFramesWithoutBeingRecorded = 10, bool limitMaxFramesWithoutBeingRecorded = true) {
        this.value = value;
        this.MaxFramesWithoutBeingRecorded = maxFramesWithoutBeingRecorded;
        this.LimitMaxFramesWithoutBeingRecorded = limitMaxFramesWithoutBeingRecorded;
        IsModified = false;
        HasBeenRecordedAtLeastOnce = false;
        RewindController.Instance.Register(this);
    }

    public RewindableVariableBase(int maxFramesWithoutBeingRecorded = 10, bool limitMaxFramesWithoutBeingRecorded = true) {
        this.value = default(T);
        this.MaxFramesWithoutBeingRecorded = maxFramesWithoutBeingRecorded;
        this.LimitMaxFramesWithoutBeingRecorded = limitMaxFramesWithoutBeingRecorded;
        IsModified = false;
        HasBeenRecordedAtLeastOnce = false;
        RewindController.Instance.Register(this);
    }




    public abstract object Record();
    public abstract void Rewind(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord);
    public abstract void OnRewindStart();
    public abstract void OnRewindStop(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord);
}