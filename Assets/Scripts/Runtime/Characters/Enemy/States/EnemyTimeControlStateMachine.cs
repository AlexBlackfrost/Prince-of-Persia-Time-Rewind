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

	public EnemyTimeControlStateMachine(UpdateMode updateMode, EnemyTimeControlSettings settings, params StateObject[] states) : base(updateMode, states) {
		//Application.targetFrameRate = settings.MaxFPS;
		this.settings = settings;
		noneState = new NoneState();
	}
}