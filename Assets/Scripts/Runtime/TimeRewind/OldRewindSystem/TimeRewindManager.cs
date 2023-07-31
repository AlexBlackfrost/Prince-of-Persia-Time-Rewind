using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeRewindManager : MonoBehaviour {
    
    public static DateTime Now {
        get {
            /* Account for the time elapsed while rewinding too, since DateTime won't stop while rewinding.
             * Since RewindSpeed doesn't need to be 1, it has to be tracked independently
             */
            return DateTime.Now.AddSeconds(-(totalElapsedTimeRewinding + totalRewindedTime)); 
        }
        private set {
            Now = value;
        }
    }
    public static event Action TimeRewindStart;
    public static event Action TimeRewindStop;
    public static bool IsRewinding;
    public static float RewindSpeed = 0.1f;

    private static TimeRewindManager instance;
    private static DateTime startRewindTimestamp;
    private static double totalElapsedTimeRewinding;
    private static double totalRewindedTime;

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
        startRewindTimestamp = DateTime.Now;
        TimeRewindStart.Invoke();
        IsRewinding = true;
        Debug.Log("Start rewind");
    }

    public static void StopTimeRewind() {
        totalElapsedTimeRewinding += DateTime.Now.Subtract(startRewindTimestamp).TotalSeconds;
        totalRewindedTime += DateTime.Now.Subtract(startRewindTimestamp).TotalSeconds * RewindSpeed;
        TimeRewindStop.Invoke();
        IsRewinding = false;
        Debug.Log("Stop rewind");
    }
}

