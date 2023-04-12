using HFSM;
using System;
using UnityEngine;

public class RollState : State {
	
	[Serializable]
	public class RollSettings {
		public Animator Animator { get; set; }
		public AnimationCurve rollForwardMotion;
	}

	private RollSettings settings;
	private int rollHash;
	public RollState(RollSettings settings) : base() {
		this.settings = settings;
		rollHash = Animator.StringToHash("Roll");
	}

	protected override void OnUpdate() { 
	
	}

	protected override void OnEnter() {
		settings.Animator.applyRootMotion = true;
		settings.Animator.SetTrigger(rollHash);
	}

	protected override void OnExit() {
		settings.Animator.applyRootMotion = false;
	
	}
}