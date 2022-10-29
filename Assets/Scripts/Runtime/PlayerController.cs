
using HFSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    [field:SerializeField] private CharacterMovement characterMovement;

    [Header("State machine settings")]
    [field: SerializeField] private IdleState.IdleSettings idleSettings;
    [field: SerializeField] private MoveState.MoveSettings moveSettings;

    private Animator animator;
    private InputController inputController;
    private RootStateMachine rootStateMachine;

    private void Awake() {
        characterMovement.CharacterController = GetComponent<CharacterController>();
        characterMovement.Transform = transform;
        animator = GetComponent<Animator>();
        inputController = GetComponent<InputController>();

        InitHFSMSettings();
        BuildHFSM();
        rootStateMachine.Init();
    }

    private void BuildHFSM() {
        IdleState idleState = new IdleState(idleSettings);
        MoveState moveState = new MoveState(moveSettings);
        rootStateMachine = new RootStateMachine(idleState, moveState);

        idleState.AddTransition(moveState, IsMoving);
        moveState.AddTransition(idleState, IsNotMoving);
    }

    private void InitHFSMSettings() {
        idleSettings.Animator = animator;

        moveSettings.CharacterMovement = characterMovement;
        moveSettings.Animator = animator;
        moveSettings.InputController = inputController;
    }
    #region Transitions
    private bool IsMoving() {
        return inputController.IsMoving();
    }

    private bool IsNotMoving() {
        return !inputController.IsMoving();
    }
    #endregion

    private void Update() {
        rootStateMachine.Update();
    }
}
