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
	}

	private JumpSettings settings;
	private int jumpHash;

	public JumpState(JumpSettings settings):base() {
		this.settings = settings;
		jumpHash = Animator.StringToHash("Jump");
    }

	protected override void OnEnter() {
		settings.Animator.applyRootMotion = true;
		settings.Animator.SetTrigger(jumpHash);
    } 

	protected override void OnExit() {
		settings.Animator.applyRootMotion = false;
	}

}

