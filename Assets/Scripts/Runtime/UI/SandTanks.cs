using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SandTanks : MonoBehaviour{
    [SerializeField] private PlayerTimeRewinder playerTimeRewinder;
    [SerializeField] private Image[] sandTankImages;
    [SerializeField] private AnimationCurve sandTankFadeInEasing;
    [SerializeField] private float sandTankFadeOutDuration = 1;

    private Coroutine[] sandTanksAnimationCoroutines;

    private void Awake() {
        playerTimeRewinder.SandTankConsumed += OnSandTankConsumed;        
        playerTimeRewinder.SandTankRestored += OnSandTankRestored;       
        playerTimeRewinder.SandTanksInitialized += OnSandTanksInitialized;      
        sandTanksAnimationCoroutines = new Coroutine[sandTankImages.Length];
    }


    private void OnSandTankConsumed(int sandTankIndex) {
        if(sandTankIndex <= sandTankImages.Length) {
            if (sandTanksAnimationCoroutines[sandTankIndex-1] != null) {
                StopCoroutine(sandTanksAnimationCoroutines[sandTankIndex-1]);
            }
            sandTanksAnimationCoroutines[sandTankIndex-1] = StartCoroutine(FadeOutSandTank(sandTankIndex));
        }
    }

    private void OnSandTankRestored(int powerTankIndex) {
        if(powerTankIndex <= sandTankImages.Length) {
            sandTankImages[powerTankIndex-1].enabled = true;
        }
    }

    private void OnSandTanksInitialized(int availableSandTanks) {
        for(int i=0;i < availableSandTanks; i++) {
            sandTankImages[i].enabled = true;
        }

        for(int i=availableSandTanks;i<sandTankImages.Length;i++) {
            sandTankImages[i].enabled = false;
        }
    }

    private IEnumerator FadeOutSandTank(int sandTankIndex) {
        Color imageColor = sandTankImages[sandTankIndex-1].color;
        float elapsedTime = 0f;
        float speed = imageColor.a / sandTankFadeOutDuration;
        float initialAlpha = imageColor.a;

        while(elapsedTime < sandTankFadeOutDuration) { 
            float lerpAlpha = sandTankFadeInEasing.Evaluate(elapsedTime);

            imageColor.a = Mathf.Lerp(initialAlpha, 0, lerpAlpha);
            sandTankImages[sandTankIndex-1].color = imageColor;

            elapsedTime += Time.deltaTime * speed;
            yield return null;
        }

        imageColor.a = 0;
        sandTankImages[sandTankIndex-1].color = imageColor;
    }
}