using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HurtboxTimeControl {
    private Hurtbox hurtbox;
    public HurtboxTimeControl(Hurtbox hurtbox) {
        this.hurtbox = hurtbox;
    }

    public void OnTimeRewindStart() {

    }

    public void OnTimeRewindStop(HurtboxRecord previousRecord, HurtboxRecord nextRecord,
                                               float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {
        RestoreHurtboxRecord(previousRecord, nextRecord, previousRecordDeltaTime, elapsedTimeSinceLastRecord);
    }

    public void RestoreHurtboxRecord(HurtboxRecord previousRecord, HurtboxRecord nextRecord,
                                               float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {

        float isDamageableRemainingTime = Mathf.Lerp(previousRecord.isDamageableRemainingTime, nextRecord.isDamageableRemainingTime,
                                                     elapsedTimeSinceLastRecord / previousRecordDeltaTime);

        hurtbox.SetIsShielded(previousRecord.isShielded);
        hurtbox.IsDamageableRemainingTime = isDamageableRemainingTime; 
    }

    public HurtboxRecord RecordHurtboxData() {
        return new HurtboxRecord(hurtbox.IsDamageableRemainingTime, hurtbox.IsShielded());
    }
}