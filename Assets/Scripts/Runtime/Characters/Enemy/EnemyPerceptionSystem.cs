using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPerceptionSystem : MonoBehaviour{
    [Header("Detect player")]
    public float detectionMaxDistance = 10;
    public float detectionAngle = 150;
    public GameObject player;
    public GameObject detectionOrigin;
    public LayerMask detectionLayer;
    public float attackPlayerRange = 1.5f;

    private Hurtbox playerHurtbox;

    private void Awake() {
        playerHurtbox = player.GetComponentInChildren<Hurtbox>();
    }
    public bool IsSeeingPlayer() {
        Vector2 playerDirectionXZ = (player.transform.position - transform.position).XZ();
        float angleToPlayer = Vector2.Angle(transform.forward.XZ().normalized, playerDirectionXZ.normalized);

        Vector3 playerDirection = (player.transform.position - detectionOrigin.transform.position).normalized;
        Ray ray = new Ray(detectionOrigin.transform.position, playerDirection);
        RaycastHit[] hits = new RaycastHit[2];
        bool clearLineSight = false;
        Debug.DrawRay(detectionOrigin.transform.position, playerDirection*detectionMaxDistance);
        if(Physics.RaycastNonAlloc(ray, hits, detectionMaxDistance, detectionLayer)>0) {
            foreach(RaycastHit hit in hits) {
                if (hit.collider.gameObject == gameObject) {
                    continue;

                }else if(hit.collider.gameObject == playerHurtbox.gameObject) {
                    clearLineSight = true;
                    break;

                } else {
                    break;
                }
            }
        } 

        return  clearLineSight &&  angleToPlayer < detectionAngle;
        /*
         * Vector2 playerDirectionXZ = (player.transform.position - transform.position).XZ();
        float angleToPlayer = Vector2.Angle(transform.forward.XZ().normalized, playerDirectionXZ.normalized);

        return angleToPlayer < detectionAngle;
        */
    }

    public bool IsPlayerInAttackRange() {
        return MathUtils.DistanceXZ(player.transform.position, transform.position) < attackPlayerRange;
    }

}