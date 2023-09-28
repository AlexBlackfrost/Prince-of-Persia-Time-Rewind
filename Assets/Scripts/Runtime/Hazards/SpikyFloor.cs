using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikyFloor : MonoBehaviour{
    [SerializeField] private AnimationCurve Movement;
    [SerializeField] private float damage = 100;
    [SerializeField] private HazardDamageSource damageSource;

    private SkinnedMeshRenderer meshRenderer;
    private int spikeBlendShapeIndex = 0;
    private Collider damageCollider;

    private void OnTriggerEnter(Collider other) {
        IDamageable damageable = other.gameObject.GetComponentInChildren<IDamageable>();
        damageable?.ReceiveDamage(damage, damageSource);
    }

    private void Awake(){
        meshRenderer = GetComponent<SkinnedMeshRenderer>();
        damageCollider = GetComponent<Collider>();
        damageCollider.enabled = false;

        damageSource.DamageApplier = this.gameObject;
    }

    private void Update(){
        float time = (float)TimeRewindManager.Instance.SecondsSinceStart();
        float spikeValue = Movement.Evaluate(Movement.Evaluate(time));
        meshRenderer.SetBlendShapeWeight(spikeBlendShapeIndex, (1 - spikeValue) * 100);

        if(Mathf.Approximately(spikeValue, 1) && !damageCollider.enabled) {
            damageCollider.enabled = true;
        }else if(Mathf.Approximately(spikeValue, 0) && damageCollider.enabled) {
            damageCollider.enabled = false;
        }
    }
}