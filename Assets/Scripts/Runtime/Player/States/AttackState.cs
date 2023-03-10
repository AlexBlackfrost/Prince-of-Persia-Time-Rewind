using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State {
    

    [Serializable] public class AttackSettings {
        public Sword Sword { get; set; }
        public Animator Animator { get; set; }
        public InputController InputController { get; set; }
        [field: SerializeField] public float AttackPressedBufferTime { get; private set; }  = 0.3f;
    }

    private const int ATTACK_PRESSED_BUFFER_SIZE = 500;
    private AttackSettings settings;
    private int attackHash;
    private AttackInputBuffer attackInputBuffer;
    private bool comboEnabled;

    public AttackState(AttackSettings settings) : base() {
        this.settings = settings;
        settings.Sword.OnSetComboEnabled = SetComboEnabled;
        comboEnabled = false;
        attackInputBuffer = new AttackInputBuffer(ATTACK_PRESSED_BUFFER_SIZE);
        attackHash = Animator.StringToHash("Attack");
    }

    protected override void OnEnter() {
        settings.Animator.SetTrigger(attackHash);
        settings.Sword.SetSwordAnimatorLayerEnabled(false);
        settings.Animator.applyRootMotion = true;
        attackInputBuffer.Clear();
    }

    protected override void OnUpdate() {
        bool isAttackPressed = settings.InputController.IsAttackPressed();
        float now = Time.time;
        attackInputBuffer.Push(new AttackInput(isAttackPressed, now));

        if (comboEnabled && attackInputBuffer.WasAttackPressedInLastSeconds(settings.AttackPressedBufferTime)) {
            comboEnabled = false;
            attackInputBuffer.Clear();
            settings.Animator.SetTrigger(attackHash);
        }
    }

    protected override void OnExit() {
        settings.Sword.SetSwordAnimatorLayerEnabled(true);
        settings.Animator.applyRootMotion = false;
    }

    public void SetComboEnabled(bool enabled) {
        comboEnabled = enabled;
    }
}