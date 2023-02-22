
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
    [field: SerializeField] private GrabSwordState.GrabSwordSettings grabSwordSettings;

    public InputController InputController { get; private set; }
    
    private Animator animator;
    public RootStateMachine rootStateMachine;
    private TimeRewinder timeRewinder;
    private Dictionary<Type, StateObject> stateObjects;
    private GameObject wall;
    private PerceptionSystem perceptionSystem;

    private void Awake() {
        characterMovement.Transform = transform;
        characterMovement.CharacterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        InputController = GetComponent<InputController>();
        perceptionSystem = GetComponent<PerceptionSystem>();
        timeRewinder = GetComponent<TimeRewinder>();
        stateObjects = new Dictionary<Type, StateObject>();

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
        GrabSwordState grabSwordState = new GrabSwordState(grabSwordSettings);
        TimeControlStateMachine timeControlStateMachine = new TimeControlStateMachine(UpdateMode.UpdateAfterChild, 
                                                                                      timeControlSettings, 
                                                                                      idleState, moveState, jumpState,
                                                                                      wallRunState, fallState, landState,
                                                                                      grabSwordState);
        rootStateMachine = new RootStateMachine(timeControlStateMachine);

        // Create transitions
        // Idle ->
        InputController.Jump.performed += idleState.AddEventTransition<CallbackContext>(jumpState);
        idleState.AddTransition(moveState, IsMoving);
        idleState.AddTransition(grabSwordState, InputController.IsAttackPressed);
         
        // Move ->
        InputController.Jump.performed += moveState.AddEventTransition<CallbackContext>(jumpState);
        moveState.AddTransition(idleState, IsNotMoving);
        moveState.AddTransition(wallRunState, SetWall, InputController.IsWallRunPressed, perceptionSystem.IsRunnableWallNear);

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

        //GrabSword ->
        grabSwordState.AddTransition(idleState, () => { return grabSwordState.GrabbedSword; });

        // Store them to modify their values after rewinding
        stateObjects[typeof(IdleState)] = idleState;
        stateObjects[typeof(MoveState)] = moveState;
        stateObjects[typeof(JumpState)] = jumpState;
        stateObjects[typeof(WallRunState)] = wallRunState;
        stateObjects[typeof(FallState)] = fallState;
        stateObjects[typeof(LandState)] = landState;
        stateObjects[typeof(TimeControlStateMachine)] = timeControlStateMachine;

    } 

    private void InjectDependencies() {
        idleSettings.Animator = animator;
        idleSettings.CharacterMovement = characterMovement;

        moveSettings.CharacterMovement = characterMovement;
        moveSettings.Animator = animator;
        moveSettings.InputController = InputController;

        jumpSettings.Animator = animator;

        timeControlSettings.TimeRewinder = timeRewinder;
        timeControlSettings.Transform = transform;
        timeControlSettings.Camera = Camera.main;
        timeControlSettings.Animator = animator;
        timeControlSettings.InputController = InputController;
        timeControlSettings.StateObjects = stateObjects;
        timeControlSettings.CharacterMovement = characterMovement;

        wallRunSettings.Animator = animator;
        wallRunSettings.Transform = transform;
        wallRunSettings.CharacterMovement = characterMovement;

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
