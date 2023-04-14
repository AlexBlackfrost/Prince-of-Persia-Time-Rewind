using Cinemachine;
using HFSM;
using System;
using UnityEngine;

public class RollState : State {
	
	[Serializable]
	public class RollSettings {
		public Animator Animator { get; set; }
		public CharacterMovement CharacterMovement { get; set; }
		public Camera MainCamera { get; set; }
		[field:SerializeField]public CinemachineFreeLook FreeLookCamera { get; set; }
		public InputController InputController { get; set; }
		public Transform Transform { get; set; }
		public AnimationCurve rollSpeed;
		public float rollSpeedMultiplier = 15;
		public float rollRotationSpeed = 6;
	}

	private RollSettings settings;
	private int rollHash;
	private float rollElapsedTime;

	public RollState(RollSettings settings) : base() {
		this.settings = settings;
		rollHash = Animator.StringToHash("Roll");
	}

	protected override void OnUpdate() {
		Vector2 inputDirection = settings.InputController.GetMoveDirection();
		Vector3 cameraRelativeInputDirection = settings.MainCamera.transform.TransformDirection(new Vector3(inputDirection.x, 0, inputDirection.y));
		cameraRelativeInputDirection.y = 0;
		cameraRelativeInputDirection.Normalize();
		Vector3 rollDirection = settings.Transform.forward;
		Vector3 moveAmount = rollDirection * settings.rollSpeed.Evaluate(rollElapsedTime) * settings.rollSpeedMultiplier * Time.deltaTime;
		settings.CharacterMovement.MoveAmount(moveAmount);

		if(cameraRelativeInputDirection.magnitude > 0) {
			Quaternion targetRotation = Quaternion.LookRotation(cameraRelativeInputDirection);
			Quaternion rollRotation = Quaternion.Slerp(settings.Transform.rotation, targetRotation, settings.rollRotationSpeed * Time.deltaTime);
			settings.CharacterMovement.SetRotation(rollRotation);

		}

		rollElapsedTime += Time.deltaTime;
	}

	protected override void OnEnter() {
		Vector2 inputDirection = settings.InputController.GetMoveDirection();
		settings.Animator.SetTrigger(rollHash);
		rollElapsedTime = 0;
		Vector3 cameraRelativeInputDirection = settings.MainCamera.transform.TransformDirection(new Vector3(inputDirection.x, 0, inputDirection.y));
		cameraRelativeInputDirection.y = 0;
		cameraRelativeInputDirection.Normalize();

		if (cameraRelativeInputDirection.magnitude > 0) {
			settings.CharacterMovement.SetRotation(Quaternion.LookRotation(cameraRelativeInputDirection));
		}

	}

	protected override void OnExit() { }

	public override void RestoreFieldsAndProperties(object stateObjectRecord) {
		RollStateRecord record = (RollStateRecord)stateObjectRecord;
		rollElapsedTime = record.rollElapsedTime;
	}

	public override object RecordFieldsAndProperties() {
		return new RollStateRecord(rollElapsedTime);
	}

}