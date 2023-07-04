using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandState : State {
    [Serializable] public class LandSettings {
        public Animator Animator { get; set; }
        public InputController InputController { get; set; }
        public CharacterMovement CharacterMovement { get; set; }
        public Camera MainCamera { get; set; }
        public Transform Transform { get; set; }
    }

    private LandSettings settings;

    public LandState(LandSettings settings) : base() {
        this.settings = settings;
    }

    protected override void OnEnter() {
        Vector3 currentVelocity = settings.CharacterMovement.Velocity;
        currentVelocity.x = 0;
        currentVelocity.z = 0;
        settings.CharacterMovement.Velocity = currentVelocity;
        settings.Animator.SetTrigger(AnimatorUtils.landHash);
    }

    protected override void OnUpdate() {
        Vector2 inputDirection = settings.InputController.GetMoveDirection();
        Vector3 lookDirection = settings.Transform.forward;

        if(inputDirection.magnitude > float.Epsilon) {
            lookDirection = settings.MainCamera.transform.TransformDirection(inputDirection.x, 0, inputDirection.y);
            lookDirection.y = 0;
            lookDirection.Normalize();
        }

        settings.CharacterMovement.Move(Vector3.zero);
        settings.CharacterMovement.SetRotation(Quaternion.LookRotation(lookDirection));
    }

    protected override void OnExit() {
    }

    public override object RecordFieldsAndProperties() {
        return null;
    }

    public override void RestoreFieldsAndProperties(object fieldsAndProperties) { }
}

