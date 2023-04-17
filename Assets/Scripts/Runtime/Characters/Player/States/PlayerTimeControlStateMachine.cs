using Cinemachine;
using HFSM;
using System;
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
		public Dictionary<Type, StateObject> StateObjects { get; set; }
		public Sword Sword { get; set; }
		//[field:SerializeField] public int MaxFPS { get; private set; } = 144;
	}

	private PlayerTimeControlSettings settings;
	private AnimationTimeControl animationTimeControl;
	private StateMachineTimeControl stateMachineTimeControl;
	private TransformTimeControl transformTimeControl;
	private CameraTimeControl cameraTimeControl;
	private CharacterMovementTimeControl characterMovementTimeControl;
	private bool timeIsRewinding;
	private float elapsedTimeSinceLastRecord;
	private PlayerRecord previousRecord, nextRecord;
	private  CircularStack<PlayerRecord> records;
	private int recordFPS = 60;
	private int recordMaxseconds = 20;
	private float rewindSpeed = 0.1f;
	private NoneState noneState;
	private CinemachineBrain cinemachineBrain;

	public PlayerTimeControlStateMachine(UpdateMode updateMode, PlayerTimeControlSettings settings, params StateObject[] states) : base(updateMode, states) {
		//Application.targetFrameRate = settings.MaxFPS;
		this.settings = settings;
		noneState = new NoneState();

		animationTimeControl = new AnimationTimeControl(settings.Animator);
		transformTimeControl = new TransformTimeControl(settings.Transform);
		cameraTimeControl = new CameraTimeControl(settings.Camera, settings.timeRewindCamera);
		stateMachineTimeControl = new StateMachineTimeControl(this);
		characterMovementTimeControl = new CharacterMovementTimeControl(settings.CharacterMovement);

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
			cinemachineBrain.ManualUpdate();
		} else {
			cinemachineBrain.ManualUpdate();
			SavePlayerRecord();
        }
	}

	private void OnTimeRewindStart() {
		elapsedTimeSinceLastRecord = 0;
		previousRecord = records.Pop();
		nextRecord = records.Peek();

		// Animation
		animationTimeControl.OnTimeRewindStart();

		// Camera
		settings.timeRewindCamera.transform.position = settings.Camera.transform.position;
		settings.timeRewindCamera.transform.rotation = settings.Camera.transform.rotation;
		settings.timeRewindCamera.gameObject.SetActive(true);
		settings.FreeLookCamera.gameObject.SetActive(false);

		// State machine
		CurrentStateObject.Exit();
		// Do not change state using ChangeState() so that OnStateEnter is not triggered after rewind stops.
		CurrentStateObject = noneState;

		// Sword
		settings.Sword.OnTimeRewindStart();
	}

	private void OnTimeRewindStop() {
		// Animation
		animationTimeControl.OnTimeRewindStop(previousRecord.animationRecord, nextRecord.animationRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		// Camera
		settings.timeRewindCamera.gameObject.SetActive(false);
		settings.FreeLookCamera.gameObject.SetActive(true);

		// State machine
		// RestoreStateMachineRecord(settings.StateObjects, previousRecord.stateMachineRecord);
		stateMachineTimeControl.RestoreStateMachineRecord(settings.StateObjects, previousRecord.stateMachineRecord);

		// Sword
		settings.Sword.OnTimeRewindStop(previousRecord.swordRecord, nextRecord.swordRecord, elapsedTimeSinceLastRecord, previousRecord.deltaTime);
		
	}

    private void SavePlayerRecord() {
		PlayerRecord playerRecord = new PlayerRecord(transformTimeControl.RecordTransformData(),
													 cameraTimeControl.RecordCameraData(),
													 animationTimeControl.RecordAnimationData(),
													 stateMachineTimeControl.RecordStateMachineData(),
													 characterMovementTimeControl.RecordCharacterMovementData(),
													 settings.Sword.RecordSwordData(),
													 Time.deltaTime);

		/*PlayerRecord playerRecord = RecordUtils.RecordPlayerData(settings.Transform,
																 settings.Camera,
																 this,
																 settings.Animator,
																 settings.CharacterMovement,
																 settings.Sword.SaveSwordRecord());*/

		// Check for interrupted transitions
		animationTimeControl.TrackInterruptedTransitions(ref playerRecord.animationRecord, playerRecord.deltaTime);
		
		records.Push(playerRecord);
	}


	private void RewindPlayerRecord() {
		while (elapsedTimeSinceLastRecord > previousRecord.deltaTime && records.Count > 2) {
			elapsedTimeSinceLastRecord -= previousRecord.deltaTime;
			previousRecord = records.Pop();
			nextRecord = records.Peek(); 
		}
		
		RestorePlayerRecord(previousRecord, nextRecord);
		elapsedTimeSinceLastRecord += Time.deltaTime * rewindSpeed;
	}


	private void RestorePlayerRecord(PlayerRecord previousRecord, PlayerRecord nextRecord) {
		transformTimeControl.RestoreTransformRecord(previousRecord.playerTransform, nextRecord.playerTransform, previousRecord.deltaTime, 
													elapsedTimeSinceLastRecord);

		cameraTimeControl.RestoreCameraRecord(previousRecord.cameraRecord, nextRecord.cameraRecord, previousRecord.deltaTime, 
											  elapsedTimeSinceLastRecord);

		animationTimeControl.RestoreAnimationRecord(previousRecord.animationRecord, nextRecord.animationRecord, previousRecord.deltaTime, 
													elapsedTimeSinceLastRecord);

		characterMovementTimeControl.RestoreCharacterMovementRecord(previousRecord.characterMovementRecord, nextRecord.characterMovementRecord, 
																	previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		settings.Sword.RestoreSwordRecord(previousRecord.swordRecord, nextRecord.swordRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);


		Debug.Log("Rewinding... " + nextRecord.stateMachineRecord.hierarchy[0].ToString());
	}

	private void RestoreTransformRecord(Transform transform, TransformRecord previousTransformRecord,
										TransformRecord nextTransformRecord, float previousRecordDeltaTime) {

		float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;
        
		transform.position = Vector3.Lerp(previousTransformRecord.position, nextTransformRecord.position, lerpAlpha);
		transform.rotation = Quaternion.Slerp(previousTransformRecord.rotation, nextTransformRecord.rotation, lerpAlpha);
		transform.localScale = Vector3.Lerp(previousTransformRecord.localScale, nextTransformRecord.localScale, lerpAlpha);
        
	} 

	private void RestoreCameraRecord(CinemachineVirtualCamera timeRewindCamera, CameraRecord previousCameraRecord, 
									 CameraRecord nextCameraRecord, float previousRecordDeltaTime) {

		TransformRecord previousTransformRecord = previousCameraRecord.cameraTransform;
		TransformRecord nextTransformRecord = nextCameraRecord.cameraTransform;
		float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;

		RestoreTransformRecord(timeRewindCamera.transform, previousTransformRecord, nextTransformRecord, previousRecordDeltaTime);
	}

	private void RestoreStateMachineRecord(Dictionary<Type, StateObject> stateObjects, StateMachineRecord stateMachineRecord) {
		for(int i=0; i < stateMachineRecord.hierarchy.Length-1; i++) {
			Type id = stateMachineRecord.hierarchy[i];
			StateMachine stateMachine = (StateMachine)stateObjects[id];
			stateMachine.IsActive = true;
			stateMachine.CurrentStateObject = stateObjects[stateMachineRecord.hierarchy[i + 1]];
			stateMachine.RestoreFieldsAndProperties(stateMachineRecord.stateObjectRecords[i]);
		}
		int leaftStateIndex = stateMachineRecord.hierarchy.Length - 1;
		Type leaftStateId = stateMachineRecord.hierarchy[leaftStateIndex];
		StateObject leafState = stateObjects[leaftStateId];
		leafState.IsActive = true;
		leafState.RestoreFieldsAndProperties(stateMachineRecord.stateObjectRecords[leaftStateIndex]);
		CurrentStateObject = stateObjects[stateMachineRecord.hierarchy[0]];

		/*
		StateObject newCurrentStateObject = settings.StateObjects[stateMachine.CurrentStateObject.GetType()];
		newCurrentStateObject.RestorePropertiesValues(stateMachine.CurrentStateObject);
		CurrentStateObject = newCurrentStateObject;*/
		//copy stateMachine.CurrentStateObject values to the original stateobject that is referenced by transitions objects and so on
	}


	private void RestoreCharacterMovementRecord(CharacterMovement characterMovement,
												CharacterMovementRecord previousCharacterMovementRecord,
												CharacterMovementRecord nextCharacterMovementRecord,
												float previousRecordDeltaTime) {

		float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;
		Vector3 velocity = Vector3.Lerp(previousCharacterMovementRecord.velocity,
										nextCharacterMovementRecord.velocity,
										lerpAlpha);
		characterMovement.Velocity = velocity;
    }

}