using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State {

    [Serializable] public class AttackSettings {
        public Sword Sword { get; set; }
        public Animator Animator { get; set; }
    }

    private AttackSettings settings;
    private int attackHash;
    public AttackState(AttackSettings settings) : base() {
        this.settings = settings;
        attackHash = Animator.StringToHash("Attack");
    }

    protected override void OnEnter() {
        settings.Animator.SetTrigger(attackHash);
    }

    protected override void OnExit() {
        
    }
}