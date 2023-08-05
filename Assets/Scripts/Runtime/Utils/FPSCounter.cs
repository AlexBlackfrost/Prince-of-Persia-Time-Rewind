using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSCounter : MonoBehaviour{

    private int N = 5;
    private float[] lastNFPS;
    private int currentIndex = 0;
    private float averageFPS;
    private float waitDisplayTime = 0.05f;

    private void Awake() {
        lastNFPS = new float[N];
    }



    private IEnumerator Start(){
        while (true) {
            CountFPS();
            yield return new WaitForSeconds(waitDisplayTime);
        }
        
    }

    private void CountFPS() {
        lastNFPS[currentIndex] = 1 / Time.unscaledDeltaTime;
        currentIndex = (currentIndex + 1) % lastNFPS.Length;

        averageFPS = 0;
        foreach (float fps in lastNFPS) {
            averageFPS += fps;
        }
        averageFPS /= lastNFPS.Length;
    }

    private void OnGUI() {
        GUI.Label(new Rect(5, 40, 100, 25), "FPS: " + Mathf.Round(averageFPS));
    }
}
