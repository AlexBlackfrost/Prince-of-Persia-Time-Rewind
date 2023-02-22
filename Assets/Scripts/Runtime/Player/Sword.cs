using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour {
    public Transform playerTransform;
    private float sinFrequency = 3;
    private float sinAmplitude = 0.1f;
    private float floorZOffset = 1.5f;
    private float speed =12;
    private float rotationSpeed = 10;
    private float followPlayerStopDistance = 2.5f;

    public Transform leftWing;
    public Transform rightWing;
    private float wingSpeed = 15f;
    private float wingSinFrequency = 10f;
    private float wingSinAmplitude = 1;

    private void Start() {

    }


    private void Update() {
        MoveSword();
        MoveWings();
    }

    private void MoveSword() {
        Vector3 playerDirection = (playerTransform.position - transform.position).normalized;

        // Follow player
        //if (Vector3.SqrMagnitude(playerTransform.position - transform.position) > followPlayerStopDistance * followPlayerStopDistance) {    
        transform.position += playerDirection * speed * Time.deltaTime;
        //}


        // Look at player
        Vector3 lookDirection = playerDirection;
        lookDirection.y = 0;
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        // Secondary up & down motion
        transform.position = new Vector3(transform.position.x, Mathf.Sin(Time.time * sinFrequency) * sinAmplitude + floorZOffset, transform.position.z);
    }

    private void MoveWings() {
        MoveWing(leftWing);
        MoveWing(rightWing);
    }

    private void MoveWing(Transform wingTransform) {
        Vector3 eulerAngles = wingTransform.transform.localRotation.eulerAngles;
        float sinValue = Mathf.Sin(Time.time * wingSinFrequency);
        eulerAngles.z = MathUtils.MapRangeClamped(sinValue, -1, 1, -1, -3);
        Quaternion targetRotation = Quaternion.Euler(eulerAngles);
        wingTransform.localRotation = Quaternion.Slerp(wingTransform.localRotation, targetRotation, wingSpeed * Time.deltaTime);
    }
}
