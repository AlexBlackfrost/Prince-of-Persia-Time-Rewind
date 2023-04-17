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
		//[field:SerializeField] public int MaxFPS { get; private set; } = 144;
	}

	private EnemyTimeControlSettings settings;
	private bool timeIsRewinding;
	private float elapsedTimeSinceLastRecord;
	private EnemyRecord previousRecord, nextRecord;
	private CircularStack<EnemyRecord> records;
	private int recordFPS = 60;
	private int recordMaxseconds = 20;
	private float rewindSpeed = 0.1f;
	private NoneState noneState;

	private AnimationRecord lastAnimationRecord;
	private TransitionRecord[] lastInterruptedTransitionRecordInLayer;
	private AnimationTimeControl animationTimeControl;
	private StateMachineTimeControl stateMachineTimeControl;
	//private TransformTimeControl transformTimeControl;

	public EnemyTimeControlStateMachine(UpdateMode updateMode, EnemyTimeControlSettings settings, params StateObject[] states) : base(updateMode, states) {
		//Application.targetFrameRate = settings.MaxFPS;
		this.settings = settings;
		noneState = new NoneState();
		timeIsRewinding = false;
		animationTimeControl = new AnimationTimeControl(settings.Animator);
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
		CurrentStateObject.Exit();
		// Do not change state using ChangeState() so that OnStateEnter is not triggered after rewind stops.
		CurrentStateObject = noneState;

		// Sword
		settings.Sword.OnTimeRewindStart();
	}

	private void OnTimeRewindStop() {
		timeIsRewinding = false;

		// Animation
		animationTimeControl.OnTimeRewindStop(previousRecord.animationRecord, nextRecord.animationRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		// State machine
		stateMachineTimeControl.RestoreStateMachineRecord(settings.StateObjects, previousRecord.stateMachineRecord);

		// Sword
		settings.Sword.OnTimeRewindStop(previousRecord.swordRecord, nextRecord.swordRecord, elapsedTimeSinceLastRecord, previousRecord.deltaTime);
	}

	private void RewindEnemyRecord() {

    }

	private void SaveEnemyRecord() {

    }
}