using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;



public class GrabSwordState : State {
	[Serializable]
	public class GrabSwordSettings {
		[field:SerializeField] public Transform RightArmIKTarget { get; private set; }
		[field:SerializeField] public Transform Sword { get; private set; }
		[field:SerializeField] public Transform rightHandSocket { get; private set; }
		[field:SerializeField] public Rig RightArmIKRig { get; private set; }
		[field: SerializeField] public float ExtendHandSpeed { get; private set; } = 5;
		[field: SerializeField] public float RetractHandSpeed { get; private set; } = 8;
	}

	public bool GrabbedSword { get; private set; }

	private GrabSwordSettings settings;
	private bool extendingHand, retractingHand;
	
	public GrabSwordState(GrabSwordSettings settings) : base() {
		this.settings = settings;
		extendingHand = false;
		retractingHand = false;
	}

	protected override void OnUpdate() {
		if (extendingHand) {
			settings.RightArmIKRig.weight = Mathf.Min(settings.RightArmIKRig.weight + settings.ExtendHandSpeed * Time.deltaTime, 1);
			if(settings.RightArmIKRig.weight == 1) {
				extendingHand = false;
				retractingHand = true;
				settings.Sword.SetParent(settings.rightHandSocket, true);
				settings.Sword.localPosition = Vector3.zero;
				settings.Sword.localRotation = Quaternion.identity;
				settings.Sword.GetComponent<Sword>().enabled = false;
            }
        }

        if (retractingHand) {
			settings.RightArmIKRig.weight = Mathf.Max(settings.RightArmIKRig.weight - settings.RetractHandSpeed * Time.deltaTime, 0);
			if (settings.RightArmIKRig.weight == 0) {
				retractingHand = false;
				GrabbedSword = true;
			}
		}
	}

	protected override void OnEnter() {
		extendingHand = true;
		GrabbedSword = false;
	}

	protected override void OnExit() {
	
	}
}