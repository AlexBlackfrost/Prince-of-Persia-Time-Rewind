using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class TimeRewindTests{
    // A Test behaves as an ordinary method
    [Test]
    public void RewindOnce(){
        RewindableVariable<float> rewindableNumber = new RewindableVariable<float>(1);
        TimeRewindController.Instance.RecordVariables();

        rewindableNumber.Value = 2;
        TimeRewindController.Instance.RecordVariables();

        rewindableNumber.Value = 5;
        TimeRewindController.Instance.RecordVariables();

        TimeRewindController.Instance.RecordVariables();

        TimeRewindController.Instance.RecordVariables();

        TimeRewindController.Instance.RecordVariables();

        rewindableNumber.Value = 7;
        TimeRewindController.Instance.RecordVariables();

        rewindableNumber.Value = 8;
        TimeRewindController.Instance.RecordVariables();

        TimeRewindController.Instance.RecordVariables();

        rewindableNumber.Value = 6;
        TimeRewindController.Instance.RecordVariables();

        TimeRewindController.Instance.StartTimeRewind();
        TimeRewindController.Instance.Rewind();
        

        Assert.True(rewindableNumber.Value > 6 && rewindableNumber.Value < 8);
    }
    
    [Test]
    public void RewindMultipleTimes(){
        RewindableVariable<float> rewindableNumber = new RewindableVariable<float>(1);
        for (int i = 0; i < 500; i++) {
            TimeRewindController.Instance.RecordVariables();
        }
        TimeRewindController.Instance.RecordVariables();

        rewindableNumber.Value = 2;
        TimeRewindController.Instance.RecordVariables();

        rewindableNumber.Value = 5;
        TimeRewindController.Instance.RecordVariables();

        TimeRewindController.Instance.RecordVariables();

        TimeRewindController.Instance.RecordVariables();

        TimeRewindController.Instance.RecordVariables();

        rewindableNumber.Value = 7;
        TimeRewindController.Instance.RecordVariables();

        rewindableNumber.Value = 8;
        TimeRewindController.Instance.RecordVariables();

        TimeRewindController.Instance.RecordVariables();

        rewindableNumber.Value = 6;
        TimeRewindController.Instance.RecordVariables();

        TimeRewindController.Instance.StartTimeRewind();

        for(int i = 0; i < 120; i++) {
            TimeRewindController.Instance.Rewind();
        }

        Assert.True(rewindableNumber.Value >= 1 && rewindableNumber.Value < 2);
    }

    
}
