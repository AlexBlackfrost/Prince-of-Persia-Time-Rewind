using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTimeControlStateMachine : StateMachine {

	[Serializable]
	public class EnemyTimeControlSettings {
		public Transform Transform { get; set; }
		public CharacterMovement CharacterMovement { get; set; }
		public Animator Animator { get; set; }
		public Dictionary<Type, StateObject> StateObjects { get; set; }
		public Sword Sword { get; set; }
		public Health Health { get; set; }

		public Hurtbox Hurtbox { get; set; }
		//[field:SerializeField] public int MaxFPS { get; private set; } = 144;
	}

	private EnemyTimeControlSettings settings;
	private AnimationTimeControl animationTimeControl;
	private StateMachineTimeControl stateMachineTimeControl;
	private TransformTimeControl transformTimeControl;
	private CharacterMovementTimeControl characterMovementTimeControl;
	private HealthTimeControl healthTimeControl;
	private HurtboxTimeControl hurtboxTimeControl;

	private bool timeIsRewinding;
	private float elapsedTimeSinceLastRecord;
	private EnemyRecord previousRecord, nextRecord;
	private CircularStack<EnemyRecord> records;
	private int recordFPS = 60; 
	private int recordMaxseconds = 20;
	private float rewindSpeed = 0.1f;

	private AnimationRecord lastAnimationRecord;
	private TransitionRecord[] lastInterruptedTransitionRecordInLayer;

	public EnemyTimeControlStateMachine(UpdateMode updateMode, EnemyTimeControlSettings settings, params StateObject[] states) : base(updateMode, states) {
		//Application.targetFrameRate = settings.MaxFPS;
		this.settings = settings;

		animationTimeControl = new AnimationTimeControl(settings.Animator);
		transformTimeControl = new TransformTimeControl(settings.Transform);
		stateMachineTimeControl = new StateMachineTimeControl(this);
		characterMovementTimeControl = new CharacterMovementTimeControl(settings.CharacterMovement);
		healthTimeControl = new HealthTimeControl(settings.Health);
		hurtboxTimeControl = new HurtboxTimeControl(settings.Hurtbox);

		records = new CircularStack<EnemyRecord>(recordFPS * recordMaxseconds);
		timeIsRewinding = false;
		TimeRewindManager.TimeRewindStart += OnTimeRewindStart;
		TimeRewindManager.TimeRewindStop += OnTimeRewindStop;
	}

	protected override void OnLateUpdate() {
		if (timeIsRewinding) {
			RewindEnemyRecord();
		} else {
			SaveEnemyRecord();
		}
	}


	private void OnTimeRewindStart() {
		timeIsRewinding = true;

		elapsedTimeSinceLastRecord = 0;
		previousRecord = records.Pop();
		nextRecord = records.Peek();

		// Animation
		animationTimeControl.OnTimeRewindStart();

		// State machine
		stateMachineTimeControl.OnTimeRewindStart();

		// Sword
		settings.Sword.OnTimeRewindStart();
	}

	private void OnTimeRewindStop() {
		timeIsRewinding = false;

		// Animation
		animationTimeControl.OnTimeRewindStop(previousRecord.animationRecord, nextRecord.animationRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		// State machine
		stateMachineTimeControl.RestoreStateMachineRecord(settings.StateObjects, previousRecord.stateMachineRecord);

		// Transform
		transformTimeControl.OnTimeRewindStop(previousRecord.enemyTransform, nextRecord.enemyTransform, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		// Character movement
		characterMovementTimeControl.OnTimeRewindStop(previousRecord.characterMovementRecord, nextRecord.characterMovementRecord,
													  previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		// Sword
		settings.Sword.OnTimeRewindStop(previousRecord.swordRecord, nextRecord.swordRecord, elapsedTimeSinceLastRecord, previousRecord.deltaTime);

		// Health
		healthTimeControl.OnTimeRewindStop(previousRecord.healthRecord, nextRecord.healthRecord, elapsedTimeSinceLastRecord, previousRecord.deltaTime);
		
		// Hurtbox		
		hurtboxTimeControl.OnTimeRewindStop(previousRecord.hurtboxRecord, nextRecord.hurtboxRecord, elapsedTimeSinceLastRecord, previousRecord.deltaTime);
	}

	private void SaveEnemyRecord() {
		EnemyRecord enemyRecord = new EnemyRecord(transformTimeControl.RecordTransformData(),
													 animationTimeControl.RecordAnimationData(),
													 stateMachineTimeControl.RecordStateMachineData(),
													 characterMovementTimeControl.RecordCharacterMovementData(),
													 settings.Sword.RecordSwordData(),
													 healthTimeControl.RecordHealthData(),
													 hurtboxTimeControl.RecordHurtboxData(),
													 Time.deltaTime);

		// Check for interrupted transitions -- Now it's done inside animationTimeControl
		//animationTimeControl.TrackInterruptedTransitions(ref enemyRecord.animationRecord, enemyRecord.deltaTime);

		records.Push(enemyRecord);
	}


	private void RewindEnemyRecord() {
		while (elapsedTimeSinceLastRecord > previousRecord.deltaTime && records.Count > 2) {
			elapsedTimeSinceLastRecord -= previousRecord.deltaTime;
			previousRecord = records.Pop();
			nextRecord = records.Peek();
		}

		RestoreEnemyRecord(previousRecord, nextRecord);
		elapsedTimeSinceLastRecord += Time.deltaTime * rewindSpeed;
	}

	private void RestoreEnemyRecord(EnemyRecord previousRecord, EnemyRecord nextRecord) {
		transformTimeControl.RestoreTransformRecord(previousRecord.enemyTransform, nextRecord.enemyTransform, previousRecord.deltaTime,
													elapsedTimeSinceLastRecord);

		animationTimeControl.RestoreAnimationRecord(previousRecord.animationRecord, nextRecord.animationRecord, previousRecord.deltaTime,
													elapsedTimeSinceLastRecord);

		characterMovementTimeControl.RestoreCharacterMovementRecord(previousRecord.characterMovementRecord, nextRecord.characterMovementRecord,
																	previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		settings.Sword.RestoreSwordRecord(previousRecord.swordRecord, nextRecord.swordRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		healthTimeControl.RestoreHealthRecord(previousRecord.healthRecord, nextRecord.healthRecord, previousRecord.deltaTime, 
											  elapsedTimeSinceLastRecord);

		hurtboxTimeControl.RestoreHurtboxRecord(previousRecord.hurtboxRecord, nextRecord.hurtboxRecord, previousRecord.deltaTime,
												elapsedTimeSinceLastRecord);


		Debug.Log("Enemy Rewinding... " + nextRecord.stateMachineRecord.hierarchy[0].ToString());
	}
}