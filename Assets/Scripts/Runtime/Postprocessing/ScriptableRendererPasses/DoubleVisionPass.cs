using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

public class DoubleVisionPass : ScriptableRenderPass {
    private Material maskMaterial;
    private Material doubleVisionMaterial;
    private Material gaussianBlurMaterial;
    private RenderTargetIdentifier source;
    private RenderTargetHandle tempTexture;
    private RenderTargetHandle maskTexture;
    private FilteringSettings filteringSettings;
    private List<ShaderTagId> shaderTagsList = new List<ShaderTagId>();
    private ProfilingSampler profilingSampler;

    /// <summary>
    /// Draw a binary mask to filter objects and then use it to draw a double vision effect on those
    /// objects.
    /// </summary>
    /// <param name="maskMaterial"> Material used to mask the objects we want to render with a double vission effect</param>
    /// <param name="duplicateMaterial"> Material used to apply the double vision effect.</param>
    /// <param name="layerMask"> Layer mask used to filter which objects are going to be masked</param>
    public DoubleVisionPass(Material maskMaterial, Material duplicateMaterial, Material gaussianBlurMaterial, LayerMask layerMask) : base() {
        this.maskMaterial = maskMaterial;
        this.doubleVisionMaterial = duplicateMaterial;
        this.gaussianBlurMaterial = gaussianBlurMaterial;

        shaderTagsList.Add(new ShaderTagId("UniversalForward"));
        shaderTagsList.Add(new ShaderTagId("LightweightForward"));
        shaderTagsList.Add(new ShaderTagId("SRPDefaultUnlit"));

        filteringSettings = new FilteringSettings(RenderQueueRange.all, layerMask);

        profilingSampler = new ProfilingSampler("DoubleVision");
        maskTexture.Init("_MaskTexture");
        tempTexture.Init("_MainTexture");
    }

    public void SetSource(RenderTargetIdentifier source) {
        this.source = source;
    }

    public override void OnCameraSetup(CommandBuffer commandBuffer, ref RenderingData renderingData) {
        //Set the background color to black.
        ConfigureClear(ClearFlag.All, Color.black);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        CommandBuffer commandBuffer = CommandBufferPool.Get();
        // Command buffer shouldn't contain anything, but apparently need to
        // execute so DrawRenderers call is put under profiling scope title correctly
        using (new ProfilingScope(commandBuffer, profilingSampler)) {
            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();

            // Draw the objects in filtering Settings layerMask in white, generating a binary mask
            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagsList, ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = maskMaterial;
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);


            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDesc.depthBufferBits = 0;
            commandBuffer.GetTemporaryRT(tempTexture.id, cameraTextureDesc, FilterMode.Point);
            commandBuffer.GetTemporaryRT(maskTexture.id, cameraTextureDesc, FilterMode.Point);

            commandBuffer.SetGlobalTexture("_MaskTexture", maskTexture.Identifier());

            // Save the binary mask used to filter the objects we don't want to be affected by the postpro effect
            Blit(commandBuffer, source, maskTexture.Identifier());

            // Applies the binary mask
            Blit(commandBuffer, source, tempTexture.Identifier(), doubleVisionMaterial, 0);

            // Apply horizontal gaussian blur
            Blit(commandBuffer, tempTexture.Identifier(), source, gaussianBlurMaterial, 0);

            // Apply vertical gaussian blur
            Blit(commandBuffer, source, tempTexture.Identifier(), gaussianBlurMaterial, 1);

            // Blend the background image and the blurred double vision image
            Blit(commandBuffer, tempTexture.Identifier(), source, gaussianBlurMaterial, 2);

        }
        context.ExecuteCommandBuffer(commandBuffer);
        commandBuffer.Clear();
        CommandBufferPool.Release(commandBuffer);
    }

    public override void OnCameraCleanup(CommandBuffer commandBuffer) {
        commandBuffer.ReleaseTemporaryRT(tempTexture.id);
    }
}