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

    [Header("Lift Gamma Gain")]
    [SerializeField] private float timeRewindLiftGammaGain;
  
    private float zoomScreenElapsedTime;
    private Coroutine resetZoomScreenCoroutine;
    
    private Bloom bloom;
    private float previousBloomThreshold;
    private float bloomElapsedTime;
    private Coroutine resetBloomCoroutine;
    
    private LiftGammaGain liftGammaGain;
    private Vector4 previousLiftGammaGain;

    private void Awake() {
        TimeRewindManager.Instance.TimeRewindStart += OnTimeRewindStart;
        TimeRewindManager.Instance.TimeRewindStop += OnTimeRewindStop;

        postprocessingVolume.profile.TryGet(out liftGammaGain);
        postprocessingVolume.profile.TryGet(out bloom);
    }

    private void Update(){
        if (TimeRewindManager.Instance.IsRewinding) {
            AnimateZoomScreen();
            AnimateBloom();
        }
    }

    private void OnTimeRewindStart() {
        zoomScreenElapsedTime = 0;
        previousBloomThreshold = bloom.threshold.value;
        previousLiftGammaGain = liftGammaGain.gain.value;

        bloom.threshold.value = timeRewindMinBloomThreshold;
        liftGammaGain.gain.Override(new Vector4(previousLiftGammaGain.x, previousLiftGammaGain.y, previousLiftGammaGain.z, timeRewindLiftGammaGain));
        timeRewindPostprocessingEffect.SetActive(true);

        if(resetBloomCoroutine != null) {
            StopCoroutine(resetBloomCoroutine);
            resetBloomCoroutine = null;
        }

        if(resetZoomScreenCoroutine != null) {
            StopCoroutine(resetZoomScreenCoroutine);
            resetZoomScreenCoroutine = null;
        }
    }

    private void OnTimeRewindStop() {
        bloom.threshold.value = previousBloomThreshold;
        liftGammaGain.gain.value = previousLiftGammaGain;
        //timeRewindPostprocessingEffect.SetActive(false);
        resetBloomCoroutine = StartCoroutine(ResetBloom());
        resetBloomCoroutine = StartCoroutine(ResetZoomScreen());
    }

    private void AnimateZoomScreen() {
        float strength01 = zoomScreenAnimationCurve.Evaluate(zoomScreenElapsedTime);
        float zoomScreenStrength = MathUtils.MapRangeClamped(strength01, 0, 1, -zoomScreenMaxStrength, -zoomScreenMinStrength);
        zoomScreenMaterial.SetFloat("_Strength", zoomScreenStrength);
        zoomScreenElapsedTime += Time.deltaTime * zoomScreenSpeed;
    }


    private void AnimateBloom() {
        float bloomThreshold01 = bloomAnimationCurve.Evaluate(bloomElapsedTime);
        float bloomThreshold = MathUtils.MapRangeClamped(bloomThreshold01, 0, 1, timeRewindMinBloomThreshold, timeRewindMaxBloomThreshold);
        bloom.threshold.value = bloomThreshold;
        bloomElapsedTime += Time.deltaTime * bloomSpeed;
    }

    private IEnumerator ResetBloom() {
        float resetBloomSpeed = Mathf.Abs(bloom.threshold.value - previousBloomThreshold)/resetBloomDuration;
        float resetBloomElapsedTime = 0;
        float initalBloomThreshold = bloom.threshold.value;

        while (!Mathf.Approximately(bloom.threshold.value,previousBloomThreshold)) {
            float lerpAlpha = resetBloomEasing.Evaluate(resetBloomElapsedTime);
            float bloomThreshold = Mathf.Lerp(initalBloomThreshold, previousBloomThreshold, lerpAlpha);
            bloom.threshold.value = bloomThreshold;
            resetBloomElapsedTime += Time.deltaTime * resetBloomSpeed;
            yield return null;
        }
        resetBloomCoroutine = null;
    }
    
    private IEnumerator ResetZoomScreen() {
        float initialZoomScreenStrength = zoomScreenMaterial.GetFloat("_Strength");
        float targetZoomScreenStrength = 0;
        float resetZoomScreenSpeed = Mathf.Abs(initialZoomScreenStrength) / resetZoomScreenDuration;
        float resetZoomScreenElapsedTime = 0;

        float currentZoomScreenStrength = initialZoomScreenStrength;
        while (!Mathf.Approximately(currentZoomScreenStrength, targetZoomScreenStrength)) {
            float lerpAlpha = resetZoomScreenEasing.Evaluate(resetZoomScreenElapsedTime);
            currentZoomScreenStrength = Mathf.Lerp(initialZoomScreenStrength, targetZoomScreenStrength, lerpAlpha);
            zoomScreenMaterial.SetFloat("_Strength", currentZoomScreenStrength);
            resetZoomScreenElapsedTime += Time.deltaTime * resetZoomScreenSpeed;
            yield return null;
        }
        zoomScreenMaterial.SetFloat("_Strength", targetZoomScreenStrength);
        resetZoomScreenCoroutine = null;

    }


}