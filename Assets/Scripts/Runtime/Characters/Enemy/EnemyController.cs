using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour{
    [field: SerializeField] private CharacterMovement characterMovement;

    [Header("Combat")]
    [SerializeField] private Hurtbox hurtbox;
    [SerializeField] private Health health;
    [SerializeField] private Sword sword;
    [SerializeField] private EnemyAI enemyAI;

    [Header("State machine settings")]
    [field: SerializeField] private IdleState.IdleSettings idleSettings;
    [field: SerializeField] private ApproachPlayerState.ApproachPlayerSettings approachPlayerSettings;
    [field: SerializeField] private AIAttackState.AIAttackSettings AIAttackSettings;
    [field: SerializeField] private EnemyTimeControlStateMachine.EnemyTimeControlSettings timeControlSettings;
    [field: SerializeField] private DamagedState.DamagedSettings damagedSettings;
    [field: SerializeField] private ParriedState.ParriedSettings parriedSettings;
    [field: SerializeField] private BlockState.BlockSettings blockSettings;
    [field: SerializeField] private AliveStateMachine.AliveSettings aliveSettings;
    [field: SerializeField] private DeadState.DeadSettings deadSettings;

    private Animator animator;
    private EnemyPerceptionSystem perceptionSystem;
    private RootStateMachine rootStateMachine;
    private RewindableTransform rewindableTransform;

    private void Awake() {
        animator = GetComponent<Animator>();
        rewindableTransform = new RewindableTransform(transform);
#if UNITY_EDITOR
        rewindableTransform.Name = "EnemyTransform" + gameObject.name;
#endif

        characterMovement.CharacterController = GetComponent<CharacterController>();
        characterMovement.Transform = rewindableTransform;
        characterMovement.Init();

        perceptionSystem = GetComponent<EnemyPerceptionSystem>();
        
        enemyAI.Init();
        health.Init();
        sword.OnEquipped(this.gameObject);

        SubscribeEvents();
        InjectDependencies();
        BuildHFSM();
        rootStateMachine.Init();
        
    }

    private void Start() {
    }

    private void InjectDependencies() {
        idleSettings.CharacterMovement = characterMovement;
        idleSettings.Animator = animator;
        idleSettings.Sword = sword;

        approachPlayerSettings.CharacterMovement = characterMovement;
        approachPlayerSettings.Transform = transform;
        approachPlayerSettings.Animator = animator;

        AIAttackSettings.Animator = animator;
        AIAttackSettings.Transform = transform;
        AIAttackSettings.CharacterMovement = characterMovement;
        AIAttackSettings.Sword = sword;
        AIAttackSettings.Hurtbox = hurtbox;

        timeControlSettings.Transform = transform;
        timeControlSettings.Animator = animator;
        timeControlSettings.CharacterMovement = characterMovement;
        timeControlSettings.Health = health;
        timeControlSettings.Hurtbox = hurtbox;
        timeControlSettings.Sword = sword;
        timeControlSettings.EnemyAI = enemyAI;

        parriedSettings.Animator = animator;

        blockSettings.Animator = animator;
        blockSettings.Hurtbox = hurtbox;

        damagedSettings.Animator = animator;

        deadSettings.Animator = animator;
    }

    private void SubscribeEvents() {
        hurtbox.DamageReceived += health.OnDamageReceived;
        hurtbox.DamageReceived += enemyAI.OnDamageReceived;
    }

    private void BuildHFSM() {
        // Create states and state machines
        IdleState idleState = new IdleState(idleSettings);
        ApproachPlayerState approachPlayerState = new ApproachPlayerState(approachPlayerSettings);
        AIAttackState AIAttackState = new AIAttackState(AIAttackSettings);
        DamagedState damagedState = new DamagedState(damagedSettings);
        ParriedState parriedState = new ParriedState(parriedSettings);
        BlockState blockState = new BlockState(blockSettings);
        AliveStateMachine aliveStateMachine = new AliveStateMachine(aliveSettings, idleState, approachPlayerState, damagedState,
                                                                    blockState, parriedState, AIAttackState);
        DeadState deadState = new DeadState(deadSettings);
        EnemyTimeControlStateMachine enemyTimeControlStateMachine = new EnemyTimeControlStateMachine(UpdateMode.UpdateAfterChild, timeControlSettings,
                                                                                                     aliveStateMachine, deadState);
        rootStateMachine = new RootStateMachine(enemyTimeControlStateMachine);

        // Create transitions
        // Idle ->
        hurtbox.DamageReceived += idleState.AddEventTransition<float>(damagedState);
        idleState.AddTransition(approachPlayerState, HasDetectedPlayer, ()=> !approachPlayerState.ReachedPlayer());
        idleState.AddTransition(AIAttackState, SetPlayerAsAttackTarget, CanAttackPlayer);

        // ApproachPlayer ->
        hurtbox.DamageReceived += approachPlayerState.AddEventTransition<float>(damagedState);
        approachPlayerState.AddTransition(idleState, approachPlayerState.ReachedPlayer);
        approachPlayerState.AddTransition(AIAttackState, SetPlayerAsAttackTarget, CanAttackPlayer);

        // AIAttack ->
        AnimatorUtils.AnimationEnded += AIAttackState.AddEventTransition<int>(idleState, AIAttackEnded);
        hurtbox.DamageReceived += AIAttackState.AddEventTransition<float>(damagedState);
        AIAttackState.Parried += AIAttackState.AddEventTransition(parriedState);

        // Damaged ->
        AnimatorUtils.AnimationEnded += damagedState.AddEventTransition<int>(idleState, DamagedAnimationEnded);
        hurtbox.DamageReceived += damagedState.AddEventTransition<float>(damagedState);
        damagedState.AddTransition(blockState, enemyAI.ResetReceivedTooMuchDamageRecently, () => enemyAI.ReceivedTooMuchDamageRecently.Value);

        // Parried ->
        AnimatorUtils.AnimationEnded += parriedState.AddEventTransition<int>(idleState, ParriedAnimationEnded);
        hurtbox.DamageReceived += parriedState.AddEventTransition<float>(damagedState);

        // Block ->
        hurtbox.DamageReceived += blockState.AddEventTransition<float>(damagedState);
        AnimatorUtils.AnimationEnded += blockState.AddEventTransition<int>(idleState, BlockAnimationEnded);

        // Alive ->
        health.Dead += aliveStateMachine.AddEventTransition(deadState);

    }

    private void Update(){
        rootStateMachine.Update();
        Debug.Log("Enemy state: " + rootStateMachine.GetCurrentStateName());
    }

    private void FixedUpdate() {
        rootStateMachine.FixedUpdate();
    }

    private void LateUpdate() {
        rootStateMachine.LateUpdate();
    }

    #region Transition conditions
    private bool DamagedAnimationEnded(int shortNameHash) {
        return AnimatorUtils.damagedHash == shortNameHash;
    }
    
    private bool ParriedAnimationEnded(int shortNameHash) {
        return AnimatorUtils.parriedHash == shortNameHash;
    }

    private bool BlockAnimationEnded(int shortNameHash) {
        return AnimatorUtils.blockHash == shortNameHash;
    }

    private bool AIAttackEnded(int shortNameHash) {
        return AnimatorUtils.attackRecoveryHash == shortNameHash;
    }

    private bool AttackCooldownFinished() {
        return sword.CooldownFinished();
    }

    private bool CanAttackPlayer() {
        return HasDetectedPlayer()  && perceptionSystem.IsPlayerInAttackRange() && AttackCooldownFinished();
    }

    private bool HasDetectedPlayer() {
        return perceptionSystem.IsSeeingPlayer() || enemyAI.HasBeenAttacked.Value;
    }

    #endregion

    #region Transition actions
    private void SetPlayerAsAttackTarget() {
        AIAttackSettings.Target = perceptionSystem.player;
    }

    #endregion

    #region Animation events
    public void SetHitboxEnabled(Bool enabled) {
        sword.SetHitboxEnabled(enabled);
    }

    public void SetIsShielded(Bool enabled) {
        hurtbox.IsShielded = Convert.ToBoolean((int)enabled);
    }

    public void SetHurtboxInvincible(Bool invincible) {
        hurtbox.IsInvincible = Convert.ToBoolean((int)invincible);
    }

    public void OnAnimatorMove() {
        if (!TimeRewindController.Instance.IsRewinding && animator.applyRootMotion) {
            animator.ApplyBuiltinRootMotion();
            rewindableTransform.IsModified = true;
        }
    }

    #endregion

}