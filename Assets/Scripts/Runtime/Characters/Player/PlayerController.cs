
using Cinemachine;
using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerController : MonoBehaviour {
    [field:SerializeField] private CharacterMovement characterMovement;

    [Header("Combat")]
    [SerializeField] private Hurtbox hurtbox;
    [SerializeField] private Health health;
    [SerializeField] private Sword sword;

    [Header("State machine settings")]
    [field: SerializeField] private IdleState.IdleSettings idleSettings;
    [field: SerializeField] private MoveState.MoveSettings moveSettings;
    [field: SerializeField] private JumpState.JumpSettings jumpSettings;
    [field: SerializeField] private PlayerTimeControlStateMachine.PlayerTimeControlSettings timeControlSettings;
    [field: SerializeField] private WallRunState.WallRunSettings wallRunSettings;
    [field: SerializeField] private FallState.FallSettings fallSettings;
    [field: SerializeField] private LandState.LandSettings landSettings;
    [field: SerializeField] private AttackState.AttackSettings attackSettings;
    [field: SerializeField] private RollState.RollSettings rollSettings;
    [field: SerializeField] private BlockState.BlockSettings blockSettings;
    [field: SerializeField] private ParriedState.ParriedSettings parriedSettings;
    [field: SerializeField] private AliveStateMachine.AliveSettings aliveSettings;
    [field: SerializeField] private DeadState.DeadSettings deadSettings;

    public InputController InputController { get; private set; }
    public RootStateMachine rootStateMachine;

    private Animator animator;
    private Dictionary<Type, StateObject> stateObjects;
    private PlayerPerceptionSystem perceptionSystem;



    private void Awake() {
        characterMovement.Transform = transform;
        characterMovement.CharacterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        InputController = GetComponent<InputController>();
        perceptionSystem = GetComponent<PlayerPerceptionSystem>();
        stateObjects = new Dictionary<Type, StateObject>();

        health.Init();
        sword.OnEquipped(this.gameObject);

        SubscribeEvents();

        InjectHFSMDependencies();
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
        RollState rollState = new RollState(rollSettings);
        BlockState blockState = new BlockState(blockSettings);
        ParriedState parriedState = new ParriedState(parriedSettings);
        AliveStateMachine aliveStateMachine = new AliveStateMachine(aliveSettings,
                                                                    idleState, moveState, jumpState,
                                                                    wallRunState, fallState, landState,
                                                                    attackState, rollState, blockState, 
                                                                    parriedState);
        DeadState deadState = new DeadState(deadSettings);
        PlayerTimeControlStateMachine timeControlStateMachine = new PlayerTimeControlStateMachine(UpdateMode.UpdateAfterChild, timeControlSettings, 
                                                                                                  aliveStateMachine, deadState);
        rootStateMachine = new RootStateMachine(timeControlStateMachine);

        // Create transitions
        // Idle ->
        InputController.Roll.performed += idleState.AddEventTransition<CallbackContext>(rollState, IsGroundAhead);
        InputController.Jump.performed += idleState.AddEventTransition<CallbackContext>(jumpState);
        InputController.Block.performed += idleState.AddEventTransition<CallbackContext>(blockState, SwordIsInHand);
        idleState.AddTransition(moveState, IsMoving);
        InputController.Attack.performed += idleState.AddEventTransition<CallbackContext>(attackState, SwordIsInHand);
         
        // Move ->
        InputController.Roll.performed += moveState.AddEventTransition<CallbackContext>(rollState, IsGroundAhead);
        InputController.Jump.performed += moveState.AddEventTransition<CallbackContext>(jumpState);
        InputController.Block.performed += moveState.AddEventTransition<CallbackContext>(blockState, SwordIsInHand);
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
        attackState.Parried += attackState.AddEventTransition(parriedState);

        // Roll ->
        AnimatorUtils.AnimationEnded += rollState.AddEventTransition<int>(idleState, RollAnimationEnded, (int shortNameHash) => { return IsNotMoving(); });
        AnimatorUtils.AnimationEnded += rollState.AddEventTransition<int>(moveState, RollAnimationEnded, (int shortNameHash) => { return IsMoving(); });

        // Block ->
        AnimatorUtils.AnimationEnded += blockState.AddEventTransition<int>(idleState, BlockAnimationEnded, (int shortNameHash) => IsNotMoving() );
        AnimatorUtils.AnimationEnded += blockState.AddEventTransition<int>(moveState, BlockAnimationEnded, (int shortNameHash) => IsMoving() );

        // Parried ->
        AnimatorUtils.AnimationEnded += parriedState.AddEventTransition<int>(moveState, ParriedAnimationEnded, (int shortNameHash) => IsMoving());
        AnimatorUtils.AnimationEnded += parriedState.AddEventTransition<int>(idleState, ParriedAnimationEnded, (int shortNameHash) => IsNotMoving());

        // Alive ->
        health.Dead += aliveStateMachine.AddEventTransition(deadState);

        // Store them to modify their values after rewinding
        stateObjects[typeof(IdleState)] = idleState;
        stateObjects[typeof(MoveState)] = moveState;
        stateObjects[typeof(JumpState)] = jumpState;
        stateObjects[typeof(WallRunState)] = wallRunState;
        stateObjects[typeof(FallState)] = fallState;
        stateObjects[typeof(LandState)] = landState;
        stateObjects[typeof(AttackState)] = attackState;
        stateObjects[typeof(RollState)] = rollState;
        stateObjects[typeof(BlockState)] = blockState;
        stateObjects[typeof(ParriedState)] = parriedState;
        stateObjects[typeof(DeadState)] = deadState;
        stateObjects[typeof(AliveStateMachine)] = aliveStateMachine;
        stateObjects[typeof(PlayerTimeControlStateMachine)] = timeControlStateMachine;

    } 

    private void InjectHFSMDependencies() {
        idleSettings.Animator = animator;
        idleSettings.CharacterMovement = characterMovement;
        idleSettings.Sword = sword;

        moveSettings.CharacterMovement = characterMovement;
        moveSettings.Animator = animator;
        moveSettings.InputController = InputController;
        moveSettings.Sword = sword;

        jumpSettings.Animator = animator;
        jumpSettings.Sword = sword;

        timeControlSettings.Transform = transform;
        timeControlSettings.Camera = Camera.main;
        timeControlSettings.Animator = animator;
        timeControlSettings.InputController = InputController;
        timeControlSettings.StateObjects = stateObjects;
        timeControlSettings.CharacterMovement = characterMovement;
        timeControlSettings.Sword = sword;
        timeControlSettings.Health = health;
        timeControlSettings.Hurtbox = hurtbox;

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

        rollSettings.Animator = animator;
        rollSettings.CharacterMovement = characterMovement;
        rollSettings.InputController = InputController;
        rollSettings.Transform = transform;
        rollSettings.MainCamera = Camera.main;

        blockSettings.Animator = animator;
        blockSettings.Hurtbox = hurtbox;

        parriedSettings.Animator = animator;

        deadSettings.Animator = animator;
    }


    private void SubscribeEvents() {
        InputController.Attack.performed += (CallbackContext ctx) => { sword.OnUnsheathePressed(); };
        InputController.Sheathe.performed += (CallbackContext ctx) => { sword.OnSheathePressed(); };

        hurtbox.DamageReceived += health.OnDamageReceived;
    }

    #region Transition conditions
    private bool IsMoving() {
        return InputController.IsMoving();
    }

    private bool IsNotMoving() {
        return !InputController.IsMoving();
    }
    private bool JumpAnimationEnded(int stateNameHash) {
        return AnimatorUtils.jumpHash == stateNameHash;
    }

    private bool WallRunAnimationEnded(int stateNameHash) {
        return AnimatorUtils.wallRunRightHash == stateNameHash ||
               AnimatorUtils.wallRunLeftHash == stateNameHash;
    }

    private bool LandAnimationEnded(int stateNameHash) {
        return AnimatorUtils.landHash == stateNameHash;
    }

    private bool RollAnimationEnded(int stateNameHash) {
        return AnimatorUtils.rollHash == stateNameHash;
    }
    
    private bool BlockAnimationEnded(int stateNameHash) {
        return AnimatorUtils.blockHash == stateNameHash;
    }
    
    private bool ParriedAnimationEnded(int stateNameHash) {
        return AnimatorUtils.parriedHash == stateNameHash;
    }

    private bool IsNotDetectingRunnableWall(){
        return !perceptionSystem.IsRunnableWallNear();
    }

    private bool SwordIsInHand(CallbackContext ctx) {
        return sword.IsInHand();
    }

    private bool IsGroundAhead(CallbackContext ctx) {
        return perceptionSystem.IsGroundAhead();
    }
    #endregion

    #region Transition actions
    private void SetWall() {
        wallRunSettings.Wall = perceptionSystem.CurrentWall;
        wallRunSettings.WallSide = perceptionSystem.CurrentWallDirection;
    }
    #endregion


    #region Animation Event Callbacks
    public void SwitchSwordSocket() {
        sword.SwitchSwordSocket();
    }

    public void SetRotationEnabled(Bool enabled) {
        sword.SetRotationEnabled(enabled);
    }

    public void SetComboEnabled(Bool enabled) {
        sword.SetComboEnabled(enabled);
    }

    public void SetHitboxEnabled(Bool enabled) {
        sword.SetHitboxEnabled(enabled);
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