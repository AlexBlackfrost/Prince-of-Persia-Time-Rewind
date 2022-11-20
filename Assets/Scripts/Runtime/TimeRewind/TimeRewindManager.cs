using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRewindManager : MonoBehaviour {

    private static TimeRewindManager instance;
    public static event Action TimeRewindStart;
    public static event Action TimeRewindStop;

    public static TimeRewindManager Instance {
        get {
            if (instance == null) {
                GameObject timeRewindManager = new GameObject(typeof(TimeRewindManager).ToString());
                Instance = timeRewindManager.AddComponent<TimeRewindManager>();
            }
            return instance;
        }
        private set{
            instance = value;
        }
    }

    private bool isRewinding;

    public void StartRewind() {
        TimeRewindStart.Invoke();
        Debug.Log("Start rewind");
    }

    public void StopRewind() {
        TimeRewindStop.Invoke();
        Debug.Log("Stop rewind");
    }
}

