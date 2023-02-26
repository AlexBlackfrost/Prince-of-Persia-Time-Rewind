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
    private int sheatheHash;
    private int unsheatheSpeedMultiplierHash;
    private int unsheatheMotionTimeHash;
    private int swordAnimatorLayer = 1;
    private float swordAnimatorLayerWeight = 0;
    private float animatorLayerTransitionSpeed = 1f;
    private SwordState swordState = SwordState.InBack;
    private float swordAnimatorLayerTargetWeight;
    private bool layerTransitionEnabled = false;
    private float unsheatheMotionTime;

    private void Awake() {
        animator = GetComponent<Animator>();
        unsheatheHash = Animator.StringToHash("Unsheathe");
        sheatheHash = Animator.StringToHash("Sheathe");
        unsheatheSpeedMultiplierHash = Animator.StringToHash("UnsheatheSpeedMultiplier");
        unsheatheMotionTimeHash = Animator.StringToHash("UnsheatheMotionTime");
        AnimatorUtils.AnimationEnded += OnAnimationEnded;
        //AnimatorUtils.AnimationStarted += OnAnimationStarted;
    }

    private void Update() {
        if (layerTransitionEnabled) {
            if(swordAnimatorLayerWeight < swordAnimatorLayerTargetWeight) {
                //swordAnimatorLayerWeight = Mathf.Min(swordAnimatorLayerWeight +100 * Time.deltaTime, 1);
            } else {
                //swordAnimatorLayerWeight = Mathf.Max(swordAnimatorLayerWeight - 0.1f * Time.deltaTime, 0.01f);
            }

            animator.SetLayerWeight(swordAnimatorLayer, swordAnimatorLayerWeight);
            if (swordAnimatorLayerWeight == swordAnimatorLayerTargetWeight) {
                layerTransitionEnabled = false;
            }
        }

        if(swordState == SwordState.Unsheathing) {
            unsheatheMotionTime = Mathf.Min(unsheatheMotionTime + Time.deltaTime * 1.5f, 1);
            animator.SetFloat(unsheatheMotionTimeHash, unsheatheMotionTime);
            if(unsheatheMotionTime == 1.0f) {
                OnUnsheatheAnimationEnded();
            }
        } else if(swordState == SwordState.Sheathing) {
            unsheatheMotionTime = Mathf.Max(unsheatheMotionTime - Time.deltaTime * 1.5f, 0);
            animator.SetFloat(unsheatheMotionTimeHash, unsheatheMotionTime);
            if (unsheatheMotionTime == 0.0f) {
                OnSheatheAnimationEnded();
            }
        }
    }

    public void SheatheIfPossible() {
        /*if (SheathingEnabled && (swordState == SwordState.InHand || swordState == SwordState.Unsheathing) ) {
            Sheathe();
        }*/
        if (SheathingEnabled) {
            if (swordState == SwordState.InHand) {
                animator.SetFloat(unsheatheSpeedMultiplierHash, 0);
                //animator.SetFloat(unsheatheMotionTimeHash, 1);
                animator.SetTrigger(unsheatheHash);
                swordState = SwordState.Sheathing;
            } else if(swordState == SwordState.Unsheathing) {
                animator.SetFloat(unsheatheSpeedMultiplierHash, 0);
                swordState = SwordState.Sheathing;
            }
        }
    }
    public void UnsheatheIfPossible() {
        /*if (UnsheathingEnabled &&  (swordState == SwordState.InBack || swordState == SwordState.Sheathing) ) {
            Unsheathe();
        }*/
        if (UnsheathingEnabled) {
            if (swordState == SwordState.InBack) {
                animator.SetFloat(unsheatheSpeedMultiplierHash, 0);
                animator.SetTrigger(unsheatheHash);
                swordState = SwordState.Unsheathing;
                animator.SetLayerWeight(swordAnimatorLayer, 1);
            } else if (swordState == SwordState.Sheathing) {
                animator.SetFloat(unsheatheSpeedMultiplierHash, 0);
                swordState = SwordState.Unsheathing;
                
            }
        }
    }

    private void Unsheathe() {
        swordState = SwordState.Unsheathing;
        animator.SetFloat(unsheatheSpeedMultiplierHash, 1);
        animator.SetTrigger(unsheatheHash);
        animator.SetLayerWeight(swordAnimatorLayer, 1.0f);
    }

    private void Sheathe() {
        animator.SetFloat(unsheatheSpeedMultiplierHash, -1);
        animator.SetTrigger(unsheatheHash);
        swordState = SwordState.Sheathing;
    }

    public void ParentSwordToHandSocket() {
        if(swordState == SwordState.Sheathing) {
            sword.SetParent(backSocket, false);
        }else if(swordState == SwordState.Unsheathing) {
            sword.SetParent(handSocket, false);
        }
        
    }

    public void ParentSwordToBackSocket() {
        if (swordState == SwordState.Sheathing) {
            sword.SetParent(handSocket, false);
        } else if (swordState == SwordState.Unsheathing) {
            sword.SetParent(backSocket, false);
        }
    }

    public bool IsInHand() {
        return swordState == SwordState.InHand;
    }

    #region Animation
    private void OnAnimationEnded(int stateHash) {
        /*
        if(stateHash == unsheatheHash) {
            OnUnsheatheAnimationEnded();
        }else if(stateHash == sheatheHash) {
            OnSheatheAnimationEnded();
        }*/
        if (stateHash == unsheatheHash) {
            if (swordState == SwordState.Unsheathing) {
                OnUnsheatheAnimationEnded();
            } else if (swordState == SwordState.Sheathing) {
                OnSheatheAnimationEnded();
            }
        }
    }

    private void OnUnsheatheAnimationEnded() {
        swordState = SwordState.InHand;
        unsheatheMotionTime = 1;
    }

    

    private void OnSheatheAnimationEnded() {
        swordState = SwordState.InBack;
        unsheatheMotionTime = 0;
        animator.SetLayerWeight(swordAnimatorLayer, 0.0f);
    }

    public void SetSwordAnimatorLayerWeightOverTime(float targetLayerWeight) {
        swordAnimatorLayerTargetWeight = targetLayerWeight;
        layerTransitionEnabled = true;
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
