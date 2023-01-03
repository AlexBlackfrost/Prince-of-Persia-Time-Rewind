using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerceptionSystem : MonoBehaviour {
    [Header("Wall run")]
    public GameObject wallRunCheckOrigin;
    public float wallMaxDistance = 1.5f;
    public float wallMaxAngle = 120;
    public float wallMinAngle = 15;
    public float AheadDistanceCheck = 1;
    public LayerMask wallLayerMask;

    [Header("Fall")]
    public GameObject groundCheckOrigin;
    public float startLandGroundDistance = 1.25f;
    public LayerMask groundMask;


    private int AheadDistanceSteps = 5;

    public GameObject CurrentWall { get; private set; }
    public Direction CurrentWallDirection { get; private set; }

    public bool IsRunnableWallNear() {
        // Check multiple times instead of a single check because the check origin may get inside the wall and then raycast won't work
        bool isRunnableWallNear = false;
        float stepSize = AheadDistanceCheck / AheadDistanceSteps;
        for(int i = 0; i < AheadDistanceSteps; i++) {
            Vector3 checkLocation = wallRunCheckOrigin.transform.position + wallRunCheckOrigin.transform.forward * stepSize * i;
            isRunnableWallNear = IsRunnableWallAt(checkLocation);
            if (isRunnableWallNear) {
                break;
            }
        }
        return isRunnableWallNear;
    }

    private bool IsRunnableWallAt(Vector3 checkLocation) {
        bool runnableWallOnLeftSide = false, runnableWallOnRightSide = false;
        RaycastHit leftHitInfo, rightHitInfo;

        // Check wall distance
        bool leftHit = Physics.Raycast(checkLocation, transform.right * -1, out leftHitInfo,
                                       wallMaxDistance, wallLayerMask);

        bool rightHit = Physics.Raycast(checkLocation, transform.right, out rightHitInfo,
                                        wallMaxDistance, wallLayerMask);
        // Check wall rotation
        if (leftHit) {
            float angleToLeftWall = Vector2.Angle(transform.forward.XZ(), leftHitInfo.transform.forward.XZ());
            runnableWallOnLeftSide = angleToLeftWall < wallMaxAngle && angleToLeftWall > wallMinAngle;
        }

        if (rightHit) {
            float angleToRightWall = Vector2.Angle(-rightHitInfo.transform.forward.XZ(), transform.forward.XZ());
            runnableWallOnRightSide = angleToRightWall < wallMaxAngle && angleToRightWall > wallMinAngle;
        }

        // Choose the best wall in case there are 2 of them
        if (runnableWallOnRightSide && !runnableWallOnLeftSide) {
            CurrentWall = rightHitInfo.transform.gameObject;
            CurrentWallDirection = Direction.Right;

        } else if (runnableWallOnLeftSide && !runnableWallOnRightSide) {
            CurrentWall = leftHitInfo.transform.gameObject;
            CurrentWallDirection = Direction.Left;

        } else if (runnableWallOnRightSide && runnableWallOnLeftSide) {
            // Choose the closest wall
            if (leftHitInfo.distance < rightHitInfo.distance) {
                CurrentWall = leftHitInfo.transform.gameObject;
                CurrentWallDirection = Direction.Left;
            } else {
                CurrentWall = rightHitInfo.transform.gameObject;
                CurrentWallDirection = Direction.Right;
            }
        }

        return runnableWallOnLeftSide | runnableWallOnRightSide;
    }

    public bool IsGroundNear() {
        return Physics.Raycast(groundCheckOrigin.transform.position, -groundCheckOrigin.transform.up, startLandGroundDistance, groundMask);
    }
}

