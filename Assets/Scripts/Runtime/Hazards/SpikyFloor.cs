using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikyFloor : MonoBehaviour{
    [SerializeField] private AnimationCurve Movement;

    private SkinnedMeshRenderer meshRenderer;
    private int spikeBlendShapeIndex = 0;

    private void Awake(){
        meshRenderer = GetComponent<SkinnedMeshRenderer>();
    }

    private void Update(){
        float time = (float)TimeRewindManager.Instance.SecondsSinceStart();
        float spikeValue = Movement.Evaluate(Movement.Evaluate(time));
        meshRenderer.SetBlendShapeWeight(spikeBlendShapeIndex, (1 - spikeValue) * 100);
    }
}