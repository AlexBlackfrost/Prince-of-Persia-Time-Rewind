
using Cinemachine;
using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerController : MonoBehaviour {
    [field: SerializeField] public CharacterMovement CharacterMovement { get; set; }

    [Header("Combat")]
    [SerializeField] public Health health;
    [SerializeField] private Hurtbox hurtbox;
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
    [field: SerializeField] private DamagedState.DamagedSettings damagedSettings;
    [field: SerializeField] private ParriedState.ParriedSettings parriedSettings;
    [field: SerializeField] private StrafeState.StrafeSettings strafeSettings;
    [field: SerializeField] private AliveStateMachine.AliveSettings aliveSettings;
    [field: SerializeField] private DeadState.DeadSettings deadSettings;

    [Header("VFX")]
    [SerializeField] private ParticleSystem leftFootstepSmokeVFX;
    [SerializeField] private ParticleSystem rightFootstepSmokeVFX;
    [SerializeField] private ParticleSystem blood;

    [Header("SFX")]
    [SerializeField] private AudioSource footstep;
    [SerializeField] private AudioSource land;
    [SerializeField] private AudioSource roll;
    [SerializeField] private float footstepMinPitch;
    [SerializeField] private float footstepMaxPitch;
    [SerializeField] private float wallRunFootstepMinPitch = 1.2f;
    [SerializeField] private float wallRunFootstepMaxPitch = 1.4f;


    public InputController InputController { get; private set; }
    public RootStateMachine rootStateMachine;

    private Animator animator;
    private PlayerPerceptionSystem perceptionSystem;
    private PlayerTimeRewinder playerTimeRewinder;



    private void Awake() {
        CharacterMovement.Transform = transform;
        CharacterMovement.CharacterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerTimeRewinder = GetComponent<PlayerTimeRewinder>();
        InputController = GetComponent<InputController>();
        perceptionSystem = GetComponent<PlayerPerceptionSystem>();

        sword.OnEquipped(this.gameObject);

        SubscribeEvents();
        health.Init();

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
        DamagedState damagedState = new DamagedState(damagedSettings);
        ParriedState parriedState = new ParriedState(parriedSettings);
        StrafeState strafeState = new StrafeState(strafeSettings);
        AliveStateMachine aliveStateMachine = new AliveStateMachine(aliveSettings,
                                                                    idleState, moveState, jumpState,
                                                                    wallRunState, fallState, landState,
                                                                    attackState, rollState, blockState, 
                                                                    parriedState, strafeState, damagedState);
        DeadState deadState = new DeadState(deadSettings);
        PlayerTimeControlStateMachine timeControlStateMachine = new PlayerTimeControlStateMachine(UpdateMode.UpdateAfterChild, timeControlSettings, 
                                                                                                  aliveStateMachine, deadState);
        rootStateMachine = new RootStateMachine(timeControlStateMachine);
        // Create transitions
        // Idle ->
        InputController.Roll.performed += idleState.AddEventTransition<CallbackContext>(rollState, IsGroundAhead);
        InputController.Jump.performed += idleState.AddEventTransition<CallbackContext>(jumpState);
        InputController.Block.performed += idleState.AddEventTransition<CallbackContext>(blockState, SwordIsInHand);
        idleState.AddTransition(fallState, NoGroundBelow);
        idleState.AddTransition(strafeState, IsMoving, SwordIsInHand, perceptionSystem.IsEnemyInsideStrafeDetectionRadius);
        idleState.AddTransition(moveState, IsMoving);
        hurtbox.DamageReceived += idleState.AddEventTransition<float, IDamageSource>(damagedState, ApplyDamageEffect);
        InputController.Attack.performed += idleState.AddEventTransition<CallbackContext>(attackState, SwordIsInHand);
         
        // Move ->
        moveState.AddTransition(strafeState, IsMoving, SwordIsInHand, perceptionSystem.IsEnemyInsideStrafeDetectionRadius);
        InputController.Roll.performed += moveState.AddEventTransition<CallbackContext>(rollState, IsGroundAhead);
        hurtbox.DamageReceived += moveState.AddEventTransition<float, IDamageSource>(damagedState, ApplyDamageEffect);
        InputController.Jump.performed += moveState.AddEventTransition<CallbackContext>(jumpState);
        InputController.Block.performed += moveState.AddEventTransition<CallbackContext>(blockState, SwordIsInHand);
        moveState.AddTransition(fallState, NoGroundBelow);
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
        fallState.AddTransition(landState, perceptionSystem.IsGroundNearToLand);

        // Land ->
        AnimatorUtils.AnimationEnded += landState.AddEventTransition<int>(idleState, LandAnimationEnded);

        // Attack ->
        hurtbox.DamageReceived += attackState.AddEventTransition<float, IDamageSource>(damagedState);
        attackState.AttackEnded += attackState.AddEventTransition(idleState, IsNotMoving);
        attackState.AttackEnded += attackState.AddEventTransition(strafeState, IsMoving, perceptionSystem.IsEnemyInsideStrafeIgnoreRadius);
        attackState.AttackEnded += attackState.AddEventTransition(moveState, IsMoving);
        attackState.Parried += attackState.AddEventTransition(parriedState);
        InputController.Block.performed += attackState.AddEventTransition<CallbackContext>(blockState);

        // Roll ->
        AnimatorUtils.AnimationEnded += rollState.AddEventTransition<int>(idleState, RollAnimationEnded, (int shortNameHash) => { return IsNotMoving(); });
        AnimatorUtils.AnimationEnded += rollState.AddEventTransition<int>(moveState, RollAnimationEnded, (int shortNameHash) => { return IsMoving(); });

        // Block ->
        hurtbox.DamageReceived += blockState.AddEventTransition<float, IDamageSource>(damagedState);
        AnimatorUtils.AnimationEnded += blockState.AddEventTransition<int>(idleState, BlockAnimationEnded, (int shortNameHash) => IsNotMoving() );
        AnimatorUtils.AnimationEnded += blockState.AddEventTransition<int>(strafeState, BlockAnimationEnded, (int shortNameHash) => IsMoving(),
                                                                                                             (int shortNameHash) => perceptionSystem.IsEnemyInsideStrafeIgnoreRadius());
        AnimatorUtils.AnimationEnded += blockState.AddEventTransition<int>(moveState, BlockAnimationEnded, (int shortNameHash) => IsMoving() );

        // Parried ->
        AnimatorUtils.AnimationEnded += parriedState.AddEventTransition<int>(strafeState, ParriedAnimationEnded, (int shortNameHash) => IsMoving(),
                                                                                                                 (int shortNameHash) => perceptionSystem.IsEnemyInsideStrafeIgnoreRadius());
        AnimatorUtils.AnimationEnded += parriedState.AddEventTransition<int>(moveState, ParriedAnimationEnded, (int shortNameHash) => IsMoving());
        AnimatorUtils.AnimationEnded += parriedState.AddEventTransition<int>(idleState, ParriedAnimationEnded, (int shortNameHash) => IsNotMoving());

        // Strafe ->
        InputController.Attack.performed += strafeState.AddEventTransition<CallbackContext>(attackState, SwordIsInHand);
        hurtbox.DamageReceived += strafeState.AddEventTransition<float, IDamageSource>(damagedState, ApplyDamageEffect);
        strafeState.AddTransition(moveState, () => !perceptionSystem.IsEnemyInsideStrafeIgnoreRadius());
        InputController.Block.performed += strafeState.AddEventTransition<CallbackContext>(blockState, SwordIsInHand);
        strafeState.AddTransition(idleState, IsNotMoving);
        InputController.Roll.performed += strafeState.AddEventTransition<CallbackContext>(rollState, IsGroundAhead);

        // Damaged ->
        AnimatorUtils.AnimationEnded += damagedState.AddEventTransition<int>(blockState, DamagedAnimationEnded, (int shortNameHash) => InputController.Block.IsPressed());
        AnimatorUtils.AnimationEnded += damagedState.AddEventTransition<int>(strafeState, DamagedAnimationEnded, (int shortNameHash) => IsMoving(), 
                                                                                                               (int shortNameHash)=> perceptionSystem.IsEnemyInsideStrafeIgnoreRadius());
        AnimatorUtils.AnimationEnded += damagedState.AddEventTransition<int>(moveState, DamagedAnimationEnded, (int shortNameHash) => IsMoving());
        AnimatorUtils.AnimationEnded += damagedState.AddEventTransition<int>(idleState, DamagedAnimationEnded, (int shortNameHash) => IsNotMoving());

        // Alive ->
        health.Dead += aliveStateMachine.AddEventTransition(deadState);

    } 

    private void InjectHFSMDependencies() {
        idleSettings.Animator = animator;
        idleSettings.CharacterMovement = CharacterMovement;
        idleSettings.Sword = sword;

        moveSettings.CharacterMovement = CharacterMovement;
        moveSettings.Animator = animator;
        moveSettings.InputController = InputController;
        moveSettings.Sword = sword;

        jumpSettings.Animator = animator;
        jumpSettings.Sword = sword;

        timeControlSettings.Transform = transform;
        timeControlSettings.Camera = Camera.main;
        timeControlSettings.Animator = animator;
        timeControlSettings.InputController = InputController;
        timeControlSettings.CharacterMovement = CharacterMovement;
        timeControlSettings.Sword = sword;
        timeControlSettings.Health = health;
        timeControlSettings.Hurtbox = hurtbox;
        timeControlSettings.PlayerTimeRewinder = playerTimeRewinder;

        wallRunSettings.Animator = animator;
        wallRunSettings.Transform = transform;
        wallRunSettings.CharacterMovement = CharacterMovement;
        wallRunSettings.Sword = sword;

        fallSettings.Animator = animator;
        fallSettings.CharacterMovement = CharacterMovement;
        fallSettings.InputController = InputController;
        fallSettings.MainCamera = Camera.main;
        fallSettings.Transform = transform;

        landSettings.Animator = animator;
        landSettings.CharacterMovement = CharacterMovement;
        landSettings.InputController = InputController;
        landSettings.MainCamera = Camera.main;
        landSettings.Transform = transform;

        attackSettings.Animator = animator;
        attackSettings.Sword = sword;
        attackSettings.InputController = InputController;
        attackSettings.CharacterMovement = CharacterMovement;
        attackSettings.Transform = transform;
        attackSettings.PerceptionSystem = perceptionSystem;

        rollSettings.Animator = animator;
        rollSettings.CharacterMovement = CharacterMovement;
        rollSettings.InputController = InputController;
        rollSettings.Transform = transform;
        rollSettings.MainCamera = Camera.main;
        rollSettings.Hurtbox = hurtbox;

        blockSettings.Animator = animator;
        blockSettings.Hurtbox = hurtbox;

        damagedSettings.Animator = animator;
        damagedSettings.CharacterMovement = CharacterMovement;
        damagedSettings.Blood = blood;

        parriedSettings.Animator = animator;

        strafeSettings.Animator = animator;
        strafeSettings.InputController = InputController;
        strafeSettings.Transform = transform;
        strafeSettings.MainCamera = Camera.main;
        strafeSettings.PerceptionSystem = perceptionSystem;
        strafeSettings.CharacterMovement = CharacterMovement;

        deadSettings.Animator = animator;
    }


    private void SubscribeEvents() {
        InputController.Attack.performed += (CallbackContext ctx) => { sword.OnUnsheathePressed(); };
        InputController.Sheathe.performed += (CallbackContext ctx) => { sword.OnSheathePressed(); };

        hurtbox.DamageReceived += health.OnDamageReceived;
        hurtbox.DamageReceived += PlayBloodVFX;

        hurtbox.Parry += OnParry;

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
    
    private bool DamagedAnimationEnded(int stateNameHash) {
        return AnimatorUtils.damagedHash == stateNameHash;
    }

    private bool IsNotDetectingRunnableWall(){
        return !perceptionSystem.IsRunnableWallNear();
    }

    private bool SwordIsInHand(CallbackContext ctx) {
        return sword.IsInHand();
    }

    private bool SwordIsInHand() {
        return sword.IsInHand();
    }

    private bool IsGroundAhead(CallbackContext ctx) {
        return perceptionSystem.IsGroundAhead();
    }

    private bool NoGroundBelow() {
        return perceptionSystem.IsGroundFarToFall();
    }
    #endregion

    #region Transition actions
    private void SetWall() {
        wallRunSettings.Wall = perceptionSystem.CurrentWall;
        wallRunSettings.WallSide = perceptionSystem.CurrentWallDirection;
    }

    private void ApplyDamageEffect(float damageAmount, IDamageSource damageSource) {
        damageSource.ApplyDamageEffect(this.gameObject);
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
        int attackLayerIndex = 0;
        // Don't enable hitbox when the animation event is fired during an animation transitions whose source state is AIAttack.
        if (animator.IsInTransition(attackLayerIndex)) {
            AnimatorStateInfo currentStateInfo = animator.GetCurrentAnimatorStateInfo(attackLayerIndex);
            if (enabled == Bool.False || (currentStateInfo.shortNameHash != AnimatorUtils.attack1Hash && 
                                          currentStateInfo.shortNameHash != AnimatorUtils.attack2Hash &&
                                          currentStateInfo.shortNameHash != AnimatorUtils.attack3Hash )) {
                sword.SetHitboxEnabled(enabled);
            }
        } else {
            sword.SetHitboxEnabled(enabled);
        }
        sword.SetHitboxEnabled(enabled);
    }

    public void SetIsShielded(Bool enabled) {
        hurtbox.SetIsShielded(Convert.ToBoolean((int)enabled));
    }

    public void PlayFootstepSmokeVFX(Direction foot) {
        if(foot == Direction.Left) {
            leftFootstepSmokeVFX.Play();

        }else if(foot == Direction.Right) {
            rightFootstepSmokeVFX.Play();
        }
    }

    private void PlayBloodVFX(float damageAmount, IDamageSource damageSource) {
        Vector3 damageDirection = (transform.position - damageSource.DamageApplier.transform.position).normalized;
        blood.transform.rotation = Quaternion.LookRotation(damageDirection);
    }

    private void OnParry(GameObject parriedCharacter) {
        sword.PlaySwordClashVFX();
    }

    public void PlayFootstepAudio() {
        int locomotionLayerIndex = 0;
        if (!animator.IsInTransition(locomotionLayerIndex) || 
            (animator.IsInTransition(locomotionLayerIndex) && animator.GetNextAnimatorStateInfo(locomotionLayerIndex).shortNameHash == AnimatorUtils.runHash) ) {

            footstep.pitch = UnityEngine.Random.Range(footstepMinPitch, footstepMaxPitch);
            footstep.Play();
        }
    }

    public void PlayWallFootstepAudio() {
        footstep.pitch = UnityEngine.Random.Range(wallRunFootstepMinPitch, wallRunFootstepMaxPitch);
        footstep.Play();
    }

    public void PlayLandAudio() {
        land.Play();
    }

    public void PlayRollAudio() {
        roll.Play();
    }

    public void PlayWeaponAudio(int index) {
        sword.PlaySwordWhoosh(index);
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

}
