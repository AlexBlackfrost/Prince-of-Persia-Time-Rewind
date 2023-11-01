using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikyPole : MonoBehaviour{
    [field: SerializeField] public float Displacement { get; set; } = 3;
    [field: SerializeField] public float Speed { get; private set; } = 6;
    [field: SerializeField] public float RotationSpeed { get; private set; } = 6;
    [field: SerializeField] [field: Tooltip("Range [0,1]. 1 is the maximum displacement")] public float InitialOffset { get; private set; }

    [SerializeField] private float damage = 10;
    [SerializeField] private HazardDamageSource damageSource;

    [SerializeField] private AudioSource sound;
    [SerializeField] private float startSoundDelay = 0.0f;

    public Vector3 InitialPosition { get; private set; }
    public Vector3 MoveDirection { get; private set; }
    public Quaternion InitialRotation { get; private set; }


    private float initialOffsetTime;
    private Collider damageCollider;

    private void OnTriggerEnter(Collider other) {
        IDamageable damageable = other.gameObject.GetComponentInChildren<IDamageable>();
        damageable?.ReceiveDamage(damage, damageSource);
    }

    private void Awake(){
        MoveDirection = transform.forward;
        InitialPosition = transform.position;
        initialOffsetTime =  Displacement/Speed * InitialOffset;
        InitialRotation = transform.rotation;

        damageSource.DamageApplier = this.gameObject;
        TimeRewindManager.Instance.TimeRewindStart += EnableSound;
        TimeRewindManager.Instance.TimeRewindStop += EnableSound;
        StartCoroutine(EnableSoundDelayed());
    }

    private void Update(){
        float time = (float)TimeRewindManager.Instance.SecondsSinceStart();
        transform.position = InitialPosition + MoveDirection * EvaluateDisplacement(time+ initialOffsetTime);
        transform.rotation = Quaternion.Euler(InitialRotation.x, InitialRotation.y + RotationSpeed * time, InitialRotation.z);
    }

    /**
     * Equation that returns a periodic position offset based on time; a triangular wave-like function
     * Inputs: time, speed, displacement.
     * 
     * How I arrived to this equation:
     * 
     * - Start with y = time.
     * - Now use mod operator to achieve the periodic effect. Let's start with period = 1. y = time%1
     * - Since we want the displacement to be simmetric, let's multiply mod's divisor by 2. y = time%2
     * - Now subtract one to offset the whole function so that half of the values are negative, 
     *   and then use abs() to make them positive. y = abs(time%2 - 1)
     * - Add one to time so that displacement at time 0 is 0 (offset the wave). y = abs( (time+1)%2 -1)
     * - Multiply everything by displacement to control the distance/wave amplitude. y = abs( (time+1)%2 -1) * Displacement
     * - Multiply time by a variable in order to control the wave speed/frequency.  y = abs( (time*Speed + 1)%2 -1) * Displacement
     * - Finally, make wavelength and amplitude the same value; divide speed by displacement so that we don't have to modify 
     *   both diplacement and speed everytime we modify just one of them, i.e. if we multiply displacement by 2 but don't modify speed, 
     *   it will take longer to reach the 2 meters offset. y = abs( (time*(Speed/Displacement) + 1)%2 -1) * Displacement
     */
    public float EvaluateDisplacement(float time) {
        return Mathf.Abs((time*(Speed/Displacement) + 1 )% 2 - 1) * Displacement;
    }

    private void EnableSound() {
        if (!sound.isPlaying) {
            sound.Play();
        }
    }

    private void DisableSound() {
        sound.Stop();
    }

    private IEnumerator EnableSoundDelayed() {
        yield return new WaitForSeconds(startSoundDelay);
        EnableSound();
    }

}