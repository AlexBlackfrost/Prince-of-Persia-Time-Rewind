using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

public class Sword : MonoBehaviour {
    public bool CanBeUnsheathed { get; set; } = false;
    public bool CanBeSheathed { get; set; } = false;
    public bool IsInHand { get; private set; } = false;

    private Animator animator;
    private int unsheatheHash;
    private int sheatheHash;

    private void Awake() {
        animator = GetComponent<Animator>();
        unsheatheHash = Animator.StringToHash("Unsheathe");
        sheatheHash = Animator.StringToHash("Sheathe");
        AnimatorUtils.AnimationEnded += OnAnimationEnded;
    }

    private void Unsheathe() {
        animator.SetTrigger(unsheatheHash);
        CanBeSheathed = true;
    }

    private void Sheathe() {
        animator.SetTrigger(sheatheHash);
        CanBeSheathed = false;
    }

    private void OnAnimationEnded(int stateHash) {
        if(stateHash == unsheatheHash) {
            OnUnsheatheAnimationEnded();
        }else if(stateHash == sheatheHash) {
            OnSheatheAnimationEnded();
        }
    }

    private void OnUnsheatheAnimationEnded() {
        IsInHand = true;
    }

    private void OnSheatheAnimationEnded() {
        IsInHand = false;
    }

    public void OnSheathePressed() {
        if (IsInHand && CanBeSheathed) {
            Sheathe();
        }
    }

    public void OnUnsheathePressed() {
        if (!IsInHand && CanBeUnsheathed) {
            Unsheathe();
        }
    }
}
