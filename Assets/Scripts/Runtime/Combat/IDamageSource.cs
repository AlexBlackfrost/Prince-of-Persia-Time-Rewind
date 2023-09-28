using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageSource {
    GameObject DamageApplier { get; set; }
    void ApplyDamageEffect(GameObject damageReceiver);
}