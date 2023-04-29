using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyAI {
    [Header("Block")]
    [Tooltip("AI will try to block if they have received more than @damagedAmountTriggerBlock damage in the last @damagedLookbackTimeTriggerBlock seconds")]
    [SerializeField] private float recentDamageLookbackTimeTriggerBlock = 2;
    [SerializeField] private float recentDamageAmountTriggerBlock = 30;

    private CircularStack<(float,DateTime)> damagedTimeStamps;
    private const int MAX_DAMAGED_TIMESTAMPS = 15;
    public bool ReceivedTooMuchDamageRecently { get; private set; }
    public bool HasBeenAttacked { get; private set; }

    public void Init() {
        damagedTimeStamps = new CircularStack<(float,DateTime)>(MAX_DAMAGED_TIMESTAMPS);
        ReceivedTooMuchDamageRecently = false;
        HasBeenAttacked = false;
    }

    public void OnDamageReceived(float amount) {
        HasBeenAttacked = true;
        damagedTimeStamps.Push( (amount,TimeRewindManager.Now) );
        if (HasReceivedTooMuchDamagedRecently()) {
            ReceivedTooMuchDamageRecently = true;
        }

    }
     
    public void ResetReceivedTooMuchDamageRecently() {
        ReceivedTooMuchDamageRecently = false;
    }

    public void OnTimeRewindStart() {

    }

    public void OnTimeRewindStop(EnemyAIRecord previousRecord, EnemyAIRecord nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        while(!damagedTimeStamps.IsEmpty() && damagedTimeStamps.Peek().Item2 > TimeRewindManager.Now) {
            damagedTimeStamps.Pop();
        }
        RestoreEnemyAIRecord(previousRecord, nextRecord, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
    }

    public EnemyAIRecord RecordEnemyAIData() {
        return new EnemyAIRecord(HasBeenAttacked, ReceivedTooMuchDamageRecently);
    }

    public void RestoreEnemyAIRecord(EnemyAIRecord previousRecord, EnemyAIRecord nextRecord, float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        HasBeenAttacked = previousRecord.hasBeenAttacked;
        ReceivedTooMuchDamageRecently = previousRecord.receivedTooMuchDamageRecently;
    }

    private bool HasReceivedTooMuchDamagedRecently() {
        bool receivedTooMuchDamageRecently = false;
        float recentAccumulatedDamaged = 0;
        double currentAccumulatedTime = 0;
        int peekDepth = 0;

        DateTime lastDamagedTime = damagedTimeStamps.Peek().Item2;
        while(peekDepth < damagedTimeStamps.Count && currentAccumulatedTime < recentDamageLookbackTimeTriggerBlock && 
              recentAccumulatedDamaged < recentDamageAmountTriggerBlock) {

            (float, DateTime) damagedTimestamp = damagedTimeStamps.Peek(peekDepth);
            recentAccumulatedDamaged += damagedTimestamp.Item1;
            currentAccumulatedTime = lastDamagedTime.Subtract(damagedTimestamp.Item2).TotalSeconds;

            if(recentAccumulatedDamaged >= recentDamageAmountTriggerBlock && currentAccumulatedTime <= recentDamageLookbackTimeTriggerBlock) {
                receivedTooMuchDamageRecently = true;
            }

            peekDepth++;
        }
        return receivedTooMuchDamageRecently;
    }
}