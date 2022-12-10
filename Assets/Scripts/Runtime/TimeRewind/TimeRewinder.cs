using HFSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TimeRewinder : MonoBehaviour {
    public Stack<PlayerRecord> records;

    private void Awake() {
        records = new Stack<PlayerRecord>();
    }
}

