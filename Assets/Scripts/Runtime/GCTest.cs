using HFSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GCTest : MonoBehaviour{
    private int size = 1200;
    private int numCharacters = 200;
    private void Start(){
        Test2();
    }

    private void Test1() {
        for (int j = 0; j < numCharacters; j++) {
            CircularStack<PlayerRecord> stack = new CircularStack<PlayerRecord>(size);
            for (int i = 0; i < size; i++) {
                CameraRecord cameraRecord = new CameraRecord();
                TransformRecord transformRecord = new TransformRecord();
                AnimationRecord animationRecord = new AnimationRecord();
                StateMachineRecord stateMachineRecord = new StateMachineRecord();
                SwordRecord swordRecord = new SwordRecord();
                HealthRecord healthRecord = new HealthRecord();
                HurtboxRecord hurtboxRecord = new HurtboxRecord();
                CharacterMovementRecord characterMovementRecord = new CharacterMovementRecord();
                PlayerRecord playerRecord = new PlayerRecord(transformRecord, cameraRecord, animationRecord, stateMachineRecord,
                                                             characterMovementRecord, swordRecord, healthRecord, hurtboxRecord, Time.deltaTime);
                stack.Push(playerRecord);
            }
        }
    }//42.1MB

    private void Test2() {
        
        for(int j=0;j< numCharacters; j++) {
            CircularStack<AnimationRecord> animationStack = new CircularStack<AnimationRecord>(size);
            CircularStack<TransformRecord> transformStack = new CircularStack<TransformRecord>(size);
            CircularStack<CameraRecord> cameraStack = new CircularStack<CameraRecord>(size);
            CircularStack<StateMachineRecord> stateMachineStack = new CircularStack<StateMachineRecord>(size);
            CircularStack<SwordRecord> swordStack = new CircularStack<SwordRecord>(size);
            CircularStack<HealthRecord> healthStack = new CircularStack<HealthRecord>(size);
            CircularStack<HurtboxRecord> hurtboxStack = new CircularStack<HurtboxRecord>(size);
            CircularStack<CharacterMovementRecord> characterMovementStack = new CircularStack<CharacterMovementRecord>(size);
            for (int i = 0; i < size; i++) {
                CameraRecord cameraRecord = new CameraRecord();
                cameraStack.Push(cameraRecord);

                TransformRecord transformRecord = new TransformRecord();
                transformStack.Push(transformRecord);

                AnimationRecord animationRecord = new AnimationRecord();
                animationStack.Push(animationRecord);

                StateMachineRecord stateMachineRecord = new StateMachineRecord();
                stateMachineStack.Push(stateMachineRecord);

                SwordRecord swordRecord = new SwordRecord();
                swordStack.Push(swordRecord);

                HealthRecord healthRecord = new HealthRecord();
                healthStack.Push(healthRecord);

                HurtboxRecord hurtboxRecord = new HurtboxRecord();
                hurtboxStack.Push(hurtboxRecord);

                CharacterMovementRecord characterMovementRecord = new CharacterMovementRecord();
                characterMovementStack.Push(characterMovementRecord);
                
            }
        }
    }//49.5mb

}