using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour{
    [field: SerializeField] private CharacterMovement characterMovement;

    [Header("Combat")]
    [SerializeField] private DamageController damageController;
    [SerializeField] private Health health;
    [SerializeField] private Sword sword;

    [Header("State machine settings")]
    [field: SerializeField] private IdleState.IdleSettings idleSettings;
    [field: SerializeField] private ApproachPlayerState.ApproachPlayerSettings approachPlayerSettings;
    [field: SerializeField] private EnemyTimeControlStateMachine.EnemyTimeControlSettings timeControlSettings;
    [field: SerializeField] private DamagedState.DamagedSettings damagedSettings;

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
        timeControlSettings.Sword = sword;

        damagedSettings.Animator = animator;
    }

    private void SubscribeEvents() {
        damageController.DamageReceived += health.OnDamageReceived;
    }

    private void BuildHFSM() {
        // Create states and state machines
        IdleState idleState = new IdleState(idleSettings);
        ApproachPlayerState approachPlayerState = new ApproachPlayerState(approachPlayerSettings);
        DamagedState damagedState = new DamagedState(damagedSettings);
        EnemyTimeControlStateMachine enemyTimeControlStateMachine = new EnemyTimeControlStateMachine(UpdateMode.UpdateAfterChild, timeControlSettings,
                                                                                                     idleState, approachPlayerState, damagedState);
        rootStateMachine = new RootStateMachine(enemyTimeControlStateMachine);

        // Create transitions
        // Idle ->
        idleState.AddTransition(approachPlayerState, perceptionSystem.IsSeeingPlayer);
        damageController.DamageReceived += idleState.AddEventTransition<float>(damagedState);

        // ApproachPlayer ->
        approachPlayerState.AddTransition(idleState, approachPlayerState.ReachedPlayer);
        damageController.DamageReceived += approachPlayerState.AddEventTransition<float>(damagedState);

        // Damaged ->
        AnimatorUtils.AnimationEnded += damagedState.AddEventTransition<int>(idleState, DamagedAnimationEnded);


        stateObjects[typeof(IdleState)] = idleState;
        stateObjects[typeof(ApproachPlayerState)] = approachPlayerState;
        stateObjects[typeof(ApproachPlayerState)] = approachPlayerState;
        stateObjects[typeof(DamagedState)] = damagedState;
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
        return Animator.StringToHash("Damaged") == shortNameHash;
    }
    #endregion

}