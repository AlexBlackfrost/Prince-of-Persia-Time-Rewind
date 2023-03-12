
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
    [field: SerializeField] private TimeControlStateMachine.TimeControlSettings timeControlSettings;
    [field: SerializeField] private WallRunState.WallRunSettings wallRunSettings;
    [field: SerializeField] private FallState.FallSettings fallSettings;
    [field: SerializeField] private LandState.LandSettings landSettings;
    [field: SerializeField] private AttackState.AttackSettings attackSettings;

    public InputController InputController { get; private set; }
    
    private Animator animator;
    public RootStateMachine rootStateMachine;
    private TimeRewinder timeRewinder;
    private Dictionary<Type, StateObject> stateObjects;
    private PerceptionSystem perceptionSystem;
    private Sword sword;

    private void Awake() {
        characterMovement.Transform = transform;
        characterMovement.CharacterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        InputController = GetComponent<InputController>();
        perceptionSystem = GetComponent<PerceptionSystem>();
        timeRewinder = GetComponent<TimeRewinder>();
        sword = GetComponent<Sword>();
        stateObjects = new Dictionary<Type, StateObject>();

        InputController.Attack.performed += (CallbackContext ctx) => { sword.OnUnsheathePressed(); };
        InputController.Sheathe.performed += (CallbackContext ctx) => { sword.OnSheathePressed(); };

        InjectDependencies();
        BuildHFSM();
        rootStateMachine.Init();
    }

    private void BuildHFSM() {
        // Create states and state machines
        IdleState idleState = new IdleState(idleSettings);
        MoveState moveState = new MoveState(moveSettings);
        JumpState jumpState = new JumpState(jumpSettings);
        WallRunState wallRunState = new WallRunState(wallRunSettings);
        FallState fallState = new FallState(fallSettings);
        LandState landState = new LandState(landSettings);
        AttackState attackState = new AttackState(attackSettings);
        TimeControlStateMachine timeControlStateMachine = new TimeControlStateMachine(UpdateMode.UpdateAfterChild, 
                                                                                      timeControlSettings, 
                                                                                      idleState, moveState, jumpState,
                                                                                      wallRunState, fallState, landState,
                                                                                      attackState);
        rootStateMachine = new RootStateMachine(timeControlStateMachine);

        // Create transitions
        // Idle ->
        InputController.Jump.performed += idleState.AddEventTransition<CallbackContext>(jumpState);
        idleState.AddTransition(moveState, IsMoving);
        InputController.Attack.performed += idleState.AddEventTransition<CallbackContext>(attackState, SwordIsInHand);
         
        // Move ->
        InputController.Jump.performed += moveState.AddEventTransition<CallbackContext>(jumpState);
        moveState.AddTransition(idleState, IsNotMoving);
        moveState.AddTransition(wallRunState, SetWall, InputController.IsWallRunPressed, perceptionSystem.IsRunnableWallNear);
        InputController.Attack.performed += moveState.AddEventTransition<CallbackContext>(attackState, SwordIsInHand);

        // Jump ->
        AnimatorUtils.AnimationEnded += jumpState.AddEventTransition<int>(idleState, JumpAnimationEnded);

        // WallRun ->
        AnimatorUtils.AnimationEnded += wallRunState.AddEventTransition<int>(fallState, WallRunAnimationEnded);
        wallRunState.AddTransition(fallState, InputController.IsWallRunNotPressed);
        wallRunState.AddTransition(fallState, IsNotDetectingRunnableWall);

        // Fall ->
        fallState.AddTransition(landState, perceptionSystem.IsGroundNear);

        // Land ->
        AnimatorUtils.AnimationEnded += landState.AddEventTransition<int>(idleState, LandAnimationEnded);

        // Attack ->
        attackState.AttackEnded += attackState.AddEventTransition(idleState, IsNotMoving);
        attackState.AttackEnded += attackState.AddEventTransition(moveState, IsMoving);

        // Store them to modify their values after rewinding
        stateObjects[typeof(IdleState)] = idleState;
        stateObjects[typeof(MoveState)] = moveState;
        stateObjects[typeof(JumpState)] = jumpState;
        stateObjects[typeof(WallRunState)] = wallRunState;
        stateObjects[typeof(FallState)] = fallState;
        stateObjects[typeof(LandState)] = landState;
        stateObjects[typeof(AttackState)] = attackState;
        stateObjects[typeof(TimeControlStateMachine)] = timeControlStateMachine;

    } 

    private void InjectDependencies() {
        idleSettings.Animator = animator;
        idleSettings.CharacterMovement = characterMovement;
        idleSettings.Sword = sword;

        moveSettings.CharacterMovement = characterMovement;
        moveSettings.Animator = animator;
        moveSettings.InputController = InputController;
        moveSettings.Sword = sword;

        jumpSettings.Animator = animator;
        jumpSettings.Sword = sword;

        timeControlSettings.TimeRewinder = timeRewinder;
        timeControlSettings.Transform = transform;
        timeControlSettings.Camera = Camera.main;
        timeControlSettings.Animator = animator;
        timeControlSettings.InputController = InputController;
        timeControlSettings.StateObjects = stateObjects;
        timeControlSettings.CharacterMovement = characterMovement;
        timeControlSettings.Sword = sword;

        wallRunSettings.Animator = animator;
        wallRunSettings.Transform = transform;
        wallRunSettings.CharacterMovement = characterMovement;
        wallRunSettings.Sword = sword;

        fallSettings.Animator = animator;
        fallSettings.CharacterMovement = characterMovement;
        fallSettings.InputController = InputController;
        fallSettings.MainCamera = Camera.main;
        fallSettings.Transform = transform;

        landSettings.Animator = animator;
        landSettings.CharacterMovement = characterMovement;
        landSettings.InputController = InputController;
        landSettings.MainCamera = Camera.main;
        landSettings.Transform = transform;

        attackSettings.Animator = animator;
        attackSettings.Sword = sword;
        attackSettings.InputController = InputController;
        attackSettings.CharacterMovement = characterMovement;
        attackSettings.Transform = transform;

    }

    #region Transition conditions
    private bool IsMoving() {
        return InputController.IsMoving();
    }

    private bool IsNotMoving() {
        return !InputController.IsMoving();
    }
    private bool JumpAnimationEnded(int stateNameHash) {
        return Animator.StringToHash("Jump") == stateNameHash;
    }

    private bool WallRunAnimationEnded(int stateNameHash) {
        return Animator.StringToHash("WallRunRight") == stateNameHash ||
               Animator.StringToHash("WallRunLeft") == stateNameHash;
    }

    private bool LandAnimationEnded(int stateNameHash) {
        return Animator.StringToHash("Land") == stateNameHash;
    }

    private bool StateNameHashEquals(int stateNameHash, string stateName) {
        return Animator.StringToHash(stateName) == stateNameHash;
    }

    private bool IsNotDetectingRunnableWall(){
        return !perceptionSystem.IsRunnableWallNear();
    }

    private bool SwordIsInHand(CallbackContext ctx) {
        return sword.IsInHand();
    }
    #endregion

    #region Transition actions
    private void SetWall() {
        wallRunSettings.Wall = perceptionSystem.CurrentWall;
        wallRunSettings.WallSide = perceptionSystem.CurrentWallDirection;
    }
    #endregion

    private void Update() {
        rootStateMachine.Update();
        //Debug.Log("Current state: " + rootStateMachine.GetCurrentStateName() );
    }

    private void FixedUpdate() {
        rootStateMachine.FixedUpdate();
    }

    private void LateUpdate() {
        rootStateMachine.LateUpdate();
    }

    
}
