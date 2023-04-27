using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyAI {
    [SerializeField] private float damagedFrequencyTriggerBlock = 1;

    private CircularStack<DateTime> damagedTimeStamps;
    private const int MAX_DAMAGED_TIMESTAMPS = 15;
    public bool DamagedTooOften { get; private set; }
    public bool HasBeenAttacked { get; private set; }

    private DateTime timeRewindStartDate;
    private DateTime now;

    public void Init() {
        damagedTimeStamps = new CircularStack<DateTime>(MAX_DAMAGED_TIMESTAMPS);
        DamagedTooOften = false;
        HasBeenAttacked = false;
    }

    public void OnDamageReceived(float amount) {
        HasBeenAttacked = true;

        DateTime now = DateTime.Now;
        if (!damagedTimeStamps.IsEmpty()) {
            DateTime lastDamagedTime = damagedTimeStamps.Peek();
            if (now.Subtract(lastDamagedTime).TotalSeconds < damagedFrequencyTriggerBlock) {
                DamagedTooOften = true;
            }
        }

        damagedTimeStamps.Push(now);
    }

    public void ResetDamagedTooOften() {
        DamagedTooOften = false;
    }

    public void OnTimeRewindStart() {
        timeRewindStartDate = DateTime.Now;

    }

    public void OnTimeRewindStop(EnemyAIRecord previousRecord, EnemyAIRecord nextRecord, float rewindSpeed, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        double elapsedTimeRewinding = DateTime.Now.Subtract(timeRewindStartDate).TotalSeconds * rewindSpeed;
        now = timeRewindStartDate.AddSeconds(-elapsedTimeRewinding);
        while(!damagedTimeStamps.IsEmpty() && damagedTimeStamps.Peek() > now) {
            damagedTimeStamps.Pop();
        }
        RestoreEnemyAIRecord(previousRecord, nextRecord, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
        
    }

    public EnemyAIRecord RecordEnemyAIData() {
        return new EnemyAIRecord(HasBeenAttacked, DamagedTooOften);
    }

    public void RestoreEnemyAIRecord(EnemyAIRecord previousRecord, EnemyAIRecord nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {

    }
}