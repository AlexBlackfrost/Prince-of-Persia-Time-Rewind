using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindController {
    private struct RecordedData {
        public object[] recordedVariables;
        public BitArray recordedMask;
        public float deltaTime;

        public RecordedData(object[] recordedVariables, BitArray recordedMask, float deltaTime) {
            this.recordedVariables = recordedVariables;
            this.recordedMask = recordedMask;
            this.deltaTime = deltaTime;
        }

        public bool HasRecordedDataFor(int id) {
            return recordedMask != null && recordedMask[id];
        }

        public object GetRecordedDataFor(int id) {
            int index = -1;
            for (int i = 0; i <= id; i++) {
                if (recordedMask[i]) {
                    index++;
                }
            }
            return recordedVariables[index];
        }
    }

    public int MaxMaxFramesWithoutBeingRecorded { get; private set; }
    private static RewindController instance;
    private List<IRewindable> rewindableVariables;
    private int NUM_FRAMES = 1200;
    private int numModifiedVariablesThisFrame;
    private int numVariablesNotRecordedAtLeastOnce;
    private int numOutdatedVariables; // number of rewindable variables that haven't been recorded in a long time
    private CircularStack<RecordedData> circularStack;
    private RecordedData previousRecordedData, nextRecordedData;
    private float elapsedTimeSinceLastRecord;

    public static RewindController Instance {
        get {
            if (instance == null) {
                instance = new RewindController();
            }
            return instance;
        }
        private set {
            Instance = value;
        }
    }

    public RewindController() {
        rewindableVariables = new List<IRewindable>();
        circularStack = new CircularStack<RecordedData>(NUM_FRAMES);
        elapsedTimeSinceLastRecord = 0;
        numVariablesNotRecordedAtLeastOnce = 0;
        TimeRewindManager.TimeRewindStart += OnTimeRewindStart;
        TimeRewindManager.TimeRewindStop += OnTimeRewindStop;

    }

    public void OnTimeRewindStart() {
        previousRecordedData = circularStack.Pop();
        nextRecordedData = circularStack.Peek();
        elapsedTimeSinceLastRecord = 0;
        numOutdatedVariables = 0;
        numOutdatedVariables = 0;

        foreach(IRewindable rewindableVariable in rewindableVariables) {
            rewindableVariable.OnRewindStart();
        }
    }

    public void OnTimeRewindStop() {
        for (int id = 0; id < rewindableVariables.Count; id++) {
            (object previousRecord, object nextRecord) = GetPreviousAndNextRecord(id, previousRecordedData, nextRecordedData);
            rewindableVariables[id].OnRewindStop(previousRecord, nextRecord, previousRecordedData.deltaTime, elapsedTimeSinceLastRecord);
        }
    }

    public int Register(IRewindable rewindableVariable) {
        rewindableVariables.Add(rewindableVariable);
        MaxMaxFramesWithoutBeingRecorded = Mathf.Max(MaxMaxFramesWithoutBeingRecorded, rewindableVariable.MaxFramesWithoutBeingRecorded);
        numVariablesNotRecordedAtLeastOnce++;
        return rewindableVariables.Count - 1;
    }

    public void IncreaseNumModifiedVariablesThisFrameBy1() {
        numModifiedVariablesThisFrame++;
    }

    // call on late update
    public void RecordVariables() {
        object[] recordedVariablesThisFrame = new object[numModifiedVariablesThisFrame + numOutdatedVariables + numVariablesNotRecordedAtLeastOnce];
        int modifiedIndex = 0;
        BitArray recordedVariablesMask = new BitArray(rewindableVariables.Count, false);

        for (int i = 0; i < rewindableVariables.Count; i++) {
            IRewindable rewindableVariable = rewindableVariables[i];
            if (!rewindableVariable.RecordedAtLeastOnce) {
                recordedVariablesThisFrame[modifiedIndex] = rewindableVariable.Record();
                modifiedIndex++;
                rewindableVariable.FramesWithoutBeingRecorded = 0;
                rewindableVariable.RecordedAtLeastOnce = true;
                recordedVariablesMask[i] = true;
                numVariablesNotRecordedAtLeastOnce--;

            } else if (rewindableVariable.IsModified) {
                recordedVariablesThisFrame[modifiedIndex] = rewindableVariable.Record(); 
                modifiedIndex++;
                rewindableVariable.FramesWithoutBeingRecorded = 0;
                recordedVariablesMask[i] = true;

            } else if( rewindableVariable.MaxFramesWithoutBeingRecordedEnabled && 
                       rewindableVariable.FramesWithoutBeingRecorded == rewindableVariable.MaxFramesWithoutBeingRecorded ) {

                recordedVariablesThisFrame[modifiedIndex] = rewindableVariable.Record();
                modifiedIndex++;
                numOutdatedVariables--;
                rewindableVariable.FramesWithoutBeingRecorded = 0;
                recordedVariablesMask[i] = true;

            } else {
                recordedVariablesMask[i] = false;
                
                if (rewindableVariable.MaxFramesWithoutBeingRecordedEnabled) {
                    rewindableVariable.FramesWithoutBeingRecorded++;
                }
                
                if(rewindableVariable.MaxFramesWithoutBeingRecordedEnabled && 
                   rewindableVariable.FramesWithoutBeingRecorded == rewindableVariable.MaxFramesWithoutBeingRecorded ) {
                    /* variables that haven't been recorded for (MaxFramesWithoutBeingRecorded) frames will be recorded next frame.
                     * Count them with numOutdatedVariables.
                     */
                    numOutdatedVariables++;
                }
            }

            rewindableVariable.IsModified = false;
        }
        
        circularStack.Push(new RecordedData(recordedVariablesThisFrame, recordedVariablesMask, Time.deltaTime));
        numModifiedVariablesThisFrame = 0;
    }

    public void Rewind(float deltaTime) {
        /* If a rewindable variable can be X frames without being recorded, we cannot keep rewinding if there are less than X frames left,
         * since there's a possibility that the last time such variable was recorded was one of those last X frames. Since interpolation is often
         * performed, we need the last 2 recorded values of a rewindable variable, so we cannot keep rewinding if there are less than 2*maxMaxFramesWithoutBeingRecorded
         * records left in the circular stack.
         */
        while (elapsedTimeSinceLastRecord > previousRecordedData.deltaTime && circularStack.Count > 2 + 2*MaxMaxFramesWithoutBeingRecorded) {
            elapsedTimeSinceLastRecord -= previousRecordedData.deltaTime;
            previousRecordedData = circularStack.Pop();
            nextRecordedData = circularStack.Peek();
        }
        elapsedTimeSinceLastRecord += Time.deltaTime * TimeRewindManager.RewindSpeed;


        for (int i = 0; i < rewindableVariables.Count; i++) {
            if (!rewindableVariables[i].OnlyExecuteOnRewindStop) {
                RewindVariable(i, previousRecordedData, nextRecordedData, elapsedTimeSinceLastRecord);
            }
        }
    }

    private void RewindVariable(int id, RecordedData previousModifiedData, RecordedData nextModifiedData, float elapsedTimeSinceLastRecord) {
        (object previousRecord, object nextRecord) = GetPreviousAndNextRecord(id, previousModifiedData, nextModifiedData);
        rewindableVariables[id].Rewind(previousRecord, nextRecord, previousModifiedData.deltaTime, elapsedTimeSinceLastRecord);
    }

    private (object, object) GetPreviousAndNextRecord(int id, RecordedData previousModifiedData, RecordedData nextModifiedData) {
        int currentPeekDepth = 0;

        while (!previousModifiedData.HasRecordedDataFor(id) && currentPeekDepth < circularStack.Count - MaxMaxFramesWithoutBeingRecorded) {
            previousModifiedData = circularStack.Peek(currentPeekDepth);
            currentPeekDepth++;
        }

        nextModifiedData = circularStack.Peek(currentPeekDepth);
        while (!nextModifiedData.HasRecordedDataFor(id) && currentPeekDepth < circularStack.Count) {
            nextModifiedData = circularStack.Peek(currentPeekDepth);
            currentPeekDepth++;
        }

        object previousRecord = previousModifiedData.GetRecordedDataFor(id);
        object nextRecord = nextModifiedData.GetRecordedDataFor(id);
        return (previousRecord, nextRecord);
    }

}