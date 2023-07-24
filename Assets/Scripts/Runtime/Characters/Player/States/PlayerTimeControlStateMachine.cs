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
	private PlayerRecord previousRecord, nextRecord;
	private  CircularStack<PlayerRecord> records;
	private int recordFPS = 60;
	private int recordMaxseconds = 20;
	private CinemachineBrain cinemachineBrain;

	public PlayerTimeControlStateMachine(UpdateMode updateMode, PlayerTimeControlSettings settings, params StateObject[] states) : base(updateMode, states) {
		//Application.targetFrameRate = settings.MaxFPS;
		this.settings = settings;

		animationTimeControl = new AnimationTimeControl(settings.Animator);
		transformTimeControl = new TransformTimeControl(settings.Transform);
		cameraTimeControl = new CameraTimeControl(settings.Camera, settings.timeRewindCamera, settings.FreeLookCamera);
		stateMachineTimeControl = new StateMachineTimeControl(this);
		characterMovementTimeControl = new CharacterMovementTimeControl(settings.CharacterMovement);
		healthTimeControl = new HealthTimeControl(settings.Health);
		hurtboxTimeControl = new HurtboxTimeControl(settings.Hurtbox);

		cinemachineBrain = settings.Camera.GetComponent<CinemachineBrain>();

		records = new CircularStack<PlayerRecord>(recordFPS * recordMaxseconds);
		timeIsRewinding = false;
		TimeRewindManager.TimeRewindStart += OnTimeRewindStart;
		TimeRewindManager.TimeRewindStop += OnTimeRewindStop;
	}
	 
    protected override void OnLateUpdate() {
		bool timeRewindPressed = settings.InputController.IsTimeRewindPressed();
		if (timeRewindPressed && !timeIsRewinding) {
			TimeRewindManager.StartTimeRewind();
		} else if (!timeRewindPressed && timeIsRewinding) {
			TimeRewindManager.StopTimeRewind();
        }

		timeIsRewinding = timeRewindPressed;
        if (timeIsRewinding) {
			RewindPlayerRecord();
			RewindController.Instance.Rewind(Time.deltaTime);
			cinemachineBrain.ManualUpdate();
		} else {
			cinemachineBrain.ManualUpdate();
			SavePlayerRecord();
			RewindController.Instance.RecordVariables();
		}
	}

	private void OnTimeRewindStart() {
		elapsedTimeSinceLastRecord = 0;
		previousRecord = records.Pop();
		nextRecord = records.Peek();

		// Animation
		animationTimeControl.OnTimeRewindStart();

		// Camera
		//cameraTimeControl.OnTimeRewindStart();

		// State machine
		stateMachineTimeControl.OnTimeRewindStart();

		// Sword
		settings.Sword.OnTimeRewindStart();
	}

	private void OnTimeRewindStop() {
		// Animation
		animationTimeControl.OnTimeRewindStop(previousRecord.animationRecord, nextRecord.animationRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

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
		settings.Sword.OnTimeRewindStop(previousRecord.swordRecord, nextRecord.swordRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		// Health
		healthTimeControl.OnTimeRewindStop(previousRecord.healthRecord, nextRecord.healthRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);
		
		// Hurtbox
		hurtboxTimeControl.OnTimeRewindStop(previousRecord.hurtboxRecord, nextRecord.hurtboxRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);
	}

    private void SavePlayerRecord() {
		PlayerRecord playerRecord = new PlayerRecord(default(TransformRecord),
													 default(CameraRecord),
													 animationTimeControl.RecordAnimationData(),
													 default(StateMachineRecord),
													 default(CharacterMovementRecord),
													 settings.Sword.RecordSwordData(),
													 healthTimeControl.RecordHealthData(),
													 hurtboxTimeControl.RecordHurtboxData(),
													 Time.deltaTime);

		// Check for interrupted transitions -- Now it's done inside animationTimeControl
		//animationTimeControl.TrackInterruptedTransitions(ref playerRecord.animationRecord, playerRecord.deltaTime);

		records.Push(playerRecord);
	}


	private void RewindPlayerRecord() {
		while (elapsedTimeSinceLastRecord > previousRecord.deltaTime && records.Count > 2 + 2 * RewindController.Instance.MaxMaxFramesWithoutBeingRecorded) {
			elapsedTimeSinceLastRecord -= previousRecord.deltaTime;
			previousRecord = records.Pop();
			nextRecord = records.Peek(); 
		}
		
		RestorePlayerRecord(previousRecord, nextRecord);
		elapsedTimeSinceLastRecord += Time.deltaTime * TimeRewindManager.RewindSpeed;
	}


	private void RestorePlayerRecord(PlayerRecord previousRecord, PlayerRecord nextRecord) {
		/*transformTimeControl.RestoreTransformRecord(previousRecord.playerTransform, nextRecord.playerTransform, previousRecord.deltaTime, 
													elapsedTimeSinceLastRecord);*/

		/*cameraTimeControl.RestoreCameraRecord(previousRecord.cameraRecord, nextRecord.cameraRecord, previousRecord.deltaTime, 
											  elapsedTimeSinceLastRecord);*/

		animationTimeControl.RestoreAnimationRecord(previousRecord.animationRecord, nextRecord.animationRecord, previousRecord.deltaTime, 
													elapsedTimeSinceLastRecord);
		animationTimeControl.RestoreRewindableAnimatorFloatParameters();
		/*animationTimeControl.RestoreAnimatorFloatParameters(previousRecord.animationRecord, nextRecord.animationRecord, previousRecord.deltaTime,
													elapsedTimeSinceLastRecord);*/

		/*characterMovementTimeControl.RestoreCharacterMovementRecord(previousRecord.characterMovementRecord, nextRecord.characterMovementRecord, 
																	previousRecord.deltaTime, elapsedTimeSinceLastRecord);*/

		healthTimeControl.RestoreHealthRecord(previousRecord.healthRecord, nextRecord.healthRecord, previousRecord.deltaTime, 
											  elapsedTimeSinceLastRecord);

		hurtboxTimeControl.RestoreHurtboxRecord(previousRecord.hurtboxRecord, nextRecord.hurtboxRecord, previousRecord.deltaTime,
												elapsedTimeSinceLastRecord);

		settings.Sword.RestoreSwordRecord(previousRecord.swordRecord, nextRecord.swordRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);


		//Debug.Log("Rewinding... " + nextRecord.stateMachineRecord.stateObjectRecords[0].stateObject.ToString());
	}

	public override object RecordFieldsAndProperties() {
		return null;
	}

	public override void RestoreFieldsAndProperties(object fieldsAndProperties) { }
}