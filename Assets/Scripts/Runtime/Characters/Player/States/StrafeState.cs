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
    }

	private StrafeSettings settings;
	public StrafeState(StrafeSettings settings) : base() {
		this.settings = settings;
	}

	protected override void OnUpdate() {
		Vector2 inputDirection = settings.InputController.GetMoveDirection();
		settings.PerceptionSystem.ScanEnemies();
		Transform closestEnemy = GetClosestEnemyTransform(inputDirection);

		UpdateStrafeMovement(inputDirection, closestEnemy);
	}

	protected override void OnEnter() {
		
	}

	protected override void OnExit() {
	
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
}