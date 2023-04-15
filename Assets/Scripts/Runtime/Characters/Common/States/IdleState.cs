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
		settings.CharacterMovement.ApplyGravity();
	}

    protected override void OnEnter() {
		settings.Sword.UnsheathingEnabled = true;
    }

	protected override void OnExit() {
		settings.Sword.UnsheathingEnabled = false;
	}


}