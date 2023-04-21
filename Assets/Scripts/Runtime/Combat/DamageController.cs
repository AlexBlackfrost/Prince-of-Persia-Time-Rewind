using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageController: MonoBehaviour, IDamageable{
    public Action<float> DamageReceived;
    

    public void ReceiveDamage(float amount) {

        // TODO conditions check if already triggered this frame, parry and blocks
        // TODO calculate damage, use scriptable objects for weapons stats I guess
        float someDamage = 1;
        DamageReceived.Invoke(someDamage);
    }


}