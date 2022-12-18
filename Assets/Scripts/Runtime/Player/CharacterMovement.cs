using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[Serializable] public class CharacterMovement {
    [SerializeField] private float maxSpeed = 8;
    [SerializeField] private float rotationSpeed = 12;
    [SerializeField] private float acceleration = 500f;
    [SerializeField] private float gravity = -40f;
    public CharacterController CharacterController { get; set; }
    public Transform Transform {get;set;}

    private Vector3 velocity;

    public void Move(Vector3 direction) {
        velocity += direction.normalized * acceleration * Time.deltaTime;
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        ApplyGravity();

        CharacterController.Move(velocity * Time.deltaTime);

        Vector3 velocityXZ = new Vector3(velocity.x, 0, velocity.z);
        Quaternion targetRotation = Quaternion.LookRotation(velocityXZ);
        Transform.rotation = Quaternion.Slerp(Transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void ApplyGravity() {
        if (CharacterController.isGrounded) {
            velocity.y = -0.01f;
        } else {
            velocity.y += gravity * Time.deltaTime;
        }
    }

}

