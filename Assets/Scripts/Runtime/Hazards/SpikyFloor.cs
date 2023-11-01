using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikyFloor : MonoBehaviour{
    [SerializeField] private AnimationCurve Movement;
    [SerializeField] private float damage = 100;
    [SerializeField] private HazardDamageSource damageSource;
    [SerializeField] private AudioSource enabledSound;
    [SerializeField] private AudioSource disabledSound;
    [SerializeField] private float playEnabledSoundTime = 0f;
    [SerializeField] private float playDisabledSoundTime = 4f;

    private SkinnedMeshRenderer meshRenderer;
    private int spikeBlendShapeIndex = 0;
    private Collider damageCollider;
    private bool playedEnabledSound; 
    private bool playedDisabledSound;
    private float animationDuration;

    private void OnTriggerEnter(Collider other) {
        IDamageable damageable = other.gameObject.GetComponentInChildren<IDamageable>();
        damageable?.ReceiveDamage(damage, damageSource);
    }

    private void Awake(){
        meshRenderer = GetComponent<SkinnedMeshRenderer>();
        damageCollider = GetComponent<Collider>();
        damageCollider.enabled = false;

        damageSource.DamageApplier = this.gameObject;
        animationDuration = Movement.keys[Movement.length - 1].time;
    }

    private void Update(){
        float time = (float)TimeRewindManager.Instance.SecondsSinceStart();

        // Animation
        float spikeValue = Movement.Evaluate(Movement.Evaluate(time));
        meshRenderer.SetBlendShapeWeight(spikeBlendShapeIndex, (1 - spikeValue) * 100);

        if(Mathf.Approximately(spikeValue, 1) && !damageCollider.enabled) {
            damageCollider.enabled = true;
        }else if(Mathf.Approximately(spikeValue, 0) && damageCollider.enabled) {
            damageCollider.enabled = false;
        }

        // Audio
        if(!TimeRewindManager.Instance.IsRewinding) {

            if (!playedEnabledSound && time%animationDuration > playEnabledSoundTime) {
                enabledSound.Play();
                playedEnabledSound = true;

            }else if (time % animationDuration >=0 && time %animationDuration < playEnabledSoundTime) {
                playedEnabledSound = false;
            }


            if (!playedDisabledSound && time%animationDuration > playDisabledSoundTime) {
                disabledSound.Play();
                playedDisabledSound = true;

            } else if (time % animationDuration >= 0 && time % animationDuration < playDisabledSoundTime) {
                playedDisabledSound = false;
            }
        }

    }
}