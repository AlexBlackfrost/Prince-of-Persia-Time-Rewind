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

    [Header("State machine settings")]
    [field: SerializeField] private IdleState.IdleSettings idleSettings;
    [field: SerializeField] private ApproachPlayerState.ApproachPlayerSettings approachPlayerSettings;
    [field: SerializeField] private EnemyTimeControlStateMachine.EnemyTimeControlSettings timeControlSettings;
    [field: SerializeField] private DamagedState.DamagedSettings damagedSettings;
    [field: SerializeField] private ParriedState.ParriedSettings parriedSettings;
    [field: SerializeField] private BlockState.BlockSettings blockSettings;
    [field: SerializeField] private AliveStateMachine.AliveSettings aliveSettings;
    [field: SerializeField] private DeadState.DeadSettings deadSettings;

    private Animator animator;
    private Dictionary<Type, StateObject> stateObjects;
    private EnemyPerceptionSystem perceptionSystem;
    private RootStateMachine rootStateMachine;

    private void Awake() {
        characterMovement.Transform = transform;
        characterMovement.CharacterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        perceptionSystem = GetComponent<EnemyPerceptionSystem>();
        stateObjects = new Dictionary<Type, StateObject>();

        health.Init();
        sword.OnEquipped(this.gameObject);

        SubscribeEvents();

        InjectDependencies();
        BuildHFSM();
        rootStateMachine.Init();
        
    }

    private void InjectDependencies() {
        idleSettings.CharacterMovement = characterMovement;
        idleSettings.Animator = animator;
        idleSettings.Sword = sword;

        approachPlayerSettings.CharacterMovement = characterMovement;
        approachPlayerSettings.Transform = transform;
        approachPlayerSettings.Animator = animator;

        timeControlSettings.Transform = transform;
        timeControlSettings.Animator = animator;
        timeControlSettings.StateObjects = stateObjects;
        timeControlSettings.CharacterMovement = characterMovement;
        timeControlSettings.Health = health;
        timeControlSettings.Sword = sword;

        parriedSettings.Animator = animator;
        blockSettings.Animator = animator;

        damagedSettings.Animator = animator;

        deadSettings.Animator = animator;
    }

    private void SubscribeEvents() {
        hurtbox.DamageReceived += health.OnDamageReceived;
    }

    private void BuildHFSM() {
        // Create states and state machines
        IdleState idleState = new IdleState(idleSettings);
        ApproachPlayerState approachPlayerState = new ApproachPlayerState(approachPlayerSettings);
        DamagedState damagedState = new DamagedState(damagedSettings);
        ParriedState parriedState = new ParriedState(parriedSettings);
        BlockState blockState = new BlockState(blockSettings);
        AliveStateMachine aliveStateMachine = new AliveStateMachine(aliveSettings, idleState, approachPlayerState, damagedState,
                                                                    blockState, parriedState);
        DeadState deadState = new DeadState(deadSettings);
        EnemyTimeControlStateMachine enemyTimeControlStateMachine = new EnemyTimeControlStateMachine(UpdateMode.UpdateAfterChild, timeControlSettings,
                                                                                                     aliveStateMachine, deadState);
        rootStateMachine = new RootStateMachine(enemyTimeControlStateMachine);

        // Create transitions
        // Idle ->
        hurtbox.DamageReceived += idleState.AddEventTransition<float>(damagedState);
        idleState.AddTransition(approachPlayerState, perceptionSystem.IsSeeingPlayer, ()=> !approachPlayerState.ReachedPlayer());

        // ApproachPlayer ->
        hurtbox.DamageReceived += approachPlayerState.AddEventTransition<float>(damagedState);
        approachPlayerState.AddTransition(idleState, approachPlayerState.ReachedPlayer);

        // Damaged ->
        AnimatorUtils.AnimationEnded += damagedState.AddEventTransition<int>(idleState, DamagedAnimationEnded);
        hurtbox.DamageReceived += damagedState.AddEventTransition<float>(damagedState);

        // Alive ->
        health.Dead += aliveStateMachine.AddEventTransition(deadState);

        stateObjects[typeof(IdleState)] = idleState;
        stateObjects[typeof(ApproachPlayerState)] = approachPlayerState;
        stateObjects[typeof(ApproachPlayerState)] = approachPlayerState;
        stateObjects[typeof(DamagedState)] = damagedState;
        stateObjects[typeof(BlockState)] = blockState;
        stateObjects[typeof(ParriedState)] = parriedState;
        stateObjects[typeof(AliveStateMachine)] = aliveStateMachine;
        stateObjects[typeof(DeadState)] = deadState;
        stateObjects[typeof(EnemyTimeControlStateMachine)] = enemyTimeControlStateMachine;
    }

    private void Update(){
        rootStateMachine.Update();
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

    #endregion

}