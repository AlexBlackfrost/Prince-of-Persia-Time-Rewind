using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRewindable {
    public bool HasBeenRecordedAtLeastOnce { get; set; }
    public int MaxFramesWithoutBeingRecorded { get;}
    public int FramesWithoutBeingRecorded { get; set; }
    public bool IsModified { get; set; }
    public object Record();
    public void Rewind(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord);
    public void OnRewindStart();
    public void OnRewindStop(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord);
}