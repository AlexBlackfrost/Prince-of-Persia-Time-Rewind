using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AliveStateMachine : StateMachine{
    [Serializable]
    public class AliveSettings {

    }

    private AliveSettings settings;
    public AliveStateMachine(AliveSettings settings, params StateObject[] stateObjects) :base(UpdateMode.UpdateBeforeChild, stateObjects) {
        this.settings = settings;
    }
}