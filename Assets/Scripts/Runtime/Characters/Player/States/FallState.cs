using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallState : State {

    [Serializable] public class FallSettings {
        public CharacterMovement CharacterMovement { get; set; }
        public Animator Animator { get; set; }
        public InputController InputController { get; set; }
        public Camera MainCamera { get; set; }
        public Transform Transform { get; set; }
    }

    private FallSettings settings;
    private int fallHash;

    public FallState(FallSettings settings) : base() {
        this.settings = settings;
        fallHash = Animator.StringToHash("Fall");
    }

    protected override void OnEnter() {
        settings.Animator.SetTrigger(fallHash);
    }

    protected override void OnUpdate() {
        /*
        Vector2 inputDirection = settings.InputController.GetMoveDirection();
        Vector3 lookDirection = settings.Transform.forward;

        if (inputDirection.magnitude > float.Epsilon) {
            lookDirection = settings.MainCamera.transform.TransformDirection(inputDirection.x, 0, inputDirection.y);
            lookDirection.y = 0;
            lookDirection.Normalize();
        }

        settings.CharacterMovement.Move(Vector3.zero);
        settings.CharacterMovement.SetRotation(Quaternion.LookRotation(lookDirection));
        */
        Vector2 inputDirection = settings.InputController.GetMoveDirection();
        Vector3 moveDirection = settings.MainCamera.transform.TransformDirection(inputDirection.x, 0, inputDirection.y);
        moveDirection.y = 0;
        moveDirection.Normalize();
        settings.CharacterMovement.Move(moveDirection);
    }

    protected override void OnExit() {
        
    }
}

