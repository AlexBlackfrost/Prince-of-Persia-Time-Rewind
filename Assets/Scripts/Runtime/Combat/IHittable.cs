using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHittable{
    public Transform GetTransform();
    public void Hit();
}