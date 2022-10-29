using HFSM;
using System;
using UnityEngine;

public class IdleState : State {
	[Serializable] public class IdleSettings {
		public Animator Animator { get; set; }
    }

	private IdleSettings settings;

	public IdleState(IdleSettings settings) : base() {
		this.settings = settings;
	}

	protected override void OnUpdate() { 
	
	}

	protected override void OnEnter() {
		
	}

	protected override void OnExit() {
	
	}
}