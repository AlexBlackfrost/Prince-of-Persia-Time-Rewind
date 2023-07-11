using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeTest : MonoBehaviour{
    RewindableVariable<float> flag;
    private float timer;
    private bool isRewinding;

    private void Start() {
        flag = new RewindableVariable<float>(2.0f);

        isRewinding = false;
        TimeRewindManager.TimeRewindStart += OnTimeRewindStart;
        TimeRewindManager.TimeRewindStop += OnTimeRewindStop;
    }
    private void OnTimeRewindStart() {
        isRewinding = true;
        RewindController.Instance.OnTimeRewindStart();
    }

    private void OnTimeRewindStop() {
        isRewinding = false;
        RewindController.Instance.OnTimeRewindStop();
    }
    private void Update() {
            flag.Value *= -1;
        if(timer < 2) {
            timer += Time.deltaTime;
        } else {
            timer = 0;
        }
    }

    private void LateUpdate() {
        if (isRewinding) {
            RewindController.Instance.Rewind(Time.deltaTime);
        } else{
            RewindController.Instance.RecordVariables();
        }
    }

}