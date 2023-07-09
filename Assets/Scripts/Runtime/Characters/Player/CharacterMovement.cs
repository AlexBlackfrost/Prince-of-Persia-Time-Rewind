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
    public RewindableTransform Transform {get;set;}
    public Vector3 Velocity {
        get {
            return velocity;
        }
        set {
            velocity = value;
        }
    }
    private Vector3 velocity;

    public void Move(Vector3 direction) {
        velocity += direction.normalized * acceleration * Time.deltaTime;
        Vector2 clampedVelocity = Vector2.ClampMagnitude(velocity.XZ(), maxSpeed);
        velocity.x = clampedVelocity.x;
        velocity.z = clampedVelocity.y;
        ApplyGravity();

        CharacterController.Move(velocity * Time.deltaTime);

        if(direction.magnitude> float.Epsilon) {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(velocity.x, 0.0f, velocity.z));
            Transform.rotation = Quaternion.Slerp(Transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void Move(Vector3 direction, float speed) {
        velocity = direction * speed;
        ApplyGravity();
        CharacterController.Move(velocity * Time.deltaTime);
    }

    public void MoveAmount(Vector3 displacement) {
        velocity = displacement/Time.deltaTime;
        CharacterController.Move(displacement);
    }

    public void SetPosition(Vector3 position) {
        Vector3 delta = position - Transform.position;
        CharacterController.Move(delta);
    }

    public void SetRotation(Quaternion rotation) {
        Transform.rotation = rotation;
    }

    public void ApplyGravity() {
        if (CharacterController.isGrounded) {
            velocity.y = -0.01f;
        } else {
            velocity.y += gravity * Time.deltaTime;
        }
    }

    public bool IsGrounded() {
        return CharacterController.isGrounded;
    }
}

