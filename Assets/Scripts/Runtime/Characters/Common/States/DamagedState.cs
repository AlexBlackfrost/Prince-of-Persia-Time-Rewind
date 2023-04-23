using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagedState : State{
    [Serializable]
    public class DamagedSettings {
        public Animator Animator { get; set; }
    }

    private DamagedSettings settings;
    public DamagedState(DamagedSettings settings) {
        this.settings = settings;
    }

    protected override void OnEnter() {
        settings.Animator.SetTrigger(AnimatorUtils.damagedHash);
    }

    protected override void OnExit() {

    }

    protected override void OnUpdate() {

    }
}
