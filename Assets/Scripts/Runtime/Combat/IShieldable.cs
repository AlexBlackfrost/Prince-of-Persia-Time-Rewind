using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IShieldable {
    public Action<GameObject> Parry { get; set; }
    public bool IsShielded();

}