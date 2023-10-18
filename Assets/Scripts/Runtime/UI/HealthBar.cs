using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Image fill;

    [Header("Glow animation")]
    [SerializeField][ColorUsage(true, true)] private Color initialColor;
    [SerializeField] private float healthChangedColorIntensity=1.2f;
    [SerializeField] private float healthChangedAnimationDuration = 1;
    [SerializeField] private AnimationCurve healthChangedGlowAnimation;

    [Header("Bar animation")]
    [SerializeField] private float barAnimationDuration = 1;
    [SerializeField] private AnimationCurve barAnimation;

    [Header("Spark animation")]
    [SerializeField] private Image spark;
    [SerializeField] private float minHealthPercentVisibility = 0.3f;
    [SerializeField] private float maxHealthPercentVisibility = 1.0f;
    [SerializeField] private float horizontalOffset = 0.0f;
    [SerializeField] private float verticalOffset = -0.2f;
    [SerializeField] private AnimationCurve sparkScaleAnimationCurve;
    [SerializeField] private float sparkScaleAnimationDuration;
    [SerializeField] private float startScale = 0.0f;
    [SerializeField] private float maxScale = 1.5f;
    [SerializeField] private AnimationCurve sparkPositionAnimationCurve;
    [SerializeField] private float sparkPositionAnimationDuration = 1;
    [SerializeField] private AnimationCurve sparkGlowAnimationCurve;
    [SerializeField] private float sparkMaxColorIntensity=1.2f;
    [SerializeField] private float sparkGlowAnimationDuration = 1;

    private const byte MAX_BYTE_FOR_OVEREXPOSED_COLOR = 191;

    private Material fillBarMaterial;
    private Coroutine animateGlowCoroutine;

    private Coroutine animateHealthBarCoroutine;

    private Material sparkMaterial;
    private Coroutine animateSparkScaleCoroutine;
    private Coroutine animateSparkPositionCoroutine;
    private Coroutine animateSparkGlowCoroutine;


    private void Awake(){
        fillBarMaterial = fill.material;
        fillBarMaterial.color = initialColor;

        sparkMaterial = spark.material;
        playerController.health.HealthChanged01 += OnHealthChanged01;
    }

    
    public void OnHealthChanged01(float previousHealth01, float currentHealth01) {
        // Glow
        if(animateGlowCoroutine != null) {
            StopCoroutine(animateGlowCoroutine);
            animateGlowCoroutine = null;
        }
        animateGlowCoroutine = StartCoroutine(AnimateGlow());

        // Bar fill
        if (animateHealthBarCoroutine != null) {
            StopCoroutine(animateHealthBarCoroutine);
            animateHealthBarCoroutine = null;
        }
        animateHealthBarCoroutine = StartCoroutine(AnimateHealthBar(previousHealth01, currentHealth01));

        // Spark

        if (TimeRewindManager.Instance.IsRewinding && previousHealth01 >= minHealthPercentVisibility &&
                                                      previousHealth01 >= minHealthPercentVisibility && 
                                                      previousHealth01 <= maxHealthPercentVisibility &&
                                                      currentHealth01 <= maxHealthPercentVisibility) {
            if (animateSparkScaleCoroutine != null) {
                StopCoroutine(animateSparkScaleCoroutine);
                animateSparkScaleCoroutine = null;
            }
            animateSparkScaleCoroutine = StartCoroutine(AnimateSparkScale());

            if (animateSparkPositionCoroutine != null) {
                StopCoroutine(animateSparkPositionCoroutine);
                animateSparkPositionCoroutine = null;
            }
            animateSparkPositionCoroutine = StartCoroutine(AnimateSparkPosition(previousHealth01, currentHealth01));

            if (animateSparkGlowCoroutine != null) {
                StopCoroutine(animateSparkGlowCoroutine);
                animateSparkGlowCoroutine = null;
            }
            animateSparkGlowCoroutine = StartCoroutine(AnimateSparkGlow());
        }
        
    }


    private IEnumerator AnimateGlow() {
        float maxColorComponent = fillBarMaterial.color.maxColorComponent;
        float scaleFactor = MAX_BYTE_FOR_OVEREXPOSED_COLOR / maxColorComponent;
        float previousColorIntensity = Mathf.Log(255f / scaleFactor) / Mathf.Log(2f);
        Color color = fillBarMaterial.color;
        float currentColorIntensity = previousColorIntensity;

        float glowAnimationElapsedTime = 0;
        float changeGlowSpeed = Mathf.Abs(healthChangedColorIntensity - previousColorIntensity) / healthChangedAnimationDuration;
        
        while (glowAnimationElapsedTime < healthChangedAnimationDuration) {
            currentColorIntensity = MathUtils.MapRangeClamped( healthChangedGlowAnimation.Evaluate(glowAnimationElapsedTime),
                                                               0,1,
                                                               previousColorIntensity, healthChangedColorIntensity);

            glowAnimationElapsedTime += Time.deltaTime*changeGlowSpeed;
            fillBarMaterial.color = color * currentColorIntensity;
            yield return null;
        }

        fillBarMaterial.color = color * previousColorIntensity;
        animateGlowCoroutine = null;
    }

    private IEnumerator AnimateHealthBar(float previousHealth01, float currentHealth01) {
        float distance = Mathf.Abs(currentHealth01 - previousHealth01);
        float speed = distance/barAnimationDuration;
        float elapsedTime = 0;
        fillBarMaterial.SetFloat("_Fill", previousHealth01);

        while (elapsedTime < barAnimationDuration) {
            float lerpAlpha = barAnimation.Evaluate(elapsedTime*speed)/distance;
            float fill = Mathf.Lerp(previousHealth01, currentHealth01, lerpAlpha);

            fillBarMaterial.SetFloat("_Fill", fill);
            elapsedTime += Time.deltaTime;
            yield return fill;
        }

        fillBarMaterial.SetFloat("_Fill", currentHealth01);
        animateHealthBarCoroutine = null;

    } 



    private IEnumerator AnimateSparkScale() {
        float speed = (maxScale - startScale) / sparkScaleAnimationDuration;
        float elapsedTime = 0;

        spark.transform.localScale = new Vector3(startScale, startScale, startScale);

        while (elapsedTime < sparkScaleAnimationDuration) {
            float scale = sparkScaleAnimationCurve.Evaluate(elapsedTime * speed) * maxScale;
            spark.transform.localScale = new Vector3(scale, scale, scale);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spark.transform.localScale = Vector3.zero;
    }

    private IEnumerator AnimateSparkPosition(float previousHealth01, float currentHealth01) {
        Vector3[] cornersWorldPosition = new Vector3[4];
        fill.rectTransform.GetWorldCorners(cornersWorldPosition);
        float positionX = (cornersWorldPosition[0] + (cornersWorldPosition[3] - cornersWorldPosition[0]) * previousHealth01).x;
        spark.rectTransform.position = new Vector3(positionX + horizontalOffset, spark.rectTransform.position.y + verticalOffset, fill.rectTransform.position.z);

        float distance = Mathf.Abs(currentHealth01 - previousHealth01);
        float speed = distance / sparkPositionAnimationDuration;
        float elapsedTime = 0;

        while (elapsedTime < sparkPositionAnimationDuration) {
            float lerpAlpha = sparkPositionAnimationCurve.Evaluate(elapsedTime * speed) / distance;
            float healthRatio01 = Mathf.Lerp(previousHealth01, currentHealth01, lerpAlpha);

            positionX = (cornersWorldPosition[0] + (cornersWorldPosition[3] - cornersWorldPosition[0]) * healthRatio01).x;
            spark.rectTransform.position = new Vector3( positionX + horizontalOffset,spark.rectTransform.position.y, spark.rectTransform.position.z);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        positionX = (cornersWorldPosition[0] + (cornersWorldPosition[3] - cornersWorldPosition[0]) * currentHealth01).x;
        spark.rectTransform.position = new Vector3(positionX + horizontalOffset, spark.rectTransform.position.y, spark.rectTransform.position.z);
        animateHealthBarCoroutine = null;
    }

    private IEnumerator AnimateSparkGlow() {
        float maxColorComponent = sparkMaterial.color.maxColorComponent;
        float scaleFactor = MAX_BYTE_FOR_OVEREXPOSED_COLOR / maxColorComponent;
        float previousColorIntensity = Mathf.Log(255f / scaleFactor) / Mathf.Log(2f);
        Color color = sparkMaterial.color;
        float currentColorIntensity = previousColorIntensity;

        float glowAnimationElapsedTime = 0;
        float changeGlowSpeed = Mathf.Abs(sparkMaxColorIntensity - previousColorIntensity) / sparkGlowAnimationDuration;

        while (glowAnimationElapsedTime < sparkGlowAnimationDuration) {
            currentColorIntensity = MathUtils.MapRangeClamped(sparkGlowAnimationCurve.Evaluate(glowAnimationElapsedTime),
                                                               0, 1,
                                                               previousColorIntensity, sparkMaxColorIntensity);

            glowAnimationElapsedTime += Time.deltaTime * changeGlowSpeed;
            sparkMaterial.color = color * currentColorIntensity;
            yield return null;
        }

        sparkMaterial.color = color * previousColorIntensity;
        animateSparkGlowCoroutine = null;
    }

}