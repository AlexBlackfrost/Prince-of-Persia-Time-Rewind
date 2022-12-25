using HFSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TimeRewinder : MonoBehaviour {
    public CircularStack<PlayerRecord> records;
    private int fps = 60;
    private int seconds = 20;
    private void Awake() {
        records = new CircularStack<PlayerRecord>(fps * seconds);
    }
}

