using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AttackInput {
    public bool pressed;
    public float time;
    public AttackInput(bool pressed, float time) {
        this.pressed = pressed;
        this.time = time;
    }
}
public class AttackInputBuffer : CircularStack<AttackInput>{

    public AttackInputBuffer(int size):base(size) {
        TimeRewindManager.TimeRewindStop += ClearInputBuffer;
    }

    private void ClearInputBuffer() {
        Clear();
    }

    public bool WasAttackPressedInLastSeconds(float seconds) {
        bool wasAttackPressed = false;
        for(int i = index; i != (index+1) % array.Length; i = MathUtils.NonNegativeMod(i - 1, array.Length) ) {
            if (array[i].pressed) {
                wasAttackPressed = true;
                break;
            }

            if(Time.time - array[i].time > seconds) {
                break;
            }
        }
        return wasAttackPressed;
    }
}