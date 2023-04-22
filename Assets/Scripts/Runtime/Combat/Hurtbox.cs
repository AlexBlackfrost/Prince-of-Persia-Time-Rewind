using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox: MonoBehaviour, IDamageable, IHittable {
    public Action<float> DamageReceived;
    public Action<float> HitReceived;

    public void Hit() {

    }

    public void ReceiveDamage(float amount) {
        DamageReceived.Invoke(amount);
    }


}