using HFSM;
using System;
using UnityEngine;

public class MoveState : State {
	[Serializable] public class MoveSettings {
		public CharacterMovement CharacterMovement { get; set; }
		public Animator Animator { get; set; }
		public InputController InputController { get; set; }
		public Sword Sword { get; set; }
	}

	private MoveSettings settings;
	private int runBoolHash;
	private Camera mainCamera;
	public MoveState(MoveSettings settings) : base() {
		this.settings = settings;
		runBoolHash = Animator.StringToHash("Run");
		mainCamera = Camera.main;
	}

	protected override void OnUpdate() {
		Vector2 inputDirection = settings.InputController.GetMoveDirection();
		Vector3 moveDirection = mainCamera.transform.TransformDirection(inputDirection.x, 0, inputDirection.y);
		moveDirection.y = 0;
		moveDirection.Normalize();

		settings.CharacterMovement.Move(moveDirection);
	}

	protected override void OnEnter() {
		settings.Animator.SetBool(runBoolHash, true);
		settings.Sword.CanBeUnsheathed = true;
	}

	protected override void OnExit() {
		settings.Animator.SetBool(runBoolHash, false);
		settings.Sword.CanBeUnsheathed = false;
	}
}