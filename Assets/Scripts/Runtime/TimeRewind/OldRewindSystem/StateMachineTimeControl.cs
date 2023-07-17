using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachineTimeControl {
    private StateMachine stateMachine;
    private State noneState;
    public StateMachineTimeControl(StateMachine stateMachine) {
        this.stateMachine = stateMachine;
        noneState = new NoneState();
    }
    public void OnTimeRewindStart() {
        stateMachine.CurrentStateObject.Value.Exit();
        // Do not change state using ChangeState() so that OnStateEnter is not triggered after rewind stops.
        stateMachine.CurrentStateObject.Value = noneState;
    }

    public void OnTimeRewindStop() {

    }
     
    public StateMachineRecord RecordStateMachineData() {
        return stateMachine.RecordStateMachine();
        
    }

    public void RestoreStateMachineRecord(StateMachineRecord record) {
        stateMachine.RestoreStateMachineRecord(record);
    }
}