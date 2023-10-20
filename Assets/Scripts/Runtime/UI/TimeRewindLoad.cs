using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeRewindLoad : MonoBehaviour{
    [SerializeField] private PlayerTimeRewinder playerTimeRewinder;
    [SerializeField] private float glowAnimationDuration = 0.75f;
    [SerializeField] private AnimationCurve glowAnimationCurve;
    [SerializeField] private float defaultColorIntensity = 1.0f;
    [SerializeField] private float maxColorIntensity = 1.2f;

    private Image image;
    private Material material;
    private Coroutine animateGlowCoroutine;
    private bool isGlowing = false;

    private void Awake(){
        image = GetComponent<Image>();
        material = image.material;
    }

    private void Update(){
        if (!TimeRewindManager.Instance.IsRewinding) {
            if (!isGlowing && image.fillAmount != 1) {
                if (animateGlowCoroutine != null) {
                    StopCoroutine(animateGlowCoroutine);
                }
                animateGlowCoroutine = StartCoroutine(AnimateGlow(maxColorIntensity));
                isGlowing = true;

            } else if (isGlowing && image.fillAmount == 1) {
                if (animateGlowCoroutine != null) {
                    StopCoroutine(animateGlowCoroutine);
                }
                animateGlowCoroutine = StartCoroutine(AnimateGlow(defaultColorIntensity));
                isGlowing = false;
            }
        } 

        image.fillAmount = playerTimeRewinder.GetRecordedDataRatio01();
    }

    private IEnumerator AnimateGlow(float targetColorIntensity) {
        /* Tip for next time: use a target color and define it inspector instead of just the intensity, dealing  with color changes is a pain.
         * Useful links:
         * https://forum.unity.com/threads/how-to-change-hdr-colors-intensity-via-shader.531861/
         * https://forum.unity.com/threads/understanding-srgb-and-gamma-corrected-values-in-the-render-pipeline.783224/
         * https://discussions.unity.com/t/how-to-get-set-hdr-color-intensity/226028/5
         */

        Color32 originalColor;
        float previousColorIntensity;
        MathUtils.DecomposeHdrColor(material.color, out originalColor, out previousColorIntensity);
        Color originalColorScaled = new Color(originalColor.r / 255.0f *2.0f, originalColor.g / 255.0f *2.0f, originalColor.b / 255.0f * 2.0f, originalColor.a);
        Color originalColorGammaSpace = new Color(Mathf.LinearToGammaSpace(originalColorScaled.r),
                                                  Mathf.LinearToGammaSpace(originalColorScaled.g),
                                                  Mathf.LinearToGammaSpace(originalColorScaled.b),
                                                  material.color.a);

        float changeGlowSpeed = Mathf.Abs(targetColorIntensity - previousColorIntensity) / glowAnimationDuration;
        float glowAnimationElapsedTime = 0;

        while (glowAnimationElapsedTime < glowAnimationDuration) {
            float lerpAlpha = glowAnimationCurve.Evaluate(glowAnimationElapsedTime * changeGlowSpeed);
            float currentColorIntensity = Mathf.Lerp(previousColorIntensity, targetColorIntensity, lerpAlpha);

            float scaledIntensity = Mathf.LinearToGammaSpace(Mathf.Pow(2.0f, currentColorIntensity-1));
            material.color = new Color(Mathf.GammaToLinearSpace(originalColorGammaSpace.r * scaledIntensity),
                                       Mathf.GammaToLinearSpace(originalColorGammaSpace.g * scaledIntensity),
                                       Mathf.GammaToLinearSpace(originalColorGammaSpace.b * scaledIntensity),
                                       material.color.a);

            glowAnimationElapsedTime += Time.deltaTime;
            // Be careful with linear and gamma color spaces https://forum.unity.com/threads/how-to-change-hdr-colors-intensity-via-shader.531861/#post-3501895
            yield return null;
        }

        float targetIntensity = Mathf.LinearToGammaSpace(Mathf.Pow(2.0f, targetColorIntensity - 1));
        material.color = new Color(Mathf.GammaToLinearSpace(originalColorGammaSpace.r * targetIntensity),
                                   Mathf.GammaToLinearSpace(originalColorGammaSpace.g * targetIntensity),
                                   Mathf.GammaToLinearSpace(originalColorGammaSpace.b * targetIntensity),
                                   material.color.a);
        animateGlowCoroutine = null;
    }
}