using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TimeRewindManager : MonoBehaviour {
    public event Action TimeRewindStart;
    public event Action TimeRewindStop;
    public bool IsRewinding;
    public float RewindSpeed = 0.1f;

    //private DateTime startRewindTimestamp;
    private double totalElapsedTimeRewinding; // Realtime elapsed rewinding (pressing the rewind button)
    private double totalRewindedTime; // Time traveled to the past, accounts for rewindSpeed
    private DateTime gameStartDate;
#if UNITY_EDITOR
    private float elapsedEditorPausedTime;
    private DateTime lastEditorPauseStartDate;
#endif

    private static TimeRewindManager instance;
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
    public  DateTime Now {
        get {
            /* Account for the time elapsed while rewinding too, since DateTime won't stop while rewinding.
             * Since RewindSpeed doesn't need to be 1, it has to be tracked independently
             */
            float offsetTime = (float) (totalElapsedTimeRewinding + totalRewindedTime);
#if UNITY_EDITOR
            offsetTime += elapsedEditorPausedTime;
#endif
            return DateTime.Now.AddSeconds(-offsetTime);
        }
        private set {
            Now = value;
        }
    }

    private void Awake() {
        gameStartDate = DateTime.Now;
#if UNITY_EDITOR
        EditorApplication.pauseStateChanged += OnPauseStateChanged;
        EditorApplication.
#endif
    }

#if UNITY_EDITOR
    private void OnPauseStateChanged(PauseState pauseState) {
        if(pauseState == PauseState.Paused) {
            lastEditorPauseStartDate = DateTime.Now;
        }else if(pauseState == PauseState.Unpaused) {
            elapsedEditorPausedTime += (float)(DateTime.Now - lastEditorPauseStartDate).TotalSeconds;
        }
    }
#endif

    private void Update() {
        if (IsRewinding) {
            totalElapsedTimeRewinding += Time.deltaTime;
            totalRewindedTime += Time.deltaTime * RewindSpeed;
        }

    }

    /// <summary>
    /// Returns the seconds elapsed since the game started taking into account time travel.
    /// </summary>
    /// <returns>Seconds elapsed since the game started taking into account time travel</returns>
    public double SecondsSinceStart() {
         return (Now - gameStartDate).TotalSeconds;
    }

    public void StartTimeRewind() {
        IsRewinding = true;
        //startRewindTimestamp = DateTime.Now;
        TimeRewindStart.Invoke();
        Debug.Log("Start rewind");
    }

    public void StopTimeRewind() {
        IsRewinding = false;
        //totalElapsedTimeRewinding += DateTime.Now.Subtract(startRewindTimestamp).TotalSeconds;
        //totalRewindedTime += DateTime.Now.Subtract(startRewindTimestamp).TotalSeconds * RewindSpeed;
        TimeRewindStop.Invoke();
        Debug.Log("Stop rewind");
    }
}

