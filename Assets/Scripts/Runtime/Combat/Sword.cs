using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Sword : MonoBehaviour {
    [SerializeField] private Transform backSocket;
    [SerializeField] private Transform handSocket;
    [SerializeField] private float attackCooldown = 0;
    [SerializeField, ReadOnly] private bool hitboxEnabled;
    [SerializeField, ReadOnly] private float attackCooldownRemainingTime;
    [field: SerializeField] public float Damage { get; private set; } = 10;

    public bool SheathingEnabled { get; set; } = true;
    public bool UnsheathingEnabled { get; set; } = true;
    [field:SerializeField]public SwordDamageSource damageSource { get; set; }

    public Action<bool> OnSetComboEnabled;
    public Action<bool> OnSetRotationEnabled;

    private GameObject owner;
    private Animator animator;
    private Hitbox hitbox;
    private const float sheatheAnimationSpeed = 1.5f;
    private const float unsheatheAnimationSpeed = 1.5f;
    private const int swordAnimatorLayer = 1;
    private const float transitionSpeedIntoSwordLayer = 10f;
    private const float transitionSpeedOutOfSwordLayer = 2.5f;
    private float unsheatheMotionTime;
    private float animatorSwordLayerWeight = 0.0f;
    private float animatorSwordLayerTargetWeight;
    private SwordState swordState;
    private Coroutine animationCoroutine;
    private Coroutine layerTransitionCoroutine;
    private HashSet<IHittable> ignoredHittableObjects = new HashSet<IHittable>();

    private void Awake() {
        hitbox = GetComponent<Hitbox>();
        /*animator = Owner.GetComponent<Animator>();
        unsheatheHash = Animator.StringToHash("Unsheathe");
        unsheatheSpeedMultiplierHash = Animator.StringToHash("UnsheatheSpeedMultiplier");
        unsheatheMotionTimeHash = Animator.StringToHash("UnsheatheMotionTime");
        swordState = SwordState.InBack;*/
    }

    public void OnEquipped(GameObject owner) {
        this.owner = owner;
        
        IHittable[] hurtboxes = owner.GetComponentsInChildren<IHittable>();
        ignoredHittableObjects = new HashSet<IHittable>(hurtboxes); // ignore its own hurtbox
        
        animator = owner.GetComponent<Animator>();
        swordState = SwordState.InBack;

        attackCooldownRemainingTime = 0;
    }

    public void SheatheIfPossible() {
        if (SheathingEnabled) {
            if (swordState == SwordState.InHand) {
                animator.SetFloat(AnimatorUtils.unsheatheSpeedMultiplierHash, 0);
                animator.SetTrigger(AnimatorUtils.unsheatheHash);
                animationCoroutine = StartCoroutine(UpdateSheatheAnimation());
                swordState = SwordState.Sheathing;

            } else if(swordState == SwordState.Unsheathing) {
                animator.SetFloat(AnimatorUtils.unsheatheSpeedMultiplierHash, 0);
                if(animationCoroutine != null) {
                    StopCoroutine(animationCoroutine);
                }
                animationCoroutine = StartCoroutine(UpdateSheatheAnimation());
                swordState = SwordState.Sheathing;
            }
        }
    }

    public void UnsheatheIfPossible() {
        if (UnsheathingEnabled) {
            if (swordState == SwordState.InBack) {
                animator.SetFloat(AnimatorUtils.unsheatheSpeedMultiplierHash, 0);
                animator.SetTrigger(AnimatorUtils.unsheatheHash);
                animationCoroutine = StartCoroutine(UpdateUnsheatheAnimation());
                if (layerTransitionCoroutine != null) {
                    StopCoroutine(layerTransitionCoroutine);
                }
                layerTransitionCoroutine = StartCoroutine(UpdateSwordLayerWeightOverTime(1, transitionSpeedIntoSwordLayer));
                swordState = SwordState.Unsheathing;

            } else if (swordState == SwordState.Sheathing) {
                animator.SetFloat(AnimatorUtils.unsheatheSpeedMultiplierHash, 0);
                if (animationCoroutine != null) {
                    StopCoroutine(animationCoroutine);
                }
                animationCoroutine = StartCoroutine(UpdateUnsheatheAnimation());
                swordState = SwordState.Unsheathing;
            }
        }
    }

    public void SwitchSwordSocket() {
        if (swordState == SwordState.Sheathing) {
            transform.SetParent(backSocket, false);
        } else if (swordState == SwordState.Unsheathing) {
            transform.SetParent(handSocket, false);
        }
    }

    public bool IsInHand() {
        return swordState == SwordState.InHand;
    }

    #region Animation
    private void OnUnsheatheAnimationEnded() {
        swordState = SwordState.InHand;
        unsheatheMotionTime = 1;
    }

    private void OnSheatheAnimationEnded() {
        swordState = SwordState.InBack;
        unsheatheMotionTime = 0;
        if (layerTransitionCoroutine != null) {
            StopCoroutine(layerTransitionCoroutine);
        }
        layerTransitionCoroutine = StartCoroutine(UpdateSwordLayerWeightOverTime(0, transitionSpeedOutOfSwordLayer));
    }

    private IEnumerator UpdateUnsheatheAnimation() {
        while (unsheatheMotionTime < 1) {
            unsheatheMotionTime = Mathf.Min(unsheatheMotionTime + Time.deltaTime * unsheatheAnimationSpeed, 1);
            animator.SetFloat(AnimatorUtils.unsheatheMotionTimeHash, unsheatheMotionTime);
            if (unsheatheMotionTime == 1.0f) {
                OnUnsheatheAnimationEnded();
            }
            yield return unsheatheMotionTime;
        }
        yield return null;
    }

    private IEnumerator UpdateSheatheAnimation() {
        while (unsheatheMotionTime > 0) { 
            unsheatheMotionTime = Mathf.Max(unsheatheMotionTime - Time.deltaTime * sheatheAnimationSpeed, 0);
            animator.SetFloat(AnimatorUtils.unsheatheMotionTimeHash, unsheatheMotionTime);
            if (unsheatheMotionTime == 0.0f) {
                OnSheatheAnimationEnded();
            }
            yield return unsheatheMotionTime;
        }
        yield return null;
    }

    private IEnumerator UpdateSwordLayerWeightOverTime(float targetWeight, float speed) {
        animatorSwordLayerTargetWeight = targetWeight;
        while(animatorSwordLayerWeight != animatorSwordLayerTargetWeight) {
            if(animatorSwordLayerWeight < animatorSwordLayerTargetWeight) {
                animatorSwordLayerWeight = Mathf.Min(animatorSwordLayerWeight + speed * Time.deltaTime, 1);
            }else if(animatorSwordLayerWeight > animatorSwordLayerTargetWeight) {
                animatorSwordLayerWeight = Mathf.Max(animatorSwordLayerWeight - speed * Time.deltaTime, 0);
            }
            animator.SetLayerWeight(swordAnimatorLayer, animatorSwordLayerWeight);
            yield return animatorSwordLayerWeight;
        }
        yield return null; layerTransitionCoroutine = null;
    }

    public void SetComboEnabled(Bool enabled) {
        OnSetComboEnabled.Invoke(Convert.ToBoolean((int) enabled));
    }

    public void SetRotationEnabled(Bool enabled) {
        OnSetRotationEnabled.Invoke(Convert.ToBoolean((int)enabled));
    }

    #endregion

    #region Input
    public void OnSheathePressed() {
        SheatheIfPossible();
    }

    public void OnUnsheathePressed() {
        UnsheatheIfPossible();
    }
    #endregion

    #region TimeRewind
    public void OnTimeRewindStart() {
        UnsheathingEnabled = false;
        SheathingEnabled = false;
        if(animationCoroutine != null) {
            StopCoroutine(animationCoroutine);
        }

        if (layerTransitionCoroutine != null) {
            StopCoroutine(layerTransitionCoroutine);
        }
    }

    public void OnTimeRewindStop(SwordRecord previousSwordRecord, SwordRecord nextSwordRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        RestoreSwordRecord(previousSwordRecord,nextSwordRecord,  previousRecordDeltaTime, elapsedTimeSinceLastRecord);
        swordState = previousSwordRecord.swordState;
        SheathingEnabled = previousSwordRecord.sheathingEnabled;
        UnsheathingEnabled = previousSwordRecord.unsheathingEnabled;
        unsheatheMotionTime = animator.GetFloat(AnimatorUtils.unsheatheMotionTimeHash);
        animatorSwordLayerWeight = animator.GetLayerWeight(swordAnimatorLayer);

        if (animationCoroutine != null) {//coroutine hadn't ended when time rewind started
            if (previousSwordRecord.swordState == SwordState.Unsheathing) {
                animationCoroutine = StartCoroutine(UpdateUnsheatheAnimation());
            } else if (previousSwordRecord.swordState == SwordState.Sheathing) {
                animationCoroutine = StartCoroutine(UpdateSheatheAnimation());
            }
        }

        if (layerTransitionCoroutine != null) {
            layerTransitionCoroutine = StartCoroutine(UpdateSwordLayerWeightOverTime(previousSwordRecord.animatorSwordLayerTargetWeight, transitionSpeedIntoSwordLayer));
        }
    }

    public void SetSwordAnimatorLayerEnabled(bool enabled) {
        if (layerTransitionCoroutine != null) {
            StopCoroutine(layerTransitionCoroutine);
        }

        if (enabled) {   
            layerTransitionCoroutine = StartCoroutine(UpdateSwordLayerWeightOverTime(1.0f, transitionSpeedIntoSwordLayer));
        } else {
            layerTransitionCoroutine = StartCoroutine(UpdateSwordLayerWeightOverTime(0.0f, transitionSpeedIntoSwordLayer));
        }
    }

    public SwordRecord RecordSwordData() {
        return new SwordRecord(SheathingEnabled, UnsheathingEnabled, animatorSwordLayerTargetWeight, 
                               swordState, transform.parent, hitboxEnabled, attackCooldownRemainingTime);
    }

    public void RestoreSwordRecord(SwordRecord previousRecord, SwordRecord nextRecord,float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;

        transform.SetParent(previousRecord.swordSocket, false);
        hitboxEnabled = previousRecord.hitboxEnabled;
        attackCooldownRemainingTime = Mathf.Lerp(previousRecord.attackCooldownRemainingTime, nextRecord.attackCooldownRemainingTime, lerpAlpha);
    }

    #endregion

    #region Hit detection
    public void SetHitboxEnabled(Bool enabled) {

        //Debug.Log(owner.name+ " hitbox Enabled: " + enabled);
        hitboxEnabled = Convert.ToBoolean((int)enabled);
    }

    public HitData[] CheckHit() {
        if (hitboxEnabled) { 
            return hitbox.CheckHit(ignoredHittableObjects);
        } else {
            return null;
        }
    }
    #endregion

    private void Update() {
        attackCooldownRemainingTime = Mathf.Max(attackCooldownRemainingTime - Time.deltaTime, 0);
    }

    public void StartCooldown() {
        attackCooldownRemainingTime = attackCooldown;
    }

    public bool CooldownFinished() {
        return attackCooldownRemainingTime <= 0;
    }
}
