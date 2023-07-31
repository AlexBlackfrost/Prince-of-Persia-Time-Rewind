using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Sword : MonoBehaviour, ISerializationCallbackReceiver {
    [SerializeField] private Transform backSocket;
    [SerializeField] private Transform handSocket;
    [SerializeField] private float attackCooldown = 0;
    [SerializeField, ReadOnly] private bool serializedHitboxEnabled;
    [SerializeField, ReadOnly] private float serializedAttackCooldownRemainingTime;
    [field: SerializeField] public float Damage { get; private set; } = 10;

    public bool SheathingEnabled {
        get {
            return sheathingEnabled.Value;
        }
        set {
            sheathingEnabled.Value = value;
        }
    }
    public bool UnsheathingEnabled {
        get {
            return unsheathingEnabled.Value;
        }
        set {
            unsheathingEnabled.Value = value;
        }
    }

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
    private float animatorSwordLayerWeight;
    private float unsheatheMotionTime;
    private Coroutine animationCoroutine;
    private Coroutine layerTransitionCoroutine;
    private HashSet<IHittable> ignoredHittableObjects = new HashSet<IHittable>();

    private bool rewindableVariablesInitialized;
    private RewindableVariable<float> animatorSwordLayerTargetWeight;
    private RewindableVariable<bool> hitboxEnabled;
    private RewindableVariable<float> attackCooldownRemainingTime;
    private RewindableVariable<SwordState> swordState;
    private RewindableParentTransform swordSocket;
    private RewindableVariable<bool> sheathingEnabled;
    private RewindableVariable<bool> unsheathingEnabled;

    public void OnBeforeSerialize() {
        if (hitboxEnabled != null) {
            serializedHitboxEnabled = hitboxEnabled.Value;
            serializedAttackCooldownRemainingTime = attackCooldownRemainingTime.Value;
        }
    }

    public void OnAfterDeserialize() { }

    private void Awake() {
        hitbox = GetComponent<Hitbox>();

        //InitRewindableVariables();
        /*animator = Owner.GetComponent<Animator>();
        unsheatheHash = Animator.StringToHash("Unsheathe");
        unsheatheSpeedMultiplierHash = Animator.StringToHash("UnsheatheSpeedMultiplier");
        unsheatheMotionTimeHash = Animator.StringToHash("UnsheatheMotionTime");
        swordState = SwordState.InBack;*/
    }

    private void InitRewindableVariables() {
        if (!rewindableVariablesInitialized) {
            animatorSwordLayerTargetWeight = new RewindableVariable<float>();
            hitboxEnabled = new RewindableVariable<bool>(onlyExecuteOnRewindStop: false);
            attackCooldownRemainingTime = new RewindableVariable<float>(onlyExecuteOnRewindStop:false);
            swordSocket = new RewindableParentTransform(transform);
            swordState = new RewindableVariable<SwordState>(onlyExecuteOnRewindStop:true);
            sheathingEnabled = new RewindableVariable<bool>(value: true, onlyExecuteOnRewindStop:true);
            unsheathingEnabled = new RewindableVariable<bool>(value: true, onlyExecuteOnRewindStop: true);
            rewindableVariablesInitialized = true;
#if UNITY_EDITOR
            hitboxEnabled.Name = "HitboxEnabled" + gameObject.name;
            attackCooldownRemainingTime.Name = "AttackCooldownRemainingTime" + gameObject.name;
            swordState.Name = "SwordState" + gameObject.name;
            swordSocket.Name = "SwordSocket" + gameObject.name;
            sheathingEnabled.Name = "SheathingEnabled" + gameObject.name;
            unsheathingEnabled.Name = "UnsheathingEnabled" + gameObject.name;
            animatorSwordLayerTargetWeight.Name = "SwordLayerTargetWeight" + gameObject.name;
#endif
        }
    }

    public void OnEquipped(GameObject owner) {
        this.owner = owner;
        
        IHittable[] hurtboxes = owner.GetComponentsInChildren<IHittable>();
        ignoredHittableObjects = new HashSet<IHittable>(hurtboxes); // ignore its own hurtbox

        animator = owner.GetComponent<Animator>();

        InitRewindableVariables();
        swordState.Value = SwordState.InBack;
        attackCooldownRemainingTime.Value = 0;
    }

    public void SheatheIfPossible() {
        if (SheathingEnabled) {
            if (swordState.Value == SwordState.InHand) {
                animator.SetFloat(AnimatorUtils.unsheatheSpeedMultiplierHash, 0);
                animator.SetTrigger(AnimatorUtils.unsheatheHash);
                animationCoroutine = StartCoroutine(UpdateSheatheAnimation());
                swordState.Value = SwordState.Sheathing;

            } else if(swordState.Value == SwordState.Unsheathing) {
                animator.SetFloat(AnimatorUtils.unsheatheSpeedMultiplierHash, 0);
                if(animationCoroutine != null) {
                    StopCoroutine(animationCoroutine);
                }
                animationCoroutine = StartCoroutine(UpdateSheatheAnimation());
                swordState.Value = SwordState.Sheathing;
            }
        }
    }

    public void UnsheatheIfPossible() {
        if (UnsheathingEnabled) {
            if (swordState.Value == SwordState.InBack) {
                animator.SetFloat(AnimatorUtils.unsheatheSpeedMultiplierHash, 0);
                animator.SetTrigger(AnimatorUtils.unsheatheHash);
                animationCoroutine = StartCoroutine(UpdateUnsheatheAnimation());
                if (layerTransitionCoroutine != null) {
                    StopCoroutine(layerTransitionCoroutine);
                }
                layerTransitionCoroutine = StartCoroutine(UpdateSwordLayerWeightOverTime(1, transitionSpeedIntoSwordLayer));
                swordState.Value = SwordState.Unsheathing;

            } else if (swordState.Value == SwordState.Sheathing) {
                animator.SetFloat(AnimatorUtils.unsheatheSpeedMultiplierHash, 0);
                if (animationCoroutine != null) {
                    StopCoroutine(animationCoroutine);
                }
                animationCoroutine = StartCoroutine(UpdateUnsheatheAnimation());
                swordState.Value = SwordState.Unsheathing;
            }
        }
    }

    public void SwitchSwordSocket() {
        if (swordState.Value == SwordState.Sheathing) {
            transform.SetParent(backSocket, false);
        } else if (swordState.Value == SwordState.Unsheathing) {
            transform.SetParent(handSocket, false);
        }
    }

    public bool IsInHand() {
        return swordState.Value == SwordState.InHand;
    }

    #region Animation
    private void OnUnsheatheAnimationEnded() {
        swordState.Value = SwordState.InHand;
        unsheatheMotionTime = 1;
    }

    private void OnSheatheAnimationEnded() {
        swordState.Value = SwordState.InBack;
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
        animatorSwordLayerTargetWeight.Value = targetWeight;
        while(animatorSwordLayerWeight != animatorSwordLayerTargetWeight.Value) {
            if(animatorSwordLayerWeight < animatorSwordLayerTargetWeight.Value) {
                animatorSwordLayerWeight = Mathf.Min(animatorSwordLayerWeight + speed * Time.deltaTime, 1);
            }else if(animatorSwordLayerWeight > animatorSwordLayerTargetWeight.Value) {
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
        /*RestoreSwordRecord(previousSwordRecord,nextSwordRecord,  previousRecordDeltaTime, elapsedTimeSinceLastRecord);
        swordState.Value = previousSwordRecord.swordState;
        SheathingEnabled = previousSwordRecord.sheathingEnabled;
        UnsheathingEnabled = previousSwordRecord.unsheathingEnabled;

        if (animationCoroutine != null) {//coroutine hadn't ended when time rewind started
            if (previousSwordRecord.swordState == SwordState.Unsheathing) {
                animationCoroutine = StartCoroutine(UpdateUnsheatheAnimation());
            } else if (previousSwordRecord.swordState == SwordState.Sheathing) {
                animationCoroutine = StartCoroutine(UpdateSheatheAnimation());
            }
        }

        if (layerTransitionCoroutine != null) {
            layerTransitionCoroutine = StartCoroutine(UpdateSwordLayerWeightOverTime(previousSwordRecord.animatorSwordLayerTargetWeight, 
                                                                                     transitionSpeedIntoSwordLayer));
        }*/
    }

    public void OnAfterRewindingVariablesOnRewindStop() {
        // Animator parameters and layer weight are recorded and rewinded in AnimationTimeControl class, update these variables values
        unsheatheMotionTime = animator.GetFloat(AnimatorUtils.unsheatheMotionTimeHash);
        animatorSwordLayerWeight = animator.GetLayerWeight(swordAnimatorLayer);

        if (animationCoroutine != null) {//coroutine hadn't ended when time rewind started
            if (swordState.Value == SwordState.Unsheathing) {
                animationCoroutine = StartCoroutine(UpdateUnsheatheAnimation());
            } else if (swordState.Value == SwordState.Sheathing) {
                animationCoroutine = StartCoroutine(UpdateSheatheAnimation());
            }
        }

        if (layerTransitionCoroutine != null) {
            layerTransitionCoroutine = StartCoroutine(UpdateSwordLayerWeightOverTime(animatorSwordLayerTargetWeight.Value,
                                                                                     transitionSpeedIntoSwordLayer));
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
        return new SwordRecord(SheathingEnabled, UnsheathingEnabled, unsheatheMotionTime, animatorSwordLayerWeight,
                               animatorSwordLayerTargetWeight.Value, swordState.Value, transform.parent, 
                               hitboxEnabled.Value, attackCooldownRemainingTime.Value);
    }

    public void RestoreSwordRecord(SwordRecord previousRecord, SwordRecord nextRecord,float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        /*float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;

        unsheatheMotionTime = Mathf.Lerp(previousRecord.unsheatheMotionTime, nextRecord.unsheatheMotionTime, lerpAlpha);
        animator.SetFloat(AnimatorUtils.unsheatheMotionTimeHash, unsheatheMotionTime);

        animatorSwordLayerWeight = Mathf.Lerp(previousRecord.animatorSwordLayerWeight, nextRecord.animatorSwordLayerWeight, lerpAlpha);
        animator.SetLayerWeight(swordAnimatorLayer, animatorSwordLayerWeight);

        transform.SetParent(previousRecord.swordSocket, false);

        hitboxEnabled.Value = previousRecord.hitboxEnabled;

        attackCooldownRemainingTime.Value = Mathf.Lerp(previousRecord.attackCooldownRemainingTime, nextRecord.attackCooldownRemainingTime, lerpAlpha);*/
    }

    #endregion

    #region Hit detection
    public void SetHitboxEnabled(Bool enabled) {

        //Debug.Log(owner.name+ " hitbox Enabled: " + enabled);
        hitboxEnabled.Value = Convert.ToBoolean((int)enabled);
    }

    public HitData[] CheckHit() {
        if (hitboxEnabled.Value) { 
            return hitbox.CheckHit(ignoredHittableObjects);
        } else {
            return null;
        }
    }
    #endregion

    private void Update() {
        attackCooldownRemainingTime.Value = Mathf.Max(attackCooldownRemainingTime.Value - Time.deltaTime, 0);
    }

    public void StartCooldown() {
        attackCooldownRemainingTime.Value = attackCooldown;
    }

    public bool CooldownFinished() {
        return attackCooldownRemainingTime.Value <= 0;
    }

    
}
