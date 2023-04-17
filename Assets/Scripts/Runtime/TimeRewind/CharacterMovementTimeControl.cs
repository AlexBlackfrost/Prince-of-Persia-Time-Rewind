using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovementTimeControl{

    private CharacterMovement characterMovement;
    public CharacterMovementTimeControl(CharacterMovement characterMovement) {
        this.characterMovement = characterMovement;
    }

    public void OnTimeRewindStart() {

    }

    public void OnTimeRewindStop() {

    }

    public void RestoreCharacterMovementRecord(CharacterMovementRecord previousRecord, CharacterMovementRecord nextRecord, 
                                               float previousRecordDeltaTime, float elapsedTimeSinceLastRecord) {

        float lerpAlpha = elapsedTimeSinceLastRecord / previousRecordDeltaTime;
        Vector3 velocity = Vector3.Lerp(previousRecord.velocity,
                                        nextRecord.velocity,
                                        lerpAlpha);
        characterMovement.Velocity = velocity;
    }

    public CharacterMovementRecord RecordCharacterMovementData() {
        return new CharacterMovementRecord(characterMovement.Velocity);;
    }
}