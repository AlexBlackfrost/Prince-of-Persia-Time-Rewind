using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTimeRewinder : TimeRewinder<PlayerRecord>{
    [SerializeField] private int maxSandTanks = 4;

    public Action<int> SandTankConsumed;
    public Action<int> SandTankRestored;
    public Action<int> SandTanksInitialized;

    private int availableSandTanks;

    private new void Awake() {
        base.Awake();
        availableSandTanks = maxSandTanks;
    }

    private void Start() {
        SandTanksInitialized(availableSandTanks);
    }

    public bool HasSandTanks() {
        return availableSandTanks > 0; 
    }

    public void ConsumeSandTank() {
        SandTankConsumed?.Invoke(availableSandTanks);
        availableSandTanks--;
    }

}