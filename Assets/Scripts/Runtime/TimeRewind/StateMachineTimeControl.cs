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
        stateMachine.CurrentStateObject.Exit();
        // Do not change state using ChangeState() so that OnStateEnter is not triggered after rewind stops.
        stateMachine.CurrentStateObject = noneState;
    }

    public void OnTimeRewindStop() {

    }
     
    public StateMachineRecord RecordStateMachineData() {
        Type[] hierarchy = GetTypeHierarchy(stateMachine.CurrentStateObject);
        object[] stateObjectsRecords = GetStateObjectsRecords(stateMachine.CurrentStateObject);
        StateMachineRecord stateMachineRecord = new StateMachineRecord(hierarchy, stateObjectsRecords);

        return stateMachineRecord;
    }

    private static Type[] GetTypeHierarchy(StateObject stateObject) {
        return GetTypeHierarchyRecursive(stateObject, 0);
    }

    private static Type[] GetTypeHierarchyRecursive(StateObject stateObject, int depth) {
        if (stateObject is State) {
            Type[] hierarchy = new Type[depth + 1];
            hierarchy[depth] = stateObject.GetType();
            return hierarchy;
        } else {
            Type[] hierarchy = GetTypeHierarchyRecursive(((StateMachine)stateObject).CurrentStateObject, depth + 1);
            hierarchy[depth] = stateObject.GetType();
            return hierarchy;
        }
    }


    private static object[] GetStateObjectsRecords(StateObject stateObject) {
        return GetStateObjectsRecordsRecursive(stateObject, 0);

    }

    private static object[] GetStateObjectsRecordsRecursive(StateObject stateObject, int depth) {
        if (stateObject is State) {
            object[] stateObjectsRecords = new object[depth + 1];
            stateObjectsRecords[depth] = stateObject.RecordFieldsAndProperties();
            return stateObjectsRecords;
        } else {
            object[] stateObjectsRecords = GetStateObjectsRecordsRecursive(((StateMachine)stateObject).CurrentStateObject, depth + 1);
            stateObjectsRecords[depth] = stateObject.RecordFieldsAndProperties();
            return stateObjectsRecords;
        }
    }

    public void RestoreStateMachineRecord(Dictionary<Type, StateObject> stateObjects, StateMachineRecord record) {
        for (int i = 0; i < record.hierarchy.Length - 1; i++) {
            Type id = record.hierarchy[i];
            StateMachine stateMachine = (StateMachine)stateObjects[id];
            stateMachine.IsActive = true;
            stateMachine.CurrentStateObject = stateObjects[record.hierarchy[i + 1]];
            stateMachine.RestoreFieldsAndProperties(record.stateObjectRecords[i]);
        }
        int leaftStateIndex = record.hierarchy.Length - 1;
        Type leaftStateId = record.hierarchy[leaftStateIndex];
        StateObject leafState = stateObjects[leaftStateId];
        leafState.IsActive = true;
        leafState.RestoreFieldsAndProperties(record.stateObjectRecords[leaftStateIndex]);
        stateMachine.CurrentStateObject = stateObjects[record.hierarchy[0]];
    }
}