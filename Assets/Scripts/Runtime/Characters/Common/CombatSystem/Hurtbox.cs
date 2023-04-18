using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hurtbox : MonoBehaviour{
    public Action<Hurtbox, Collider> TriggerEntered;
    [field:SerializeField] public string Name { get; private set; }

    private void OnTriggerEnter(Collider other) {
        TriggerEntered.Invoke(this, other);
    }
}