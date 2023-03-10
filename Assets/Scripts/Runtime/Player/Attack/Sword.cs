using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;


public class Sword : MonoBehaviour {
    [SerializeField] private Transform sword;
    [SerializeField] private Transform backSocket;
    [SerializeField] private Transform handSocket;

    public bool SheathingEnabled { get; set; } = true;
    public bool UnsheathingEnabled { get; set; } = true;

    public Action<bool> OnSetComboEnabled;

    private Animator animator;
    private int unsheatheHash;
    private int unsheatheSpeedMultiplierHash;
    private int unsheatheMotionTimeHash;
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

    private void Awake() {
        animator = GetComponent<Animator>();
        unsheatheHash = Animator.StringToHash("Unsheathe");
        unsheatheSpeedMultiplierHash = Animator.StringToHash("UnsheatheSpeedMultiplier");
        unsheatheMotionTimeHash = Animator.StringToHash("UnsheatheMotionTime");
        swordState = SwordState.InBack;
    }

    public void SheatheIfPossible() {
        if (SheathingEnabled) {
            if (swordState == SwordState.InHand) {
                animator.SetFloat(unsheatheSpeedMultiplierHash, 0);
                animator.SetTrigger(unsheatheHash);
                animationCoroutine = StartCoroutine(UpdateSheatheAnimation());
                swordState = SwordState.Sheathing;

            } else if(swordState == SwordState.Unsheathing) {
                animator.SetFloat(unsheatheSpeedMultiplierHash, 0);
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
                animator.SetFloat(unsheatheSpeedMultiplierHash, 0);
                animator.SetTrigger(unsheatheHash);
                animationCoroutine = StartCoroutine(UpdateUnsheatheAnimation());
                if (layerTransitionCoroutine != null) {
                    StopCoroutine(layerTransitionCoroutine);
                }
                layerTransitionCoroutine = StartCoroutine(UpdateSwordLayerWeightOverTime(1, transitionSpeedIntoSwordLayer));
                swordState = SwordState.Unsheathing;

            } else if (swordState == SwordState.Sheathing) {
                animator.SetFloat(unsheatheSpeedMultiplierHash, 0);
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
            sword.SetParent(backSocket, false);
        } else if (swordState == SwordState.Unsheathing) {
            sword.SetParent(handSocket, false);
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
            animator.SetFloat(unsheatheMotionTimeHash, unsheatheMotionTime);
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
            animator.SetFloat(unsheatheMotionTimeHash, unsheatheMotionTime);
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

    public void OnTimeRewindStop(SwordRecord previousSwordRecord, SwordRecord nextSwordRecord, float elapsedTimeSinceLastRecord, float previousRecordDeltaTime) {
        RestoreSwordRecord(previousSwordRecord,nextSwordRecord, elapsedTimeSinceLastRecord, previousRecordDeltaTime);
        swordState = previousSwordRecord.swordState;
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

    public SwordRecord SaveSwordRecord() {
        return new SwordRecord(SheathingEnabled, UnsheathingEnabled, unsheatheMotionTime, animatorSwordLayerWeight, animatorSwordLayerTargetWeight, swordState, sword.parent);
    }

    public void RestoreSwordRecord(SwordRecord previousSwordRecord, SwordRecord nextSwordRecord, float elapsedTimeSinceLastRecord, float previousRecordDeltaTime) {
        float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;

        unsheatheMotionTime = Mathf.Lerp(previousSwordRecord.unsheatheMotionTime, nextSwordRecord.unsheatheMotionTime, lerpAlpha);
        animator.SetFloat(unsheatheMotionTimeHash, unsheatheMotionTime);

        animatorSwordLayerWeight = Mathf.Lerp(previousSwordRecord.animatorSwordLayerWeight, nextSwordRecord.animatorSwordLayerWeight, lerpAlpha);
        animator.SetLayerWeight(swordAnimatorLayer, animatorSwordLayerWeight);

        sword.SetParent(previousSwordRecord.swordSocket, false);
    }

    #endregion
}
