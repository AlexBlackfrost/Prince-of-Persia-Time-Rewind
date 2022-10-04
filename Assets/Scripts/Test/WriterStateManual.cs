using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class WriterStateManual : State {
    private StringBuilder stringBuilder;

    public WriterStateManual(StringBuilder stringBuilder) {
        this.stringBuilder = stringBuilder;
    }

    protected override void OnUpdate() {
        //StateMachine.Update();
        stringBuilder.Append("StateMessage");
    }
}

