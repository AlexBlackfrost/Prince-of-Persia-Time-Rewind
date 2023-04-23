using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthTimeControl {
    private Health health;
    
    public HealthTimeControl(Health health) {
        this.health = health;
    }

    public void OnTimeRewindStart() { }
    public void OnTimeRewindStop(HealthRecord previousRecord, HealthRecord nextRecord, float previousRecordDeltaTime, 
                                 float elapsedTimeSinceLastRecord) {

        RestoreHealthRecord(previousRecord, nextRecord, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
    }

    public void RestoreHealthRecord(HealthRecord previousRecord, HealthRecord nextRecord,
                                               float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {

        /*float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;

        float currentHealth = Mathf.Lerp(previousRecord.currentHealth,
                                           nextRecord.currentHealth,
                                           lerpAlpha);*/
        float currentHealth = previousRecord.currentHealth;
        health.CurrentHealth = currentHealth;
    }

    public HealthRecord RecordHealthData() {
        return new HealthRecord(health.CurrentHealth);
    }
}