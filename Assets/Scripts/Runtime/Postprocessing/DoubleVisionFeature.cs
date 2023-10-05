using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DoubleVisionFeature : ScriptableRendererFeature{

    /// <summary>
    /// Save color in global texture "_MainTex" so that it can be accessed from 
    /// the double vision screen space shader in the next pass
    /// </summary>
    public class SaveColorPass : ScriptableRenderPass {
        private RenderTargetHandle tempTexture;
        private ProfilingSampler profilingSampler;

        public SaveColorPass() : base() {
            // This render target will render to the global shader texture _ColorTexture, which can be accessed from a shader
            tempTexture.Init("_ColorTexture");
            profilingSampler = new ProfilingSampler(featureName + "_SaveColor");
        }


        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDesc.depthBufferBits = 0;

            cmd.GetTemporaryRT(tempTexture.id, cameraTextureDesc, FilterMode.Point);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer commandBuffer = CommandBufferPool.Get();
            // Command buffer shouldn't contain anything, but apparently need to
            // execute so DrawRenderers call is put under profiling scope title correctly
            using (new ProfilingScope(commandBuffer, profilingSampler)) {
                context.ExecuteCommandBuffer(commandBuffer);
                commandBuffer.Clear();

                // Save the current rendered screen into the shader global texture assigned to the temporary render target
                Blit(commandBuffer, renderingData.cameraData.renderer.cameraColorTarget, tempTexture.Identifier());
            }
            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
            CommandBufferPool.Release(commandBuffer);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }


    public class DoubleVisionPass : ScriptableRenderPass{
        private Material maskMaterial;
        private Material doubleVisionMaterial;
        private Material gaussianBlurMaterial;
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempTexture;
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

            profilingSampler = new ProfilingSampler(featureName + "_MaskAndRender");
        }

        public void SetSource(RenderTargetIdentifier source) {
            this.source = source;
        }

        public override void OnCameraSetup(CommandBuffer commandBuffer, ref RenderingData renderingData) {
            //Set the background color to black.
            ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData){
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


                // Get an offsetted copy of the player
                RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
                cameraTextureDesc.depthBufferBits = 0;
                commandBuffer.GetTemporaryRT(tempTexture.id, cameraTextureDesc, FilterMode.Point);
                Blit(commandBuffer, source, tempTexture.Identifier(), doubleVisionMaterial, 0); 
                Blit(commandBuffer, tempTexture.Identifier(), source);

                // Apply horizontal gaussian blur
                Blit(commandBuffer, source, tempTexture.Identifier(), gaussianBlurMaterial, 0);
                // Apply vertical gaussian blur
                Blit(commandBuffer, tempTexture.Identifier(), source, gaussianBlurMaterial, 1);
                // Combine the background image and the blurred double vision image
                Blit(commandBuffer, source, tempTexture.Identifier(), gaussianBlurMaterial, 2);
                Blit(commandBuffer, tempTexture.Identifier(), source);
            }
            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
            CommandBufferPool.Release(commandBuffer);
        }

        public override void OnCameraCleanup(CommandBuffer commandBuffer) {
            commandBuffer.ReleaseTemporaryRT(tempTexture.id);
        }
    }

    [SerializeField] private static string featureName = "DoubleVisionEffect";
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Material duplicateMaterial;
    [SerializeField] private Material maskMaterial;
    [SerializeField] private Material gaussianBlurMaterial;
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

    private SaveColorPass saveColorPass;
    private DoubleVisionPass doubleVisionPass;

    public override void Create(){
        saveColorPass = new SaveColorPass();
        saveColorPass.renderPassEvent = renderPassEvent;

        doubleVisionPass = new DoubleVisionPass(maskMaterial, duplicateMaterial, gaussianBlurMaterial, layerMask);
        doubleVisionPass.renderPassEvent = renderPassEvent;

    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData){
        renderer.EnqueuePass(saveColorPass);

        doubleVisionPass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(doubleVisionPass);
    }
}


