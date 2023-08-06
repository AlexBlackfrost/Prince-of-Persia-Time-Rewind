using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Stats : MonoBehaviour{
    private int N = 5;
    private float[] lastNFPS;
    private double[] lastNRewindTimes;
    private double[] lastNRecordTimes;
    private int currentFPSIndex = 0;
    private int currentRewindIndex = 0;
    private int currentRecordIndex = 0;
    private float averageFPS;
    private float minFPS = float.MaxValue;
    private static double accumulatedRecordTime;
    private static double accumulatedRewindTime;
    private double averageRecordTime;
    private double averageRewindTime;
    private float drawStatsUIInterval = 0.1f;
    private float timeSincelastUIUpdate;
    private float minFPSUI;
    private float averageFPSUI;
    private double averageRecordTimeUI;
    private double averageRewindTimeUI;



    public static void AddAccumulatedRecordTime(double time) {
        accumulatedRecordTime += time;
    }

    public static void AddAccumulatedRewindTime(double time) {
        accumulatedRewindTime += time;
    }

    private void Awake() {
        lastNFPS = new float[N];
        lastNRewindTimes = new double[N];
        lastNRecordTimes = new double[N];
    }

    private void Update(){
        CountFPS();
    }

    private void LateUpdate() {
        CalculateRecordTime();
        CalculateRewindTime();
        accumulatedRecordTime = 0;
        accumulatedRewindTime = 0;
        timeSincelastUIUpdate += Time.unscaledDeltaTime;
    }

    private void CountFPS() {
        float currentFPS = 1 / Time.unscaledDeltaTime;
        lastNFPS[currentFPSIndex] = currentFPS;
        currentFPSIndex = (currentFPSIndex + 1) % lastNFPS.Length;

        averageFPS = 0;
        foreach (float fps in lastNFPS) {
            averageFPS += fps;
        }
        averageFPS /= lastNFPS.Length;
    }

    private void CalculateRecordTime() {
        if (!TimeRewindManager.IsRewinding) {
            lastNRecordTimes[currentRecordIndex] = accumulatedRecordTime;
            currentRecordIndex = (currentRecordIndex + 1) % lastNRecordTimes.Length;
            averageRecordTime = 0;
            foreach(double recordTime in lastNRecordTimes) {
                averageRecordTime += recordTime;
            }
            averageRecordTime /= lastNRecordTimes.Length;
        }
    }

    private void CalculateRewindTime() {
        if (TimeRewindManager.IsRewinding) {
            averageRewindTime = accumulatedRewindTime;
            lastNRewindTimes[currentRewindIndex] = accumulatedRewindTime;
            currentRewindIndex = (currentRewindIndex + 1) % lastNRewindTimes.Length;
            foreach (double rewindTime in lastNRewindTimes) {
                averageRewindTime += rewindTime;
            }
            averageRewindTime /= lastNRewindTimes.Length;
        }
    }

    private void OnGUI() {
        if(timeSincelastUIUpdate > drawStatsUIInterval) {
            averageFPSUI = averageFPS;
            minFPSUI = minFPS;
            averageRecordTimeUI = averageRecordTime;
            averageRewindTimeUI = averageRewindTime;
            timeSincelastUIUpdate = 0;
        }
        GUI.Label(new Rect(5, 40, 500, 25), "Average FPS (last " + lastNFPS.Length + " frames): " + Mathf.Round(averageFPSUI));
        //GUI.Label(new Rect(5, 70, 100, 25), "Min FPS: " + Mathf.Round(minFPSUI));
        GUI.Label(new Rect(5, 70, 500, 25), "Average Record time (last " + lastNRecordTimes.Length + " frames) (ms): " + averageRecordTimeUI);
        GUI.Label(new Rect(5, 100, 500, 25), "Average Rewind time (last " + lastNRewindTimes.Length + " frames): (ms): " + averageRewindTimeUI);
    }

}
