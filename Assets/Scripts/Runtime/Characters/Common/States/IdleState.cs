using HFSM;
using System;
using UnityEngine;

public class IdleState : State {
	[Serializable] public class IdleSettings {
		public Animator Animator { get; set; }
		public CharacterMovement CharacterMovement { get; set; }
		public Sword Sword { get; set; }
    }

	private IdleSettings settings;

	public IdleState(IdleSettings settings) : base() {
		this.settings = settings;
	}

	protected override void OnUpdate() {
		// CharacterController doesn't take collisions into account if CharacterController.Move() hasn't been called.
		// Also, gravity needs to do its job, so call Move with zero speed and direction.
		settings.CharacterMovement.Move(Vector3.zero, 0.0f);
	}

    protected override void OnEnter() {
		settings.Sword.UnsheathingEnabled = true;
    }

	protected override void OnExit() {
		settings.Sword.UnsheathingEnabled = false;
	}

	public override object RecordFieldsAndProperties() {
		return null;
	}

	public override void RestoreFieldsAndProperties(object fieldsAndProperties) { }
}