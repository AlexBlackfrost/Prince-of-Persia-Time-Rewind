using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour{
    [field: SerializeField] private CharacterMovement characterMovement;

    [Header("State machine settings")]
    [field: SerializeField] private IdleState.IdleSettings idleSettings;
    [field: SerializeField] private ApproachPlayerState.ApproachPlayerSettings approachPlayerSettings;

    private Animator animator;
    private Sword sword;
    private Dictionary<Type, StateObject> stateObjects;
    private EnemyPerceptionSystem perceptionSystem;
    private RootStateMachine rootStateMachine;

    private void Awake() {
        characterMovement.Transform = transform;
        characterMovement.CharacterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        sword = GetComponent<Sword>();
        perceptionSystem = GetComponent<EnemyPerceptionSystem>();
        stateObjects = new Dictionary<Type, StateObject>();

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
    }

    private void BuildHFSM() {
        // Create states and state machines
        IdleState idleState = new IdleState(idleSettings);
        ApproachPlayerState approachPlayerState = new ApproachPlayerState(approachPlayerSettings);
        rootStateMachine = new RootStateMachine(idleState, approachPlayerState);

        // Create transitions
        idleState.AddTransition(approachPlayerState, perceptionSystem.IsSeeingPlayer);
        approachPlayerState.AddTransition(idleState, approachPlayerState.ReachedPlayer);

        stateObjects[typeof(IdleState)] = idleState;
        stateObjects[typeof(ApproachPlayerState)] = approachPlayerState;
    }

    private void Update(){
        rootStateMachine.Update();
    }

    private void FixedUpdate() {
        rootStateMachine.FixedUpdate();
    }
}