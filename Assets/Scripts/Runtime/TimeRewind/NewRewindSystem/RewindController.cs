using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class RewindController {
    private struct RecordedData {
        public object[] recordedVariables;
        public BitArray mrecordedMask;
        public float deltaTime;

        public RecordedData(object[] recordedVariables, BitArray recordedMask, float deltaTime) {
            this.recordedVariables = recordedVariables;
            this.mrecordedMask = recordedMask;
            this.deltaTime = deltaTime;
        }

        public bool HasRecordedDataFor(int id) {
            return mrecordedMask != null && mrecordedMask[id];
        }

        public object GetRecordedDataFor(int id) {
            int index = -1;
            for (int i = 0; i <= id; i++) {
                if (mrecordedMask[i]) {
                    index++;
                }
            }
            return recordedVariables[index];
        }
    }

    private static RewindController instance;
    private List<IRewindable> rewindableVariables;
    private int NUM_FRAMES = 1000;
    private int numModifiedVariablesThisFrame;
    private int numVariablesNotRecordedAtLeastOnce;
    private int numOutdatedVariables; // number of rewindable variables that haven't been recorded in a long time
    private CircularStack<RecordedData> circularStack;
    private int maxMaxFramesWithoutBeingRecorded;
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

    }

    public void OnTimeRewindStart() {
        previousRecordedData = circularStack.Pop();
        nextRecordedData = circularStack.Peek();
        elapsedTimeSinceLastRecord = 0;
        numOutdatedVariables = 0;
        numOutdatedVariables = 0;
    }

    public void OnTimeRewindStop() {

    }

    public int Register(IRewindable rewindableVariable) {
        rewindableVariables.Add(rewindableVariable);
        maxMaxFramesWithoutBeingRecorded = Mathf.Max(maxMaxFramesWithoutBeingRecorded, rewindableVariable.MaxFramesWithoutBeingRecorded);
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
            if (!rewindableVariable.HasBeenRecordedAtLeastOnce) {
                recordedVariablesThisFrame[modifiedIndex] = rewindableVariable.Record();
                modifiedIndex++;
                rewindableVariable.FramesWithoutBeingRecorded = 0;
                rewindableVariable.HasBeenRecordedAtLeastOnce = true;
                recordedVariablesMask[i] = true;

            } else if (rewindableVariable.IsModified) {
                recordedVariablesThisFrame[modifiedIndex] = rewindableVariable.Record();
                modifiedIndex++;
                rewindableVariable.FramesWithoutBeingRecorded = 0;
                recordedVariablesMask[i] = true;

            } else if( rewindableVariable.FramesWithoutBeingRecorded == rewindableVariable.MaxFramesWithoutBeingRecorded ) {

                recordedVariablesThisFrame[modifiedIndex] = rewindableVariable.Record();
                modifiedIndex++;
                numOutdatedVariables--;
                rewindableVariable.FramesWithoutBeingRecorded = 0;
                recordedVariablesMask[i] = true;

            } else {
                rewindableVariable.FramesWithoutBeingRecorded++;
                if(rewindableVariable.FramesWithoutBeingRecorded == rewindableVariable.MaxFramesWithoutBeingRecorded ) {
                    /* variables that haven't been recorded for (MaxFramesWithoutBeingRecorded - 1) frames will be recorded next frame.
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
        while (elapsedTimeSinceLastRecord > previousRecordedData.deltaTime && circularStack.Count > 2 + 2*maxMaxFramesWithoutBeingRecorded) {
            elapsedTimeSinceLastRecord -= previousRecordedData.deltaTime;
            previousRecordedData = circularStack.Pop();
            nextRecordedData = circularStack.Peek();
        }
        elapsedTimeSinceLastRecord += Time.deltaTime * TimeRewindManager.RewindSpeed;


        for (int i = 0; i < rewindableVariables.Count; i++) {
            RewindVariable(i, previousRecordedData, nextRecordedData, elapsedTimeSinceLastRecord);
        }
    }

    private void RewindVariable(int id, RecordedData previousModifiedData, RecordedData nextModifiedData, float elapsedTimeSinceLastRecord) {
        int currentPeekDepth = 0;

        while (!previousModifiedData.HasRecordedDataFor(id) && currentPeekDepth < circularStack.Count-maxMaxFramesWithoutBeingRecorded) {
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
        rewindableVariables[id].Rewind(previousRecord, nextRecord, previousModifiedData.deltaTime, elapsedTimeSinceLastRecord);
    }

}