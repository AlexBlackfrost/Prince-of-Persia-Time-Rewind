using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class WriterStateMachine : StateMachine {

    private StringBuilder stringBuilder;
    public WriterStateMachine(StringBuilder stringBuilder, params StateObject[] states) : base(states) {
        this.stringBuilder = stringBuilder;
    }

    protected override void OnUpdate() {
        stringBuilder.Append("StateMachineMessage");
    }
}

