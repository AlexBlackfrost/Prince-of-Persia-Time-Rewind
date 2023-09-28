using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour{
    [field: SerializeField] public CharacterMovement CharacterMovement { get; set; }

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

    private void Awake() {
        CharacterMovement.Transform = transform;
        CharacterMovement.CharacterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        perceptionSystem = GetComponent<EnemyPerceptionSystem>();
        
        enemyAI.Init();
        health.Init();

        SubscribeEvents();
        InjectDependencies();
        BuildHFSM();
        rootStateMachine.Init();
        
    }

    private void Start() {
        sword.OnEquipped(this.gameObject);
        
    }

    private void InjectDependencies() {
        idleSettings.CharacterMovement = CharacterMovement;
        idleSettings.Animator = animator;
        idleSettings.Sword = sword;

        approachPlayerSettings.CharacterMovement = CharacterMovement;
        approachPlayerSettings.Transform = transform;
        approachPlayerSettings.Animator = animator;

        AIAttackSettings.Animator = animator;
        AIAttackSettings.Transform = transform;
        AIAttackSettings.CharacterMovement = CharacterMovement;
        AIAttackSettings.Sword = sword;
        AIAttackSettings.Hurtbox = hurtbox;

        timeControlSettings.Transform = transform;
        timeControlSettings.Animator = animator;
        timeControlSettings.CharacterMovement = CharacterMovement;
        timeControlSettings.Health = health;
        timeControlSettings.Hurtbox = hurtbox;
        timeControlSettings.Sword = sword;
        timeControlSettings.EnemyAI = enemyAI;

        parriedSettings.Animator = animator;

        blockSettings.Animator = animator;
        blockSettings.Hurtbox = hurtbox;

        damagedSettings.Animator = animator;
        damagedSettings.CharacterMovement = CharacterMovement;

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
        hurtbox.DamageReceived += idleState.AddEventTransition<float, IDamageSource>(damagedState, ApplyDamageEffect);
        idleState.AddTransition(approachPlayerState, HasDetectedPlayer, ()=> !approachPlayerState.ReachedPlayer());
        idleState.AddTransition(AIAttackState, SetPlayerAsAttackTarget, CanAttackPlayer);

        // ApproachPlayer ->
        hurtbox.DamageReceived += approachPlayerState.AddEventTransition<float, IDamageSource>(damagedState, ApplyDamageEffect);
        approachPlayerState.AddTransition(idleState, approachPlayerState.ReachedPlayer);
        approachPlayerState.AddTransition(AIAttackState, SetPlayerAsAttackTarget, CanAttackPlayer);

        // AIAttack ->
        AnimatorUtils.AnimationEnded += AIAttackState.AddEventTransition<int>(idleState, AIAttackEnded);
        hurtbox.DamageReceived += AIAttackState.AddEventTransition<float, IDamageSource>(damagedState, ApplyDamageEffect);
        AIAttackState.Parried += AIAttackState.AddEventTransition(parriedState);

        // Damaged ->
        AnimatorUtils.AnimationEnded += damagedState.AddEventTransition<int>(idleState, DamagedAnimationEnded);
        hurtbox.DamageReceived += damagedState.AddEventTransition<float, IDamageSource>(damagedState, ApplyDamageEffect);
        damagedState.AddTransition(blockState, enemyAI.ResetReceivedTooMuchDamageRecently, () => enemyAI.ReceivedTooMuchDamageRecently);

        // Parried ->
        AnimatorUtils.AnimationEnded += parriedState.AddEventTransition<int>(idleState, ParriedAnimationEnded);
        hurtbox.DamageReceived += parriedState.AddEventTransition<float, IDamageSource>(damagedState, ApplyDamageEffect);

        // Block ->
        hurtbox.DamageReceived += blockState.AddEventTransition<float, IDamageSource>(damagedState, ApplyDamageEffect);
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
        return perceptionSystem.IsSeeingPlayer() || enemyAI.HasBeenAttacked;
    }

    #endregion

    #region Transition actions
    private void SetPlayerAsAttackTarget() {
        AIAttackSettings.Target = perceptionSystem.player;
    }
    private void ApplyDamageEffect(float damageAmount, IDamageSource damageSource) {
        damageSource.ApplyDamageEffect(this.gameObject);
    }
    #endregion

    #region Animation events
    public void SetHitboxEnabled(Bool enabled) {
        sword.SetHitboxEnabled(enabled);
       
        /*
        int AIAttackLayerIndex = 0;
        // Don't enable hitbox when the animation event is fired during an animation transitions whose source state is AIAttack.
        if (animator.IsInTransition(AIAttackLayerIndex)) {
            AnimatorStateInfo currentStateInfo = animator.GetCurrentAnimatorStateInfo(AIAttackLayerIndex);
            if((currentStateInfo.shortNameHash != AnimatorUtils.AIAttackHash) || enabled == Bool.False) {
                sword.SetHitboxEnabled(enabled);
            }
        } else {
            sword.SetHitboxEnabled(enabled);
        }*/
    }

    public void SetIsShielded(Bool enabled) {
        hurtbox.SetIsShielded(Convert.ToBoolean((int)enabled));
    }

    public void SetHurtboxInvincible(Bool invincible) {
        hurtbox.IsInvincible = Convert.ToBoolean((int)invincible);
    }

    #endregion

}