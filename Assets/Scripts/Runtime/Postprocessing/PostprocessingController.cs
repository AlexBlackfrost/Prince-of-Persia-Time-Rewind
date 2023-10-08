using Codice.Client.BaseCommands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostprocessingController : MonoBehaviour {
    [SerializeField] private ScriptableRendererFeature timeRewindPostprocessingEffect;
    [SerializeField] Volume postprocessingVolume;

    [Header("Zoom screen")]
    [SerializeField] private Material zoomScreenMaterial;
    [SerializeField] private float zoomScreenMinStrength = 0.2f;
    [SerializeField] private float zoomScreenMaxStrength = 0.5f;
    [SerializeField] AnimationCurve zoomScreenAnimationCurve;
    [SerializeField] private float zoomScreenSpeed = 0.5f;
    [SerializeField] AnimationCurve resetZoomScreenEasing;
    [SerializeField] private float resetZoomScreenDuration = 0.5f;

    [Header("Bloom")]
    [SerializeField] private float timeRewindMinBloomThreshold = 1.2f;
    [SerializeField] private float timeRewindMaxBloomThreshold = 1.6f;
    [SerializeField] AnimationCurve bloomAnimationCurve;
    [SerializeField] private float bloomSpeed = 1f;
    [SerializeField] AnimationCurve resetBloomEasing;
    [SerializeField] private float resetBloomDuration = 0.5f;

    [Header("Gain")]
    [SerializeField] private float timeRewindGain = 1.6f;
    [SerializeField] private float resetGainDuration = 0.5f;
    [SerializeField] AnimationCurve resetGainEasing;

    [Header("Lift")]
    [SerializeField] private float startRewindLift = 2;
    [SerializeField] private float startLiftDuration = 0.1f;
    [SerializeField] AnimationCurve startLiftEasing;
  
    private float zoomScreenElapsedTime;
    
    private Bloom bloom;
    private float previousBloomThreshold;
    private float bloomElapsedTime;
    
    private LiftGammaGain liftGammaGain;
    private Vector4 previousGain;
    private Vector4 previousLift;

    private Dictionary<string, Coroutine> runningTweens;
    private Dictionary<string, Action> onTweenStoppedCallbacks;

    private const string resetBloomKey = "ResetBloom";
    private const string resetZoomScreenKey = "ResetZoomScreen";
    private const string resetGainKey = "ResetGain";
    private const string liftKey = "Lift";

    private void Awake() {
        TimeRewindManager.Instance.TimeRewindStart += OnTimeRewindStart;
        TimeRewindManager.Instance.TimeRewindStop += OnTimeRewindStop;

        postprocessingVolume.profile.TryGet(out liftGammaGain);
        postprocessingVolume.profile.TryGet(out bloom);

        runningTweens = new Dictionary<string, Coroutine>();
        onTweenStoppedCallbacks = new Dictionary<string, Action>();
    }

    private void Update(){
        if (TimeRewindManager.Instance.IsRewinding) {
            AnimateZoomScreen();
            AnimateBloom();
        }
    }

    private void OnTimeRewindStart() {
        StopRunningTweens();

        zoomScreenElapsedTime = 0;
        bloomElapsedTime = 0;

        previousBloomThreshold = bloom.threshold.value;
        previousGain = liftGammaGain.gain.value;
        previousLift = liftGammaGain.lift.value;

        bloom.threshold.value = timeRewindMinBloomThreshold;
        
        StartTween(liftKey, startRewindLift, 0, startLiftDuration, startLiftEasing, UpdateLift);
        onTweenStoppedCallbacks[liftKey] = () => { UpdateLift(previousLift.w); };
        
        liftGammaGain.gain.Override(new Vector4(previousGain.x, previousGain.y, previousGain.z, timeRewindGain));
        
        timeRewindPostprocessingEffect.SetActive(true);
    }

    private void OnTimeRewindStop() {
        onTweenStoppedCallbacks[resetBloomKey] = () => { UpdateBloomThreshold(previousBloomThreshold); };
        onTweenStoppedCallbacks[resetZoomScreenKey] = () => { UpdateZoomScreenStrength(0); };
        onTweenStoppedCallbacks[resetGainKey] = () => { UpdateGain(previousGain.w); };

        StartTween(resetBloomKey, bloom.threshold.value, previousBloomThreshold, resetBloomDuration, resetBloomEasing, UpdateBloomThreshold, OnResetTweenerFinished);
        StartTween(resetZoomScreenKey, zoomScreenMaterial.GetFloat("_Strength"),0, resetZoomScreenDuration, resetZoomScreenEasing, UpdateZoomScreenStrength, OnResetTweenerFinished);
        StartTween(resetGainKey, liftGammaGain.gain.value.w, previousGain.w, resetGainDuration, resetGainEasing, UpdateGain, OnResetTweenerFinished);

    }

    private void AnimateZoomScreen() {
        float zoomScreenStrength01 = zoomScreenAnimationCurve.Evaluate(zoomScreenElapsedTime);
        float zoomScreenStrength = MathUtils.MapRangeClamped(zoomScreenStrength01, 0, 1, -zoomScreenMaxStrength, -zoomScreenMinStrength);
        zoomScreenMaterial.SetFloat("_Strength", zoomScreenStrength);
        zoomScreenElapsedTime += Time.deltaTime * zoomScreenSpeed;
    }

    private void AnimateBloom() {
        float bloomThreshold01 = bloomAnimationCurve.Evaluate(bloomElapsedTime);
        float bloomThreshold = MathUtils.MapRangeClamped(bloomThreshold01, 0, 1, timeRewindMinBloomThreshold, timeRewindMaxBloomThreshold);
        bloom.threshold.value = bloomThreshold;
        bloomElapsedTime += Time.deltaTime * bloomSpeed;
    }

    private void StartTween(string tweenId, float initialValue, float targetValue, float duration, AnimationCurve easing, Action<float> setter, Action<string> onTweenEnd = null) {
        runningTweens[tweenId] = StartCoroutine(Tween(tweenId, initialValue, targetValue, duration, easing, setter, onTweenEnd ));
    }

    private IEnumerator Tween(string tweenId, float initialValue, float targetValue, float duration, AnimationCurve easing, Action<float> setter, Action<string> onTweenEnd = null) {
        float currentValue = initialValue;
        float tweenSpeed = Mathf.Abs(targetValue-initialValue) / duration;
        float elapsedTime = 0;
        while (!Mathf.Approximately(currentValue, targetValue)) {
            float lerpAlpha = Mathf.Clamp01(easing.Evaluate(elapsedTime));
            currentValue = Mathf.Lerp(initialValue, targetValue, lerpAlpha);
            setter(currentValue);
            elapsedTime += Time.deltaTime * tweenSpeed;
            yield return currentValue;
        }
        runningTweens.Remove(tweenId);
        onTweenEnd?.Invoke(tweenId);
    }

    private void UpdateBloomThreshold(float bloomThreshold) { 
        bloom.threshold.Override(bloomThreshold);
    }
    
    private void UpdateZoomScreenStrength(float zoomScreenStrength) {
        zoomScreenMaterial.SetFloat("_Strength", zoomScreenStrength);
    }

    private void UpdateGain(float gainW) {
        Vector4 gainValue = liftGammaGain.gain.value;
        gainValue.w = gainW;
        liftGammaGain.gain.Override(gainValue);
    }

    private void UpdateLift(float liftGammaLiftW) {
        Vector4 liftValue = liftGammaGain.lift.value;
        liftValue.w = liftGammaLiftW;
        liftGammaGain.lift.Override(liftValue);
    }

    private void OnResetTweenerFinished(string tweenId) {
        if(runningTweens.Count == 0) { 
            timeRewindPostprocessingEffect.SetActive(false);
        }
    }

    private void StopRunningTweens() {
        List<string> keysToRemove = new List<string>();
        foreach (KeyValuePair<string, Coroutine> runningTween in runningTweens) {
            if (runningTween.Value != null) {
                StopCoroutine(runningTween.Value);
                onTweenStoppedCallbacks[runningTween.Key]?.Invoke();
                keysToRemove.Add(runningTween.Key);
            }
        }

        foreach(String key in keysToRemove) {
            runningTweens.Remove(key);
            onTweenStoppedCallbacks.Remove(key);
        }
    }

}