using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRewindManager : MonoBehaviour {

    private static TimeRewindManager instance;
    public static event Action TimeRewindStart;
    public static event Action TimeRewindStop;
    public static bool IsRewinding;

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


    public static void StartTimeRewind() {
        TimeRewindStart.Invoke();
        IsRewinding = true;
        Debug.Log("Start rewind");
    }

    public static void StopTimeRewind() {
        TimeRewindStop.Invoke();
        IsRewinding = false;
        Debug.Log("Stop rewind");
    }
}

