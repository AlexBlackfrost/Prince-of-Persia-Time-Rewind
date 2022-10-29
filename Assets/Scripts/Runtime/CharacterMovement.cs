using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable] public class CharacterMovement {
    [SerializeField] private float maxSpeed = 8;
    [SerializeField] private float rotationSpeed = 12;
    [SerializeField] private float acceleration = 500f;
    public CharacterController CharacterController { get; set; }
    public Transform Transform {get;set;}

    private Vector3 velocity;

    public void Move(Vector3 direction) {
        velocity += direction.normalized * acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        CharacterController.Move(velocity * Time.deltaTime);

        Quaternion targetRotation = Quaternion.LookRotation(velocity);
        Transform.rotation = Quaternion.Slerp(Transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

}

