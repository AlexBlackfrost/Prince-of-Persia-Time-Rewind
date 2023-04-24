using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : State {

	[Serializable]
	public class JumpSettings {
		public CharacterMovement CharacterMovement { get; set; }
		public Animator Animator { get; set; }
		public InputController InputController { get; set; }
		public Sword Sword { get; set; }
	}

	private JumpSettings settings;

	public JumpState(JumpSettings settings):base() {
		this.settings = settings;
    }

	protected override void OnEnter() {
		settings.Sword.SheatheIfPossible();
		settings.Sword.UnsheathingEnabled = false;

		settings.Animator.applyRootMotion = true;
		settings.Animator.SetTrigger(AnimatorUtils.jumpHash);
    } 

	protected override void OnExit() {
		settings.Sword.UnsheathingEnabled = true;

		settings.Animator.applyRootMotion = false;
	}

}
