
using Cinemachine;
using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerController : MonoBehaviour {
    [field:SerializeField] private CharacterMovement characterMovement;

    [Header("State machine settings")]
    [field: SerializeField] private IdleState.IdleSettings idleSettings;
    [field: SerializeField] private MoveState.MoveSettings moveSettings;
    [field: SerializeField] private JumpState.JumpSettings jumpSettings;
    [field: SerializeField] private TimeControlStateMachine.TimeForwardSettings timeForwardSettings;

    public InputController InputController { get; private set; }
    
    private Animator animator;
    public RootStateMachine rootStateMachine;
    private TimeRewinder timeRewinder;
    private Dictionary<Type, StateObject> stateObjects;


    private void Awake() {
        characterMovement.Transform = transform;
        characterMovement.CharacterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        InputController = GetComponent<InputController>();
        timeRewinder = GetComponent<TimeRewinder>();
        stateObjects = new Dictionary<Type, StateObject>();

        InitHFSMSettings();
        BuildHFSM();
        rootStateMachine.Init();
    }

    private void BuildHFSM() {
        // Create states and state machines
        IdleState idleState = new IdleState(idleSettings);
        MoveState moveState = new MoveState(moveSettings);
        JumpState jumpState = new JumpState(jumpSettings);
        TimeControlStateMachine timeForwardStateMachine = new TimeControlStateMachine(UpdateMode.UpdateAfterChild, timeForwardSettings, idleState, moveState, jumpState);
        rootStateMachine = new RootStateMachine(timeForwardStateMachine);

        // Create transitions
        InputController.Jump.performed += idleState.AddEventTransition<CallbackContext>(jumpState);
        idleState.AddTransition(moveState, IsMoving);
         
        InputController.Jump.performed += moveState.AddEventTransition<CallbackContext>(jumpState);
        moveState.AddTransition(idleState, IsNotMoving);

        AnimatorUtils.AnimationEnded += jumpState.AddEventTransition<int>(idleState, JumpAnimationEnded);

        // Store them to modify their values after rewinding
        stateObjects[typeof(IdleState)] = idleState;
        stateObjects[typeof(MoveState)] = moveState;
        stateObjects[typeof(JumpState)] = jumpState;
        stateObjects[typeof(TimeControlStateMachine)] = timeForwardStateMachine;

    } 

    private void InitHFSMSettings() {
        idleSettings.Animator = animator;
        idleSettings.CharacterMovement = characterMovement;

        moveSettings.CharacterMovement = characterMovement;
        moveSettings.Animator = animator;
        moveSettings.InputController = InputController;

        jumpSettings.Animator = animator;

        timeForwardSettings.TimeRewinder = timeRewinder;
        timeForwardSettings.Transform = transform;
        timeForwardSettings.Camera = Camera.main;
        timeForwardSettings.Animator = animator;
        timeForwardSettings.InputController = InputController;
        timeForwardSettings.StateObjects = stateObjects;
        timeForwardSettings.SkinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        timeForwardSettings.CharacterMovement = characterMovement;
    }


    #region Transitions
    private bool IsMoving() {
        return InputController.IsMoving();
    }

    private bool IsNotMoving() {
        return !InputController.IsMoving();
    }
    #endregion

    private void Update() {
        rootStateMachine.Update();
        Debug.Log("Current state: " + rootStateMachine.GetCurrentStateName() );
    }

    private void FixedUpdate() {
        rootStateMachine.FixedUpdate();
    }

    private void LateUpdate() {
        rootStateMachine.LateUpdate();
    }

    #region Conditions
    private bool JumpAnimationEnded(int stateNameHash) {
        return Animator.StringToHash("Jump") == stateNameHash;
    }

    private bool StateNameHashEquals(int stateNameHash, string stateName) {
        return Animator.StringToHash(stateName) == stateNameHash;
    }
    #endregion
}
