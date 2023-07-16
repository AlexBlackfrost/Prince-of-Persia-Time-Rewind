using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindableVariable<BasicType> : RewindableVariableBase<BasicType> {
    public bool InterpolationEnabled { get; set; }
    public RewindableVariable(BasicType value) : base(value) {
        SetInterpolationEnabledDefaultValue();
    }

    public RewindableVariable(BasicType value, bool interpolationEnabled, bool onlyExecuteOnRewindStop) : base(value, onlyExecuteOnRewindStop: onlyExecuteOnRewindStop) {
        InterpolationEnabled = interpolationEnabled;
    }

    public RewindableVariable() : base() {
        SetInterpolationEnabledDefaultValue();
    }

    public RewindableVariable(bool interpolationEnabled) : base() {
        InterpolationEnabled = interpolationEnabled;
    }

    private void SetInterpolationEnabledDefaultValue() {
        if (typeof(BasicType) == typeof(float) ||
           typeof(BasicType) == typeof(double) ||
           typeof(BasicType) == typeof(Vector2) ||
           typeof(BasicType) == typeof(Vector3)) {

            InterpolationEnabled = true;

        } else {
            InterpolationEnabled = false;
        }
    }

    public override object Record() {
        return Value;
    }

    public override void Rewind(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        if (InterpolationEnabled) {
            float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;
            Value = (BasicType)Interpolate(previousRecord, nextRecord, lerpAlpha);
        } else {
            if(previousRecord.GetType() != typeof(BasicType)) {
                string type1 = previousRecord.GetType().ToString();
                string type2 = typeof(BasicType).ToString();
                Debug.Log("Casting different types: " + type1 + " and "+ type2);
            }
            
            Value = (BasicType)previousRecord;
        }
    }

    public override void OnRewindStart() { }

    public override void OnRewindStop(object previousRecord, object nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        Rewind(previousRecord, nextRecord, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
    }

    private object Interpolate(object a, object b, float t) {
        if (a.GetType() != b.GetType()) {
            throw new NotImplementedException();
        }

        if (a is Vector2) {
            return Vector2.Lerp((Vector2)a, (Vector2)b, t);

        } else if (a is Vector3) {
            return Vector3.Lerp((Vector3)a, (Vector3)b, t);

        } else if (a is float || a is double) {
            return Mathf.Lerp((float)a, (float)b, t);

        } else {
            return Mathf.Lerp((float)a, (float)b, t);
        }
    }
}