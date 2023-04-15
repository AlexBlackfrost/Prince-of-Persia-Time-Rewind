using HFSM;
using System;
using UnityEngine;

public class WallRunState : State {

	[Serializable] public class WallRunSettings {
		public Animator Animator {get;set;}
		public GameObject Wall { get; set; }
		public Transform Transform { get; set; }
		public CharacterMovement CharacterMovement { get; set; }
		public Sword Sword { get; set; }
		public Direction WallSide { get; set; }
		public float positionOffsetCorrectionDuration = 0.5f;
		public float rotationOffsetCorrectionDuration = 0.75f;
		public float downwardSpeedAtEnd = 5;
	}

	private const float DISTANCE_TO_WALL_SURFACE = 0.57f; //Animation is made so that there's a distance of DISTANCE_TO_WALL_SURFACE in the X axis
	private WallRunSettings settings;
	private int wallRunHash;
	private int wallRunDirectionHash;
	private float positionOffsetCorrectionSpeed;
	private float rotationOffsetCorrectionSpeed;
	private float wallHalfThickness;
	private int animatorLayer = 0;
	private float forwardSpeedOnTransition = 7;
	private Vector3 targetForwardDirection;

	public WallRunState(WallRunSettings settings) : base() {
		this.settings = settings;
		wallRunHash = Animator.StringToHash("WallRun");
		wallRunDirectionHash = Animator.StringToHash("WallRunDirection");
	}

	protected override void OnUpdate() {
		// Move towards the wall, that is, move in the opposite direction of walls' forward vector
		// Wall's forward vector face should be the face where the player is going to wall run
		// Calculate the distance to wall in the X axis in the wall local space.
		CorrectPosition();
		CorrectRotation();
		
        /* Add some speed for better game feeling. This shouldn't be necessary if I had split the wall run into a JumpToWall
         * animation where I would do the root correction and an actual WallRunning animation. Since wall running is not
         * the main focus of this project I'm fine with this small fix to make it feel a little better.
         */
        if (settings.Animator.IsInTransition(animatorLayer)) {
			settings.CharacterMovement.MoveAmount(settings.Transform.forward * forwardSpeedOnTransition * Time.deltaTime);
		}

	}


	protected override void OnEnter() {
		settings.Sword.SheatheIfPossible();
		settings.Sword.UnsheathingEnabled = false;
		settings.Animator.SetInteger(wallRunDirectionHash, (int)settings.WallSide);
		settings.Animator.SetBool(wallRunHash, true);
		settings.Animator.applyRootMotion = true;
		wallHalfThickness = settings.Wall.GetComponent<BoxCollider>().size.z*settings.Wall.transform.localScale.z / 2;

		if(settings.WallSide == Direction.Right) {
			targetForwardDirection = settings.Wall.transform.right;
        }else if(settings.WallSide == Direction.Left) {
			targetForwardDirection = -settings.Wall.transform.right;
        }

		float ZAxisDistanceToWall = Mathf.Abs((DISTANCE_TO_WALL_SURFACE - wallHalfThickness) - 
											  settings.Wall.transform.InverseTransformPoint(settings.Transform.position).z);
		positionOffsetCorrectionSpeed = ZAxisDistanceToWall / settings.positionOffsetCorrectionDuration;

		float yawRotationToWall = Vector3.Angle(settings.Transform.forward, targetForwardDirection);
		rotationOffsetCorrectionSpeed = yawRotationToWall / settings.rotationOffsetCorrectionDuration;
	}

	protected override void OnExit() {
		settings.Sword.UnsheathingEnabled = true;
		settings.Animator.SetBool(wallRunHash, false);
		settings.Animator.applyRootMotion = false;

		settings.CharacterMovement.Velocity = new Vector3(settings.CharacterMovement.Velocity.x,
														  -settings.downwardSpeedAtEnd,
														  settings.CharacterMovement.Velocity.z);
	}

	private void CorrectPosition() {
		float horizontalDistanceToWallSurface = settings.Wall.transform.InverseTransformPointUnscaled(settings.Transform.position).z -
												wallHalfThickness;

		if (horizontalDistanceToWallSurface > DISTANCE_TO_WALL_SURFACE) {
			// Clamp the movement amount so that it doesn't get too close to the wall and clip through it
			Vector3 direction = -settings.Wall.transform.forward;
			Vector3 displacement = Vector3.ClampMagnitude(direction * positionOffsetCorrectionSpeed * Time.deltaTime,
														  horizontalDistanceToWallSurface - DISTANCE_TO_WALL_SURFACE);
			settings.CharacterMovement.MoveAmount(displacement);

		} else if (horizontalDistanceToWallSurface < DISTANCE_TO_WALL_SURFACE) {
			// Clamp the movement amount so that it doesn't get too far from the wall
			Vector3 direction = settings.Wall.transform.forward;
			Vector3 displacement = Vector3.ClampMagnitude(direction * positionOffsetCorrectionSpeed * Time.deltaTime,
														  DISTANCE_TO_WALL_SURFACE - horizontalDistanceToWallSurface);
			settings.CharacterMovement.MoveAmount(displacement);
		}
	}

	private void CorrectRotation() {
		float yawAngleToWall = Vector3.Angle(settings.Transform.forward, settings.Wall.transform.right);

		if (yawAngleToWall != 0) {
			Vector3 newForwardDirection = Vector3.RotateTowards(settings.Transform.forward,
																targetForwardDirection,
																Math.Min(yawAngleToWall, rotationOffsetCorrectionSpeed * Time.deltaTime),
																0.0f);
			Quaternion newRotation = Quaternion.LookRotation(newForwardDirection);
			settings.CharacterMovement.SetRotation(newRotation);
		}
	}
}