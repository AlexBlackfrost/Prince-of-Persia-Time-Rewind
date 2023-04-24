using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIAttackState : State{

    [Serializable]
    public class AIAttackSettings {
        [field:SerializeField] public float RotationSpeed { get; private set; } = 8f;
        public Animator Animator { get; set; }
        public CharacterMovement CharacterMovement { get; set; }
        public GameObject Target { get; set; }
        public Transform Transform { get; set; }
        public Sword Sword { get; set; }
    }

    private AIAttackSettings settings;
    public AIAttackState(AIAttackSettings settings) {
        this.settings = settings;
    }

    protected override void OnEnter() {
        settings.Animator.applyRootMotion = true;
        settings.Animator.SetBool(AnimatorUtils.attackHash, true);
    }

    protected override void OnUpdate() {
        Vector3 targetDirection = settings.Target.transform.position - settings.Transform.position;
        targetDirection.y = 0;
        targetDirection.Normalize();
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion rotation = Quaternion.Slerp(settings.Transform.rotation, targetRotation, settings.RotationSpeed * Time.deltaTime);
        settings.CharacterMovement.SetRotation(rotation);
    }

    protected override void OnExit() {
        settings.Animator.applyRootMotion = false;
        settings.Animator.SetBool(AnimatorUtils.attackHash, false);
        settings.Sword.StartCooldown();
    }
}