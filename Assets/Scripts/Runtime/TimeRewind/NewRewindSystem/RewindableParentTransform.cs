using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindableParentTransform : RewindableVariable<Transform>{
    public RewindableParentTransform(Transform transform) : base(transform) { }

    public override void OnRewindStop(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        Rewind(previousRecord, nextRecord, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
    }

    public override object Record() {
        return value.parent;
    }

    public override void Rewind(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        Transform parentTransform = (Transform)previousRecord;
        value.SetParent(parentTransform, false);
    }
}