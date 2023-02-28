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

    private Animator animator;
    private int unsheatheHash;
    private int unsheatheSpeedMultiplierHash;
    private int unsheatheMotionTimeHash;
    private float unsheatheMotionTime;
    private float sheatheAnimationSpeed = 1.5f;
    private float unsheatheAnimationSpeed = 1.5f;
    private int swordAnimatorLayer = 1;
    private float animatorSwordLayerWeight = 0.0f;
    private float transitionSpeedIntoSwordLayer = 10f;
    private float transitionSpeedOutOfSwordLayer = 2.5f;
    private Coroutine animationCoroutine;
    private Coroutine layerTransitionCoroutine;
    private SwordState swordState = SwordState.InBack;

    private void Awake() {
        animator = GetComponent<Animator>();
        unsheatheHash = Animator.StringToHash("Unsheathe");
        unsheatheSpeedMultiplierHash = Animator.StringToHash("UnsheatheSpeedMultiplier");
        unsheatheMotionTimeHash = Animator.StringToHash("UnsheatheMotionTime");
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
                StopCoroutine(animationCoroutine);
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
                StopCoroutine(animationCoroutine);
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
        StopCoroutine(layerTransitionCoroutine);
        layerTransitionCoroutine = StartCoroutine(UpdateSwordLayerWeightOverTime(0, transitionSpeedOutOfSwordLayer));
    }

    private IEnumerator UpdateUnsheatheAnimation() {
        while (unsheatheMotionTime < 1) {
            unsheatheMotionTime = Mathf.Min(unsheatheMotionTime + Time.deltaTime * unsheatheAnimationSpeed, 1);
            animator.SetFloat(unsheatheMotionTimeHash, unsheatheMotionTime);
            if (unsheatheMotionTime == 1.0f) {
                OnUnsheatheAnimationEnded();
            }
            yield return null;
        }
    }

    private IEnumerator UpdateSheatheAnimation() {
        while (unsheatheMotionTime > 0) { 
            unsheatheMotionTime = Mathf.Max(unsheatheMotionTime - Time.deltaTime * sheatheAnimationSpeed, 0);
            animator.SetFloat(unsheatheMotionTimeHash, unsheatheMotionTime);
            if (unsheatheMotionTime == 0.0f) {
                OnSheatheAnimationEnded();
            }
            yield return null;
        }
    }

    private IEnumerator UpdateSwordLayerWeightOverTime(float targetWeight, float speed) {
        while(animatorSwordLayerWeight != targetWeight) {
            if(animatorSwordLayerWeight < targetWeight) {
                animatorSwordLayerWeight = Mathf.Min(animatorSwordLayerWeight + speed * Time.deltaTime, 1);
            }else if(animatorSwordLayerWeight > targetWeight) {
                animatorSwordLayerWeight = Mathf.Max(animatorSwordLayerWeight - speed * Time.deltaTime, 0);
            }
            animator.SetLayerWeight(swordAnimatorLayer, animatorSwordLayerWeight);
            yield return null;
        }
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
}
