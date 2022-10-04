using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class WriterState : State {
    private StringBuilder stringBuilder;

    public WriterState(StringBuilder stringBuilder) {
        this.stringBuilder = stringBuilder;
    }

    protected override void OnUpdate() {
        stringBuilder.Append("StateMessage");
    }
}

