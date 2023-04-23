using HFSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockState : State {

    [Serializable]
    public class BlockSettings {
        public Animator Animator { get; set; }
    }

    private BlockSettings settings;
    public BlockState(BlockSettings settings) {
        this.settings = settings;
    }

    protected override void OnEnter() {
        settings.Animator.SetTrigger(AnimatorUtils.blockHash);
    }
}