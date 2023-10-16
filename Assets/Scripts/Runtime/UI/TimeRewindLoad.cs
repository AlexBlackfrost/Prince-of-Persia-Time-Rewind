using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeRewindLoad : MonoBehaviour{
    [SerializeField] private PlayerTimeRewinder playerTimeRewinder;
    private Image image;

    private void Awake(){
        image = GetComponent<Image>();
    }

    private void Update(){
        image.fillAmount = playerTimeRewinder.GetRecordedDataRatio01();

    }
}