using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachPlayerState : State{
	[Serializable]
	public class ApproachPlayerSettings {
		public CharacterMovement CharacterMovement { get; set; }
		public Animator Animator { get; set; }
		[field:SerializeField] public PlayerController PlayerController { get; set; }
        public Transform Transform { get; set; }
		public Sword Sword { get; set; }
        public float approachPlayerSpeed = 8;
        public float approachPlayerRotationSpeed = 13;
        public float stopApproachingPlayerAcceptanceRadius = 1;
        public float stopApproachingPlayerAcceptanceDotProduct = 0.8f;
	}

    private ApproachPlayerSettings settings;

	public ApproachPlayerState(ApproachPlayerSettings settings) {
		this.settings = settings;
    }

    protected override void OnEnter() {
        settings.Animator.SetBool(AnimatorUtils.runHash, true);
    }

    protected override void OnUpdate() {
        Vector3 playerDirection = (settings.PlayerController.transform.position - settings.Transform.position);
        playerDirection.y = 0;
        playerDirection.Normalize();

        Vector3 moveAmount = playerDirection * settings.approachPlayerSpeed * Time.deltaTime;
        settings.CharacterMovement.MoveAmount(moveAmount);

        if(playerDirection.magnitude > 0) {
            Quaternion targetRotation = Quaternion.LookRotation(playerDirection);
            Quaternion rotation = Quaternion.Slerp(settings.Transform.rotation, targetRotation, settings.approachPlayerRotationSpeed * Time.deltaTime);
            settings.CharacterMovement.SetRotation(rotation);
        }
    }

    protected override void OnExit() {
        settings.Animator.SetBool(AnimatorUtils.runHash, false);
    }

    public bool ReachedPlayer() {
        float distanceXZ = Vector2.Distance(settings.PlayerController.transform.position.XZ(), settings.Transform.position.XZ());
        Vector2 playerDirectionXZ = (settings.PlayerController.transform.position - settings.Transform.position).XZ().normalized;
        Vector3 lookDirectionXZ = settings.Transform.forward.XZ();
        float dotProduct = Vector2.Dot(playerDirectionXZ, lookDirectionXZ);

        return distanceXZ < settings.stopApproachingPlayerAcceptanceRadius &&
               dotProduct >= settings.stopApproachingPlayerAcceptanceDotProduct;
    }
}