using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class TimeRewindController {
    
    public event Action TimeRewindStart;
    public event Action TimeRewindStop;
    public float RewindSpeed { get; set; } = 0.1f;
    public bool IsRewinding { get; set; }
    public DateTime Now {
        get {
            /* Account for the time elapsed while rewinding too, since DateTime won't stop while rewinding.
             * Since RewindSpeed doesn't need to be 1, it has to be tracked independently
             */
            return DateTime.Now.AddSeconds(-(totalElapsedTimeRewinding + totalRewindedTime));
        }
        private set {
            Now = value;
        }
    }

    private DateTime startRewindTimestamp;
    private double totalElapsedTimeRewinding;
    private double totalRewindedTime;

    public int MaxMaxFramesWithoutBeingRecorded { get; private set; }
    public Action AfterRewindingVariablesOnRewind { get; set; }
    public Action BeforeRewindingVariablesOnRewind { get; set; }
    public Action BeforeRewindingVariablesOnRewindStop { get; set; }
    public Action AfterRewindingVariablesOnRewindStop { get; set; }

    private static TimeRewindController instance;
    private List<IRewindable> rewindableVariables;
    private int NUM_FRAMES = 1200;
    private int numModifiedVariablesThisFrame;
    private int numVariablesNotRecordedAtLeastOnce;
    private int numOutdatedVariables; // number of rewindable variables that haven't been recorded in a long time
    private CircularStack<RecordedData> circularStack;
    private RecordedData previousRecordedData, nextRecordedData;
    private float elapsedTimeSinceLastRecord;

    public static TimeRewindController Instance {
        get {
            if (instance == null) {
                instance = new TimeRewindController();
            }
            return instance;
        }
        private set {
            Instance = value;
        }
    }

    public TimeRewindController() {
        rewindableVariables = new List<IRewindable>();
        circularStack = new CircularStack<RecordedData>(NUM_FRAMES);
        elapsedTimeSinceLastRecord = 0;
        numVariablesNotRecordedAtLeastOnce = 0;
    }

    public void StartTimeRewind() {
        startRewindTimestamp = DateTime.Now;
        TimeRewindStart?.Invoke();
        OnTimeRewindStart();
        IsRewinding = true;
        Debug.Log("Start rewind");
    }

    public void StopTimeRewind() {
        totalElapsedTimeRewinding += DateTime.Now.Subtract(startRewindTimestamp).TotalSeconds;
        totalRewindedTime += DateTime.Now.Subtract(startRewindTimestamp).TotalSeconds * RewindSpeed;
        TimeRewindStop?.Invoke();
        OnTimeRewindStop();
        IsRewinding = false;
        Debug.Log("Stop rewind");
    }

    private void OnTimeRewindStart() {
        previousRecordedData = circularStack.Pop();
        nextRecordedData = circularStack.Peek();
        elapsedTimeSinceLastRecord = 0;

        foreach(IRewindable rewindableVariable in rewindableVariables) {
            rewindableVariable.OnRewindStart();
        }
    }

    private void OnTimeRewindStop() {
        /**
         * MaxPeekDepthPreviousModifiedData has to be the maximum number of frames without being recorded so that
         * when the NextModifiedData is searched, there's still a chance to find it at least once, since it is 
         * going to be recorded at least once in the last @MaxMaxFramesWithoutBeingRecorded frames.
         */
        BeforeRewindingVariablesOnRewindStop?.Invoke();
        int maxPeekDepthPreviousModifiedData = circularStack.Count - (MaxMaxFramesWithoutBeingRecorded + 1);
        int maxPeekDepthNextModifiedData = circularStack.Count;
        for (int id = 0; id < rewindableVariables.Count; id++) {
            IRewindable rewindableVariable = rewindableVariables[id];
            (object previousRecord, object nextRecord) = GetPreviousAndNextRecord(id, previousRecordedData, nextRecordedData, 
                                                                                  maxPeekDepthPreviousModifiedData, maxPeekDepthNextModifiedData);
            if(previousRecord!=null && nextRecord != null) {
                rewindableVariable.OnRewindStop(previousRecord, nextRecord, previousRecordedData.deltaTime, elapsedTimeSinceLastRecord);
            }

            int? numFramesToPreviousRecord = FindNumFramesToPreviousRecord(id);
            if(numFramesToPreviousRecord == null) {
                rewindableVariable.FramesWithoutBeingRecorded = rewindableVariable.MaxFramesWithoutBeingRecorded;
            }
            if (rewindableVariable.FramesWithoutBeingRecorded == rewindableVariable.MaxFramesWithoutBeingRecorded) {
                /* variables that haven't been recorded for (MaxFramesWithoutBeingRecorded) frames will be recorded next frame.
                 * Count them with numOutdatedVariables.
                 */
                numOutdatedVariables++;
            }
        }
        //numOutdatedVariables = 0;
        AfterRewindingVariablesOnRewindStop?.Invoke();
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
        object[] recordedVariablesThisFrame = null;
        try {
            recordedVariablesThisFrame = new object[numModifiedVariablesThisFrame + numOutdatedVariables + numVariablesNotRecordedAtLeastOnce];
        } catch(OverflowException e) {
            Debug.Log(e.StackTrace);
        }
        int modifiedIndex = 0;
        bool[] recordedVariablesMask = new bool[rewindableVariables.Count];
        for (int i = 0; i < rewindableVariables.Count; i++) {
            IRewindable rewindableVariable = rewindableVariables[i];

            if (!rewindableVariable.RecordedAtLeastOnce) {
                recordedVariablesThisFrame[modifiedIndex] = rewindableVariable.Record();
                modifiedIndex++;
                rewindableVariable.FramesWithoutBeingRecorded = 0;
                rewindableVariable.RecordedAtLeastOnce = true;
                recordedVariablesMask[i] = true;
                numVariablesNotRecordedAtLeastOnce--;

            } else if(rewindableVariable.FramesWithoutBeingRecorded == rewindableVariable.MaxFramesWithoutBeingRecorded ) {

                recordedVariablesThisFrame[modifiedIndex] = rewindableVariable.Record();
                modifiedIndex++;
                numOutdatedVariables--;
                rewindableVariable.FramesWithoutBeingRecorded = 0;
                recordedVariablesMask[i] = true;
            
            } else if (rewindableVariable.IsModified) {
                try {
                    recordedVariablesThisFrame[modifiedIndex] = rewindableVariable.Record(); 

                }catch(IndexOutOfRangeException e) {
                    Debug.Log(e.StackTrace);
                }
                modifiedIndex++;
                rewindableVariable.FramesWithoutBeingRecorded = 0;
                recordedVariablesMask[i] = true;


            } else {
                recordedVariablesMask[i] = false;
                rewindableVariable.FramesWithoutBeingRecorded++;
                
                if(rewindableVariable.FramesWithoutBeingRecorded == rewindableVariable.MaxFramesWithoutBeingRecorded ) {
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

    public void Rewind() {
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
        elapsedTimeSinceLastRecord += Time.deltaTime * RewindSpeed;

        BeforeRewindingVariablesOnRewind?.Invoke();
        for (int i = 0; i < rewindableVariables.Count; i++) {
            if (!rewindableVariables[i].OnlyExecuteOnRewindStop) {
                RewindVariable(i, previousRecordedData, nextRecordedData, elapsedTimeSinceLastRecord);
            }
        }
        AfterRewindingVariablesOnRewind?.Invoke();
    }

    private void RewindVariable(int id, RecordedData previousModifiedData, RecordedData nextModifiedData, float elapsedTimeSinceLastRecord) {
        /**
         * MaxPeekDepthPreviousModifiedData has to be the maximum number of frames without being recorded so that
         * when the NextModifiedData is searched, there's still a chance to find it at least once, since it is 
         * going to be recorded at least once in the last @MaxMaxFramesWithoutBeingRecorded frames.
         * 
         * However, after RewindVariable is called, OnRewindStop is going to be called at the end, so we need
         * to multiply it by 2 (by 3 in previousModifiedData) to account for it and make sure the records
         * are going to be found too in OnRewindStop.
         */
        int maxPeekDepthPreviousModifiedData = Math.Max(circularStack.Count - 3 * (MaxMaxFramesWithoutBeingRecorded + 1), circularStack.Count-3);
        int maxPeekDepthNextNextModifiedData = Math.Max(circularStack.Count - 2 * (MaxMaxFramesWithoutBeingRecorded + 1), circularStack.Count-2);
        (object previousRecord, object nextRecord) = GetPreviousAndNextRecord(id, previousModifiedData, nextModifiedData, 
                                                                              maxPeekDepthPreviousModifiedData, maxPeekDepthNextNextModifiedData);

        if(previousRecord!=null && nextRecord != null) {
            rewindableVariables[id].Rewind(previousRecord, nextRecord, previousModifiedData.deltaTime, elapsedTimeSinceLastRecord);
        }
    }

    private (object, object) GetPreviousAndNextRecord(int id, RecordedData previousModifiedData, RecordedData nextModifiedData, 
                                                      int maxPeekDepthPreviousModifiedData, int maxPeekDepthNextModifiedData) {

        // Find previous record
        int currentPeekDepth = 0;
        while (!previousModifiedData.HasRecordedDataFor(id) && currentPeekDepth < maxPeekDepthPreviousModifiedData) {
            previousModifiedData = circularStack.Peek(currentPeekDepth);
            nextModifiedData = circularStack.Peek(currentPeekDepth+1);
            currentPeekDepth++;
        }
        object previousRecord = previousModifiedData.GetRecordedDataFor(id);

        // Find next record
        object nextRecord = null;
        if (currentPeekDepth == 0) {
            while (!nextModifiedData.HasRecordedDataFor(id) && currentPeekDepth < maxPeekDepthNextModifiedData) {
                nextModifiedData = circularStack.Peek(currentPeekDepth);
                currentPeekDepth++;
            }
            nextRecord = nextModifiedData.GetRecordedDataFor(id);
        } else {
            nextRecord = previousModifiedData.GetRecordedDataFor(id);

        }

        return (previousRecord, nextRecord);
    }

    private int? FindNumFramesToPreviousRecord(int id) {
        int? currentPeekDepth = 0;
        RecordedData previousModifiedData = circularStack.Peek(currentPeekDepth.GetValueOrDefault());
        while (!previousModifiedData.HasRecordedDataFor(id) && currentPeekDepth < circularStack.Count) {
            currentPeekDepth++;
            previousModifiedData = circularStack.Peek(currentPeekDepth.GetValueOrDefault());
        }

        if (!previousModifiedData.HasRecordedDataFor(id)) {
            currentPeekDepth = null;
        }

        return currentPeekDepth;
    }
}