using HFSM;
using System;
using UnityEngine;

public class StrafeState : State {
	[Serializable]
	public class StrafeSettings {
		public Animator Animator { get; set; }
		public CharacterMovement CharacterMovement { get; set; }
		public PlayerPerceptionSystem PerceptionSystem {get;set;}
		public Transform Transform { get; set; }
		public Camera MainCamera { get; set; }
		public InputController InputController { get; set; }
		public float StrafeSpeed = 8;
		public float RotationSpeed = 8;
		public float StrafeAnimationSmoothTime = 0.1f;
    }

	private StrafeSettings settings;
	private float strafeSideAnimationVelocity;
	private float strafeForwardAnimationVelocity;
	/* Since strafe animation velocities need to be passed by reference, I cannot use the Rewindable variable autoproperty,,
	*  so I need to have both a rewindable variable and a regular variable
	*/
	private RewindableVariable<float> rewindableStrafeSideAnimationVelocity; 
	private RewindableVariable<float> rewindableStrafeForwardAnimationVelocity;
	public StrafeState(StrafeSettings settings) : base() {
		this.settings = settings;
		this.rewindableStrafeForwardAnimationVelocity = new RewindableVariable<float>(onlyExecuteOnRewindStop : true) ;
		this.rewindableStrafeSideAnimationVelocity = new RewindableVariable<float>(onlyExecuteOnRewindStop : true);
	}

	protected override void OnUpdate() {
		Vector2 inputDirection = settings.InputController.GetMoveDirection();
		settings.PerceptionSystem.ScanEnemiesInStrafeIgnoreRadius();
		Transform closestEnemy = GetClosestEnemyTransform(inputDirection);

		UpdateStrafeMovement(inputDirection, closestEnemy);
		UpdateAnimation(inputDirection);
	}

	protected override void OnEnter() {
		settings.Animator.SetBool(AnimatorUtils.strafeHash, true);
	}

	protected override void OnExit() {
		settings.Animator.SetBool(AnimatorUtils.strafeHash, false);
	}

	private Transform GetClosestEnemyTransform(Vector2 inputDirection) {
		Transform closestEnemy = settings.PerceptionSystem.CurrentDetectedEnemies[0].transform;
		Vector2 enemyDirectionXZ = (closestEnemy.transform.position.XZ() - settings.Transform.position.XZ()).normalized;
		Vector2 cameraRelativeInputDirection = settings.MainCamera.transform.TransformDirection(inputDirection.x, 0, inputDirection.y).XZ().normalized;
		float closestDotProduct = Vector2.Dot(enemyDirectionXZ, cameraRelativeInputDirection);

		foreach(Collider enemyCollider in settings.PerceptionSystem.CurrentDetectedEnemies) {
			enemyDirectionXZ = (enemyCollider.transform.position.XZ() - settings.Transform.position.XZ()).normalized;
			cameraRelativeInputDirection = settings.MainCamera.transform.TransformDirection(inputDirection.x, 0, inputDirection.y).XZ().normalized;
			float dotProduct = Vector2.Dot(enemyDirectionXZ, cameraRelativeInputDirection);

			if(dotProduct > closestDotProduct) {
				closestEnemy = enemyCollider.transform;
				closestDotProduct = dotProduct;
            }
		}
		return closestEnemy;
    }

	private void UpdateStrafeMovement(Vector2 inputDirection, Transform closestEnemy) {
		Vector3 enemyDirection = closestEnemy.position - settings.Transform.position;
		enemyDirection.y = 0;
		enemyDirection.Normalize();

		// Position
		Vector3 moveDirection = settings.MainCamera.transform.TransformDirection(inputDirection.x, 0, inputDirection.y);
		moveDirection.y = 0;
		moveDirection.Normalize();
		settings.CharacterMovement.MoveAmount(moveDirection* settings.StrafeSpeed*Time.deltaTime);

		// Rotation
		Quaternion targetRotation = Quaternion.LookRotation(enemyDirection);
		Quaternion rotation = Quaternion.Slerp(settings.Transform.rotation, targetRotation, settings.RotationSpeed * Time.deltaTime);
		settings.CharacterMovement.SetRotation(rotation);
	}

	private void UpdateAnimation(Vector2 inputDirection) {
		Vector2 cameraRelativeInputDirection = settings.MainCamera.transform.TransformDirection(inputDirection.x, 0, inputDirection.y).XZ().normalized;
		Vector2 characterRelativeDirection = settings.Transform.InverseTransformDirection(cameraRelativeInputDirection.x, 0, cameraRelativeInputDirection.y).XZ().normalized;

		/*Round the vector coordinates to integer values since those are the ones that look better.
		* Non integer values output a blended pose that doesn't look really good. 
		* Blended poses should only be used for abrief amount of time when interpolating with SmoothDamp.*/
		Vector2 discretizedCharacterRelativeDirection = Discretize(characterRelativeDirection);

		float currentStrafeSideSpeed = settings.Animator.GetFloat(AnimatorUtils.strafeSideHash);
		float currentStrafeForwardSpeed = settings.Animator.GetFloat(AnimatorUtils.strafeForwardHash);

		// Get the values from the rewindable variables
		strafeForwardAnimationVelocity = rewindableStrafeForwardAnimationVelocity.Value;
		strafeSideAnimationVelocity = rewindableStrafeSideAnimationVelocity.Value;

		float targetStrafeSideSpeed = Mathf.SmoothDamp(currentStrafeSideSpeed, discretizedCharacterRelativeDirection.x, 
													   ref strafeSideAnimationVelocity, settings.StrafeAnimationSmoothTime);
		float targetStrafeForwardSpeed = Mathf.SmoothDamp(currentStrafeForwardSpeed, discretizedCharacterRelativeDirection.y, 
														  ref strafeForwardAnimationVelocity, settings.StrafeAnimationSmoothTime);
		
		// After passing the values by reference, SmoothDamp has modified them, now update the rewindable variables
		rewindableStrafeForwardAnimationVelocity.Value = strafeForwardAnimationVelocity;
		rewindableStrafeSideAnimationVelocity.Value = strafeSideAnimationVelocity;
		
		settings.Animator.SetFloat(AnimatorUtils.strafeSideHash, targetStrafeSideSpeed);
		settings.Animator.SetFloat(AnimatorUtils.strafeForwardHash, targetStrafeForwardSpeed);
		
    }

	private Vector2 Discretize(Vector2 input) {
		input.x = Mathf.RoundToInt(input.x);
		input.y = Mathf.RoundToInt(input.y);
		return input;
    }

	public override void RestoreFieldsAndProperties(object stateObjectRecord) {
		StrafeStateRecord record = (StrafeStateRecord)stateObjectRecord;
		rewindableStrafeSideAnimationVelocity.Value = record.strafeSideAnimationVelocity;
		rewindableStrafeForwardAnimationVelocity.Value = record.strafeForwardAnimationVelocity;
	}

	public override object RecordFieldsAndProperties() {
		return new StrafeStateRecord(rewindableStrafeSideAnimationVelocity.Value, rewindableStrafeForwardAnimationVelocity.Value);
	}
}