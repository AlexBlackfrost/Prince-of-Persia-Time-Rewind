using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimatorUtils  {
    public static event Action<int> AnimationEnded;
    public static event Action<int> AnimationStarted;

    public static void NotifyAnimationEnded(this Animator animator, int shortNameHash) {
        AnimationEnded.Invoke(shortNameHash);
    }

    public static void NotifyAnimationStarted(this Animator animator, int shortNameHash) {
        AnimationStarted.Invoke(shortNameHash);
    }
}

