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
		[SerializeField] public float minTimeBetweenFootstepSounds { get; set; } = 0.3f;
		public Action PlayFootstepSound;
		[field:SerializeField] public AudioSource FootstepSound { get; set; }
		[field:SerializeField] public float FootstepMinPitch { get; set; } = 0.9f;
		[field:SerializeField] public float FootstepMaxPitch { get; set; } = 1.1f;
    }

	private StrafeSettings settings;
	private float strafeSideAnimationVelocity;
	private float strafeForwardAnimationVelocity;
	private float elapsedTimeSinceLastFootstepSound;
	public StrafeState(StrafeSettings settings) : base() {
		this.settings = settings;
		settings.PlayFootstepSound = PlayFootstepSound;
	}

	protected override void OnUpdate() {
		Vector2 inputDirection = settings.InputController.GetMoveDirection();
		settings.PerceptionSystem.ScanEnemiesInStrafeIgnoreRadius();
		Transform closestEnemy = GetClosestEnemyTransform(inputDirection);

		UpdateStrafeMovement(inputDirection, closestEnemy);
		UpdateAnimation(inputDirection);
		elapsedTimeSinceLastFootstepSound += Time.deltaTime;
	}

	protected override void OnEnter() {
		settings.Animator.SetBool(AnimatorUtils.strafeHash, true);
		elapsedTimeSinceLastFootstepSound = 0;
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

		float targetStrafeSideSpeed = Mathf.SmoothDamp(currentStrafeSideSpeed, discretizedCharacterRelativeDirection.x, 
													   ref strafeSideAnimationVelocity, settings.StrafeAnimationSmoothTime);
		float targetStrafeForwardSpeed = Mathf.SmoothDamp(currentStrafeForwardSpeed, discretizedCharacterRelativeDirection.y, 
														  ref strafeForwardAnimationVelocity, settings.StrafeAnimationSmoothTime);
		
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
		strafeSideAnimationVelocity = record.strafeSideAnimationVelocity;
		strafeForwardAnimationVelocity = record.strafeForwardAnimationVelocity;
		elapsedTimeSinceLastFootstepSound = 0;
	}

	public override object RecordFieldsAndProperties() {
		return new StrafeStateRecord(strafeSideAnimationVelocity, strafeForwardAnimationVelocity);
	}

    private void PlayFootstepSound() {
        if (elapsedTimeSinceLastFootstepSound > settings.minTimeBetweenFootstepSounds) {
            settings.FootstepSound.pitch = UnityEngine.Random.Range(settings.FootstepMinPitch, settings.FootstepMaxPitch);
            settings.FootstepSound.Play();
            elapsedTimeSinceLastFootstepSound = 0;
        }

    }
}