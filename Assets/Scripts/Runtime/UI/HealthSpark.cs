using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthSpark : MonoBehaviour{
    [SerializeField] private HealthBar healthBar;
    [SerializeField] private float minHealthPercentVisibility = 0.3f;
    [SerializeField] private float maxHealthPercentVisibility = 1.0f;
    [SerializeField] private float horizontalOffset = 0.0f;
    [SerializeField] private float verticalOffset = -0.2f;
    [SerializeField] private AnimationCurve scaleAnimationCurve;
    [SerializeField] private float scaleAnimationDuration;
    [SerializeField] private float startScale =0.0f;
    [SerializeField] private float maxScale = 1.5f;


    private Image spark;
    private Coroutine scaleAnimationCoroutine;


    private void Awake() {
        spark = GetComponent<Image>();
    }


    public void OnHealthBarAnimationEnded(float barCompletionRatio, Vector3 position) {
        if(TimeRewindManager.Instance.IsRewinding && barCompletionRatio >= minHealthPercentVisibility && barCompletionRatio <= maxHealthPercentVisibility) {
            ShowSpark(barCompletionRatio, position);
        }
    }

    private void ShowSpark(float barCompletionRatio, Vector3 rectPosition) {
        if(scaleAnimationCoroutine != null) {
            StopCoroutine(scaleAnimationCoroutine);
            scaleAnimationCoroutine = null;
        }

        spark.rectTransform.position = new Vector3(rectPosition.x + horizontalOffset, spark.rectTransform.position.y + verticalOffset, rectPosition.z);
        scaleAnimationCoroutine = StartCoroutine(AnimateSparkScale());
    }



    private IEnumerator AnimateSparkScale() {
        float speed = (maxScale - startScale) / scaleAnimationDuration;

        float elapsedTime = 0;
        spark.transform.localScale = new Vector3(startScale, startScale, startScale);
        while(elapsedTime < scaleAnimationDuration) {
            float scale = scaleAnimationCurve.Evaluate(elapsedTime*speed)*maxScale;
            spark.transform.localScale = new Vector3(scale, scale, scale);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spark.transform.localScale = new Vector3(startScale,startScale, startScale);
    }
}