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
    private RewindableVariable<Vector3> velocity;
    public Vector3 Velocity {
        get {
            return velocity.Value;
        }
        set {
            velocity.Value = value;
        }
    }

    public void Init() {
         velocity = new RewindableVariable<Vector3>();
#if UNITY_EDITOR
        velocity.Name = "CharacterMovementVelocity"+Transform.Value.gameObject.name;
#endif
    }

    public void Move(Vector3 direction) {
        velocity.Value += direction.normalized * acceleration * Time.deltaTime;
        Vector2 clampedVelocity = Vector2.ClampMagnitude(velocity.Value.XZ(), maxSpeed);
        velocity.Value = new Vector3(clampedVelocity.x, velocity.Value.y, clampedVelocity.y);
        ApplyGravity();

        CharacterController.Move(velocity.Value * Time.deltaTime);

        if(direction.magnitude> float.Epsilon) {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(velocity.Value.x, 0.0f, velocity.Value.z));
            Transform.rotation = Quaternion.Slerp(Transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    public void Move(Vector3 direction, float speed) {
        velocity.Value = direction * speed;
        ApplyGravity();
        CharacterController.Move(velocity.Value * Time.deltaTime);
    }

    public void MoveAmount(Vector3 displacement) {
        velocity.Value = displacement/Time.deltaTime;
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
            Vector3 currentVelocity = velocity.Value;
            currentVelocity.y = -0.01f;
            velocity.Value = currentVelocity;
        } else {
            Vector3 currentVelocity = velocity.Value;
            currentVelocity.y += gravity * Time.deltaTime;
            velocity.Value = currentVelocity;
        }
    }

    public bool IsGrounded() {
        return CharacterController.isGrounded;
    }
}

