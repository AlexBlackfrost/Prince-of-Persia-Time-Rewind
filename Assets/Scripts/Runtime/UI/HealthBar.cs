using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour{
    [SerializeField] private Image fill;

    [Header("Glow animation")]
    [SerializeField] private float healthChangedColorIntensity=1.2f;
    [SerializeField] private float healthChangedAnimationDuration = 1;
    [SerializeField] private AnimationCurve healthChangedGlowAnimation;

    [Header("Bar animation")]
    [SerializeField] private float barAnimationDuration = 1;
    [SerializeField] private AnimationCurve barAnimation;

    private const byte MAX_BYTE_FOR_OVEREXPOSED_COLOR = 191;

    private Material material;
    private Coroutine animateGlowCoroutine;
    private float currentColorIntensity;

    private Coroutine animateHealthBarCoroutine;

    private void Awake(){
        material = fill.material;
    }

    
    public void OnHealthChanged01(float previousHealth01, float currentHealth01) {
        if(animateGlowCoroutine != null) {
            StopCoroutine(animateGlowCoroutine);
            animateGlowCoroutine = null;
        }
        animateGlowCoroutine = StartCoroutine(AnimateGlow());

        if (animateHealthBarCoroutine != null) {
            StopCoroutine(animateHealthBarCoroutine);
            animateHealthBarCoroutine = null;
        }
        animateHealthBarCoroutine = StartCoroutine(AnimateHealthBar(previousHealth01, currentHealth01));
    }

    private IEnumerator AnimateGlow() {
        float maxColorComponent = material.color.maxColorComponent;
        float scaleFactor = MAX_BYTE_FOR_OVEREXPOSED_COLOR / maxColorComponent;
        float previousColorIntensity = Mathf.Log(255f / scaleFactor) / Mathf.Log(2f);
        Color color = material.color;
        currentColorIntensity = previousColorIntensity;

        float glowAnimationElapsedTime = 0;
        float changeGlowSpeed = Mathf.Abs(healthChangedColorIntensity - previousColorIntensity) / healthChangedAnimationDuration;
        
        while (glowAnimationElapsedTime < healthChangedAnimationDuration) {
            currentColorIntensity = MathUtils.MapRangeClamped( healthChangedGlowAnimation.Evaluate(glowAnimationElapsedTime),
                                                               0,1,
                                                               previousColorIntensity, healthChangedColorIntensity);

            glowAnimationElapsedTime += Time.deltaTime*changeGlowSpeed;
            material.color = color * currentColorIntensity;
            yield return null;
        }

        material.color = color * previousColorIntensity;
        animateGlowCoroutine = null;
    }

    private IEnumerator AnimateHealthBar(float previousHealth01, float currentHealth01) {
        float distance = Mathf.Abs(currentHealth01 - previousHealth01);
        float speed = distance/barAnimationDuration;
        float elapsedTime = 0;
        material.SetFloat("_Fill", previousHealth01);

        while (elapsedTime < barAnimationDuration) {
            float lerpAlpha = barAnimation.Evaluate(elapsedTime*speed)/distance;
            float fill = Mathf.Lerp(previousHealth01, currentHealth01, lerpAlpha);

            material.SetFloat("_Fill", fill);
            elapsedTime += Time.deltaTime;
            yield return fill;
        }

        material.SetFloat("_Fill", currentHealth01);
        animateHealthBarCoroutine = null;  
    } 

}