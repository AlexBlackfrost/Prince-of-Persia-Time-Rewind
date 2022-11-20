using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimatorUtils  {
    public static event Action<int> AnimationEnded;

    public static void NotifyAnimationEnded(this Animator animator, int shortNameHash) {
        AnimationEnded.Invoke(shortNameHash);
    }
}

