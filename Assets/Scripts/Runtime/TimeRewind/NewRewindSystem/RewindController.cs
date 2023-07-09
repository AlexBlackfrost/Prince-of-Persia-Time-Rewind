using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class RewindController {
    private struct ModifiedData {
        public object[] modifiedVariables;
        public BitArray modifiedMask;
        public float deltaTime;

        public ModifiedData(object[] modifiedVariables, BitArray modifiedMask, float deltaTime) {
            this.modifiedVariables = modifiedVariables;
            this.modifiedMask = modifiedMask;
            this.deltaTime = deltaTime;
        }

        public bool HasRecordedDataFor(int id) {
            return modifiedMask != null && modifiedMask[id];
        }

        public object GetRecordedDataFor(int id) {
            int index = -1;
            for (int i = 0; i <= id; i++) {
                if (modifiedMask[i]) {
                    index++;
                }
            }
            return modifiedVariables[index];
        }
    }

    private static RewindController instance;
    private List<IRewindable> rewindableVariables;
    private int NUM_FRAMES = 1000;
    private int numModifiedVariablesThisFrame;
    private CircularStack<ModifiedData> circularStack;
    private ModifiedData previousModifiedData, nextModifiedData;
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
        circularStack = new CircularStack<ModifiedData>(NUM_FRAMES);
        elapsedTimeSinceLastRecord = 0;
    }



    public int Register(IRewindable rewindableVariable) {
        rewindableVariables.Add(rewindableVariable);
        return rewindableVariables.Count - 1;
    }

    public void IncreaseNumModifiedVariablesByOne() {
        numModifiedVariablesThisFrame++;
    }

    // call on late update
    public void RecordValues(bool onlyModified) {
        object[] modifiedVariablesThisFrame = new object[numModifiedVariablesThisFrame];
        int modifiedIndex = 0;
        BitArray modifiedVariablesMask = new BitArray(rewindableVariables.Count);
        for (int i = 0; i < rewindableVariables.Count; i++) {
            IRewindable rewindableVariable = rewindableVariables[i];
            if (!onlyModified || (onlyModified && rewindableVariable.IsModified)) {//record all or onlyModified
                modifiedVariablesThisFrame[modifiedIndex] = rewindableVariable.Record();
                modifiedIndex++;
            }

            modifiedVariablesMask[i] = rewindableVariable.IsModified;
            rewindableVariable.IsModified = false;
        }

        circularStack.Push(new ModifiedData(modifiedVariablesThisFrame, modifiedVariablesMask, Time.deltaTime));
        numModifiedVariablesThisFrame = 0;
    }

    public void Rewind(float deltaTime) {
        while (elapsedTimeSinceLastRecord > previousModifiedData.deltaTime && circularStack.Count > 2) {
            elapsedTimeSinceLastRecord -= previousModifiedData.deltaTime;
            previousModifiedData = circularStack.Pop();
            nextModifiedData = circularStack.Peek();
        }
        ModifiedData lastFrameModifiedData = circularStack.Pop();
        elapsedTimeSinceLastRecord += Time.deltaTime * TimeRewindManager.RewindSpeed;

        for (int i = 0; i < rewindableVariables.Count; i++) {
            RewindVariable(i, previousModifiedData, nextModifiedData, elapsedTimeSinceLastRecord);
        }
    }

    private void RewindVariable(int id, ModifiedData previousModifiedData, ModifiedData nextModifiedData, float elapsedTimeSinceLastRecord) {
        int currentPeekDepth = 0;
        while (!previousModifiedData.HasRecordedDataFor(id) && currentPeekDepth < circularStack.Count-1) {
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