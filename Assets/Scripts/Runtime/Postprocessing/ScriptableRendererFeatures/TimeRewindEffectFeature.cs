using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TimeRewindEffectFeature : ScriptableRendererFeature{
    [SerializeField] public static string featureName = "TimeRewindEffect";
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Material duplicateMaterial;
    [SerializeField] private Material maskMaterial;
    [SerializeField] private Material gaussianBlurMaterial;
    [SerializeField] private Material zoomScreenMaterial;
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

    private SaveColorPass saveColorPass;
    private DoubleVisionPass doubleVisionPass;
    private ZoomScreenPass zoomScreenPass;

    public override void Create(){
        saveColorPass = new SaveColorPass();
        saveColorPass.renderPassEvent = renderPassEvent;
        
        zoomScreenPass = new ZoomScreenPass(zoomScreenMaterial);
        zoomScreenPass.renderPassEvent = renderPassEvent;

        doubleVisionPass = new DoubleVisionPass(maskMaterial, duplicateMaterial, gaussianBlurMaterial, layerMask);
        doubleVisionPass.renderPassEvent = renderPassEvent;

    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData){
        renderer.EnqueuePass(saveColorPass);
        renderer.EnqueuePass(zoomScreenPass);

        doubleVisionPass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(doubleVisionPass);
    }

}


