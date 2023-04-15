using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPerceptionSystem : MonoBehaviour{
    [Header("Detect player")]
    public float detectionAngle = 150;
    public GameObject player;

    public bool IsSeeingPlayer() {
        Vector2 playerDirectionXZ = (player.transform.position - transform.position).XZ();
        float angleToPlayer = Vector2.Angle(transform.forward.XZ().normalized, playerDirectionXZ.normalized);

        return angleToPlayer < detectionAngle;
    }

}