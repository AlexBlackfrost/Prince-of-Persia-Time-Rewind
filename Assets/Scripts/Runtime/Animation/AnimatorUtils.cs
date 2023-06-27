using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimatorUtils  {
    public static event Action<int> AnimationEnded;
    public static event Action<int> AnimationStarted;

    public static int runHash = Animator.StringToHash("Run");
    public static int jumpHash = Animator.StringToHash("Jump");
    public static int fallHash = Animator.StringToHash("Fall");
    public static int landHash = Animator.StringToHash("Land");
    public static int strafeHash = Animator.StringToHash("Strafe");
    public static int strafeSideHash = Animator.StringToHash("StrafeSide");
    public static int strafeForwardHash = Animator.StringToHash("StrafeForward");
    public static int wallRunHash = Animator.StringToHash("WallRun");
    public static int wallRunDirectionHash = Animator.StringToHash("WallRunDirection");
    public static int wallRunRightHash = Animator.StringToHash("WallRunRight");
    public static int wallRunLeftHash = Animator.StringToHash("WallRunLeft");
    public static int blockHash = Animator.StringToHash("Block");
    public static int rollHash = Animator.StringToHash("Roll");
    public static int dieHash = Animator.StringToHash("Die");
    public static int attackHash = Animator.StringToHash("Attack");
    public static int attack1Hash = Animator.StringToHash("Attack1");
    public static int attack2Hash = Animator.StringToHash("Attack2");
    public static int attack3Hash = Animator.StringToHash("Attack3");
    public static int nextComboAttackHash = Animator.StringToHash("NextComboAttack");
    public static int damagedHash = Animator.StringToHash("Damaged");
    public static int parriedHash = Animator.StringToHash("Parried");
    public static int attackRecoveryHash = Animator.StringToHash("Recovery");
    public static int AIAttackHash = Animator.StringToHash("AIAttack");
    public static int attackWindUpHash = Animator.StringToHash("WindUp");
    
    public static int sheatheHash = Animator.StringToHash("Sheathe");
    public static int unsheatheHash = Animator.StringToHash("Unsheathe");
    public static int unsheatheSpeedMultiplierHash = Animator.StringToHash("UnsheatheSpeedMultiplier");
    public static int unsheatheMotionTimeHash = Animator.StringToHash("UnsheatheMotionTime");



    public static void NotifyAnimationEnded(this Animator animator, int shortNameHash) {
        AnimationEnded?.Invoke(shortNameHash);
    }

    public static void NotifyAnimationStarted(this Animator animator, int shortNameHash) {
        AnimationStarted?.Invoke(shortNameHash);
    }
}

