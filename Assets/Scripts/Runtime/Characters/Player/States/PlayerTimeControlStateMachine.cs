using Cinemachine;
using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class PlayerTimeControlStateMachine : StateMachine {
	[Serializable]
	public class PlayerTimeControlSettings {
		[field: SerializeField] public CinemachineFreeLook FreeLookCamera { get; set; }
		public CinemachineVirtualCamera timeRewindCamera;
		public Camera Camera { get; set; }
		public InputController InputController { get; set; }
		public Transform Transform { get; set; }
		public CharacterMovement CharacterMovement { get; set; }
		public Animator Animator { get; set; }
		public Sword Sword { get; set; }
		public Health Health { get; set; }
		public Hurtbox Hurtbox { get; set; }

		//[field:SerializeField] public int MaxFPS { get; private set; } = 144;
	}

	private PlayerTimeControlSettings settings;
	private AnimationTimeControl animationTimeControl;
	private StateMachineTimeControl stateMachineTimeControl;
	private TransformTimeControl transformTimeControl;
	private CameraTimeControl cameraTimeControl;
	private CharacterMovementTimeControl characterMovementTimeControl;
	private HealthTimeControl healthTimeControl;
	private HurtboxTimeControl hurtboxTimeControl;
	private bool timeIsRewinding;
	private float elapsedTimeSinceLastRecord;
	private AnimationRecord previousRecord, nextRecord;
	private  CircularStack<AnimationRecord> animationRecords;
	private int recordFPS = 60;
	private int recordMaxseconds = 20;
	private CinemachineBrain cinemachineBrain;
	private NoneState noneState;

	private System.Diagnostics.Stopwatch stopwatch;

	public PlayerTimeControlStateMachine(UpdateMode updateMode, PlayerTimeControlSettings settings, params StateObject[] states) : base(updateMode, states) {
		//Application.targetFrameRate = settings.MaxFPS;
		this.settings = settings;

		noneState = new NoneState();
		animationTimeControl = new AnimationTimeControl(settings.Animator);
		//transformTimeControl = new TransformTimeControl(settings.Transform);
		//cameraTimeControl = new CameraTimeControl(settings.Camera, settings.timeRewindCamera, settings.FreeLookCamera);
		//stateMachineTimeControl = new StateMachineTimeControl(this);
		//characterMovementTimeControl = new CharacterMovementTimeControl(settings.CharacterMovement);
		//healthTimeControl = new HealthTimeControl(settings.Health);
		//hurtboxTimeControl = new HurtboxTimeControl(settings.Hurtbox);

		cinemachineBrain = settings.Camera.GetComponent<CinemachineBrain>();

		animationRecords = new CircularStack<AnimationRecord>(recordFPS * recordMaxseconds);
		timeIsRewinding = false;
		TimeRewindController.Instance.TimeRewindStart += OnTimeRewindStart;
		TimeRewindController.Instance.TimeRewindStop += OnTimeRewindStop;

		stopwatch = new System.Diagnostics.Stopwatch();
	}
	 
    protected override void OnLateUpdate() {
		bool timeRewindPressed = settings.InputController.IsTimeRewindPressed();
		if (timeRewindPressed && !timeIsRewinding) {
			TimeRewindController.Instance.StartTimeRewind();
			timeIsRewinding = timeRewindPressed;
		} else if (!timeRewindPressed && timeIsRewinding) {
			TimeRewindController.Instance.StopTimeRewind();
			timeIsRewinding = timeRewindPressed;
		}

		if (timeIsRewinding) {
			stopwatch.Restart();
			TimeRewindController.Instance.Rewind();
			RewindAnimationRecord();
			stopwatch.Stop();
			Stats.AddAccumulatedRewindTime(stopwatch.Elapsed.TotalMilliseconds);
			cinemachineBrain.ManualUpdate();
		} else {
			cinemachineBrain.ManualUpdate();
			stopwatch.Restart();
			SaveAnimationRecord();
			stopwatch.Stop();
			TimeRewindController.Instance.RecordVariables();
			Stats.AddAccumulatedRecordTime(stopwatch.Elapsed.TotalMilliseconds);
		}
		timeIsRewinding = timeRewindPressed;
        

	}

	private void OnTimeRewindStart() {
		elapsedTimeSinceLastRecord = 0;
		previousRecord = animationRecords.Pop();
		nextRecord = animationRecords.Peek();

		// Animation
		animationTimeControl.OnTimeRewindStart();

		// Camera
		//cameraTimeControl.OnTimeRewindStart();

		// State machine
		//stateMachineTimeControl.OnTimeRewindStart();
		CurrentStateObject.Value.Exit();
		// Do not change state using ChangeState() so that OnStateEnter is not triggered after rewind stops.
		CurrentStateObject.Value = noneState;

		// Sword
		//settings.Sword.OnTimeRewindStart();
	}

	private void OnTimeRewindStop() {
		// Animation
		animationTimeControl.OnTimeRewindStop(previousRecord, nextRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		// Camera
		//cameraTimeControl.OnTimeRewindStop();

		// State machine
		//stateMachineTimeControl.RestoreStateMachineRecord(previousRecord.stateMachineRecord);

		// Transform
		//transformTimeControl.OnTimeRewindStop(previousRecord.playerTransform, nextRecord.playerTransform, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		// Character movement
		/*characterMovementTimeControl.OnTimeRewindStop(previousRecord.characterMovementRecord, nextRecord.characterMovementRecord, 
													  previousRecord.deltaTime, elapsedTimeSinceLastRecord);*/
		// Sword
		//settings.Sword.OnTimeRewindStop(previousRecord.swordRecord, nextRecord.swordRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		// Health
		//healthTimeControl.OnTimeRewindStop(previousRecord.healthRecord, nextRecord.healthRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);
		
		// Hurtbox
		//hurtboxTimeControl.OnTimeRewindStop(previousRecord.hurtboxRecord, nextRecord.hurtboxRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);
	}

    private void SaveAnimationRecord() {
		AnimationRecord animationRecord = animationTimeControl.RecordAnimationData();

		animationRecords.Push(animationRecord);
	}


	private void RewindAnimationRecord() {
		while (elapsedTimeSinceLastRecord > previousRecord.deltaTime && animationRecords.Count > 2 + 2 * TimeRewindController.Instance.MaxMaxFramesWithoutBeingRecorded) {
			elapsedTimeSinceLastRecord -= previousRecord.deltaTime;
			previousRecord = animationRecords.Pop();
			nextRecord = animationRecords.Peek(); 
		}
		
		RestoreAnimationRecord(previousRecord, nextRecord);
		elapsedTimeSinceLastRecord += Time.deltaTime * TimeRewindController.Instance.RewindSpeed;
	}


	private void RestoreAnimationRecord(AnimationRecord previousRecord, AnimationRecord nextRecord) {
		/*transformTimeControl.RestoreTransformRecord(previousRecord.playerTransform, nextRecord.playerTransform, previousRecord.deltaTime, 
													elapsedTimeSinceLastRecord);*/

		/*cameraTimeControl.RestoreCameraRecord(previousRecord.cameraRecord, nextRecord.cameraRecord, previousRecord.deltaTime, 
											  elapsedTimeSinceLastRecord);*/

		animationTimeControl.RestoreAnimationRecord(previousRecord, nextRecord, previousRecord.deltaTime, 
													elapsedTimeSinceLastRecord);
		//animationTimeControl.RestoreRewindableAnimatorFloatParameters();
		/*animationTimeControl.RestoreAnimatorFloatParameters(previousRecord.animationRecord, nextRecord.animationRecord, previousRecord.deltaTime,
													elapsedTimeSinceLastRecord);*/

		/*characterMovementTimeControl.RestoreCharacterMovementRecord(previousRecord.characterMovementRecord, nextRecord.characterMovementRecord, 
																	previousRecord.deltaTime, elapsedTimeSinceLastRecord);*/

		/*healthTimeControl.RestoreHealthRecord(previousRecord.healthRecord, nextRecord.healthRecord, previousRecord.deltaTime, 
											  elapsedTimeSinceLastRecord);*/

		/*hurtboxTimeControl.RestoreHurtboxRecord(previousRecord.hurtboxRecord, nextRecord.hurtboxRecord, previousRecord.deltaTime,
												elapsedTimeSinceLastRecord);*/

		//settings.Sword.RestoreSwordRecord(previousRecord.swordRecord, nextRecord.swordRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);


		//Debug.Log("Rewinding... " + nextRecord.stateMachineRecord.stateObjectRecords[0].stateObject.ToString());
	}

	public override object RecordFieldsAndProperties() {
		return null;
	}

	public override void RestoreFieldsAndProperties(object fieldsAndProperties) { }
}