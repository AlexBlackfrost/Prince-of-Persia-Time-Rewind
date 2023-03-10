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

    }

    public bool WasAttackPressedInLastSeconds(float seconds) {
        bool wasAttackPressed = false;
        for(int i = index; i != index; i %= ++i) {
            if (array[i].pressed) {
                wasAttackPressed = true;
                break;
            }

            if(array[i].time < seconds) {
                break;
            }
        }
        return wasAttackPressed;
    }
}