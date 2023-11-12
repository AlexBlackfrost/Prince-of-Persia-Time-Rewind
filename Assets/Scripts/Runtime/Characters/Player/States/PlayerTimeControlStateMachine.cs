using Cinemachine;
using HFSM;
using System;
using UnityEngine;

public class PlayerTimeControlStateMachine : StateMachine {
	[Serializable]
	public class PlayerTimeControlSettings {
		[field: SerializeField] public CinemachineFreeLook FreeLookCamera { get; set; }
		[field: SerializeField] public AudioSource TimeRewindStartSound { get; set; }
		[field: SerializeField] public AudioSource TimeRewindLoopSound { get; set; }
		[field: SerializeField] public AudioSource TimeRewindEndSound { get; set; }
		public CinemachineVirtualCamera timeRewindCamera;
		public Camera Camera { get; set; }
		public InputController InputController { get; set; }
		public Transform Transform { get; set; }
		public CharacterMovement CharacterMovement { get; set; }
		public Animator Animator { get; set; }
		public Sword Sword { get; set; }
		public Health Health { get; set; }
		public Hurtbox Hurtbox { get; set; }
		public PlayerTimeRewinder PlayerTimeRewinder { get; set; }

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
	private CinemachineBrain cinemachineBrain;
	private bool timeRewindPressedLastFrame = false;
	private bool timeRewindIsPressed = false;

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

		timeIsRewinding = false;
		TimeRewindManager.Instance.TimeRewindStart += OnTimeRewindStart;
		TimeRewindManager.Instance.TimeRewindStop += OnTimeRewindStop;
	}
	 
    protected override void OnLateUpdate() {
		bool timeRewindPressedThisFrame = settings.InputController.IsTimeRewindPressed();

		if(timeRewindPressedThisFrame && !timeRewindPressedLastFrame) {
			timeRewindIsPressed = true;
		}else if(!timeRewindPressedThisFrame && timeRewindPressedLastFrame) {
			timeRewindIsPressed = false;
		}

		timeRewindPressedLastFrame = timeRewindPressedThisFrame;

		if (settings.PlayerTimeRewinder.HasSandTanks() && timeRewindIsPressed && !timeIsRewinding) {
			settings.PlayerTimeRewinder.ConsumeSandTank();
			TimeRewindManager.Instance.StartTimeRewind();
			timeIsRewinding = true;

		} else if ((!timeRewindIsPressed && timeIsRewinding) || (timeIsRewinding && settings.PlayerTimeRewinder.Count()<=2)) {
			TimeRewindManager.Instance.StopTimeRewind();
			timeIsRewinding = false;
			timeRewindIsPressed = false;
        }

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
		previousRecord = settings.PlayerTimeRewinder.Pop();
		nextRecord = settings.PlayerTimeRewinder.Peek();

		// Animation
		animationTimeControl.OnTimeRewindStart();

		// Camera
		cameraTimeControl.OnTimeRewindStart();

		// State machine
		stateMachineTimeControl.OnTimeRewindStart();

		// Sword
		settings.Sword.OnTimeRewindStart();

        // Sound
        if (settings.TimeRewindEndSound.isPlaying) {
            settings.TimeRewindEndSound.Stop();
        }

        if (settings.TimeRewindStartSound.isPlaying) {
			settings.TimeRewindStartSound.Stop();
		}
		settings.TimeRewindStartSound.Play();

        if (settings.TimeRewindLoopSound.isPlaying) {
            settings.TimeRewindLoopSound.Stop();
        }
        settings.TimeRewindLoopSound.Play();
    }

	private void OnTimeRewindStop() {
		// Animation
		animationTimeControl.OnTimeRewindStop(previousRecord.animationRecord, nextRecord.animationRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		// Camera
		cameraTimeControl.OnTimeRewindStop();

		// State machine
		stateMachineTimeControl.RestoreStateMachineRecord(previousRecord.stateMachineRecord);

		// Transform
		transformTimeControl.OnTimeRewindStop(previousRecord.playerTransform, nextRecord.playerTransform, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		// Character movement
		characterMovementTimeControl.OnTimeRewindStop(previousRecord.characterMovementRecord, nextRecord.characterMovementRecord, 
													  previousRecord.deltaTime, elapsedTimeSinceLastRecord);
		// Sword
		settings.Sword.OnTimeRewindStop(previousRecord.swordRecord, nextRecord.swordRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		// Health
		healthTimeControl.OnTimeRewindStop(previousRecord.healthRecord, nextRecord.healthRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);
		
		// Hurtbox
		hurtboxTimeControl.OnTimeRewindStop(previousRecord.hurtboxRecord, nextRecord.hurtboxRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);

        // Sound
        settings.TimeRewindLoopSound.Stop();
        settings.TimeRewindEndSound.Play();
    }

    private void SavePlayerRecord() {
		PlayerRecord playerRecord = new PlayerRecord(transformTimeControl.RecordTransformData(),
													 cameraTimeControl.RecordCameraData(),
													 animationTimeControl.RecordAnimationData(),
													 stateMachineTimeControl.RecordStateMachineData(),
													 characterMovementTimeControl.RecordCharacterMovementData(),
													 settings.Sword.RecordSwordData(),
													 healthTimeControl.RecordHealthData(),
													 hurtboxTimeControl.RecordHurtboxData(),
													 Time.deltaTime);

		// Check for interrupted transitions -- Now it's done inside animationTimeControl
		//animationTimeControl.TrackInterruptedTransitions(ref playerRecord.animationRecord, playerRecord.deltaTime);

		settings.PlayerTimeRewinder.Push(playerRecord);
	}


	private void RewindPlayerRecord() {
		while (elapsedTimeSinceLastRecord > previousRecord.deltaTime && settings.PlayerTimeRewinder.Count() > 2) {
			elapsedTimeSinceLastRecord -= previousRecord.deltaTime;
			previousRecord = settings.PlayerTimeRewinder.Pop();
			nextRecord = settings.PlayerTimeRewinder.Peek(); 
		}
		
		RestorePlayerRecord(previousRecord, nextRecord);
		elapsedTimeSinceLastRecord += Time.deltaTime * TimeRewindManager.Instance.RewindSpeed;
	}


	private void RestorePlayerRecord(PlayerRecord previousRecord, PlayerRecord nextRecord) {
		transformTimeControl.RestoreTransformRecord(previousRecord.playerTransform, nextRecord.playerTransform, previousRecord.deltaTime, 
													elapsedTimeSinceLastRecord);

		cameraTimeControl.RestoreCameraRecord(previousRecord.cameraRecord, nextRecord.cameraRecord, previousRecord.deltaTime, 
											  elapsedTimeSinceLastRecord);

		animationTimeControl.RestoreAnimationRecord(previousRecord.animationRecord, nextRecord.animationRecord, previousRecord.deltaTime, 
													elapsedTimeSinceLastRecord);
		animationTimeControl.RestoreAnimatorFloatParameters(previousRecord.animationRecord, nextRecord.animationRecord, previousRecord.deltaTime,
													elapsedTimeSinceLastRecord);

		characterMovementTimeControl.RestoreCharacterMovementRecord(previousRecord.characterMovementRecord, nextRecord.characterMovementRecord, 
																	previousRecord.deltaTime, elapsedTimeSinceLastRecord);

		healthTimeControl.RestoreHealthRecord(previousRecord.healthRecord, nextRecord.healthRecord, previousRecord.deltaTime, 
											  elapsedTimeSinceLastRecord);

		hurtboxTimeControl.RestoreHurtboxRecord(previousRecord.hurtboxRecord, nextRecord.hurtboxRecord, previousRecord.deltaTime,
												elapsedTimeSinceLastRecord);

		settings.Sword.RestoreSwordRecord(previousRecord.swordRecord, nextRecord.swordRecord, previousRecord.deltaTime, elapsedTimeSinceLastRecord);


		UnityEngine.Debug.Log("Rewinding... " + nextRecord.stateMachineRecord.stateObjectRecords[0].stateObject.ToString());
	}

	public override object RecordFieldsAndProperties() {
		return null;
	}

	public override void RestoreFieldsAndProperties(object fieldsAndProperties) { }
}