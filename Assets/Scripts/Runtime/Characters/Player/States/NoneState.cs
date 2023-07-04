using HFSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoneState :State { // State used while rewinding time
    public NoneState() : base() { }

    public override object RecordFieldsAndProperties() {
        return null;
    }

    public override void RestoreFieldsAndProperties(object fieldsAndProperties) { }
}

