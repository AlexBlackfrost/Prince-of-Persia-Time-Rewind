using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DuplicatedEffectFeature : ScriptableRendererFeature{
    [SerializeField] private static string featureName = "DuplicatedEffect";
    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Material duplicateMaterial;
    [SerializeField] private Material maskMaterial;
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

    public class MaskPass : ScriptableRenderPass{
        private Material maskMaterial;
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempTexture;
        private FilteringSettings filteringSettings;

        private List<ShaderTagId> shaderTagsList = new List<ShaderTagId>();
        public MaskPass(Material material, LayerMask layerMask) : base() {
            this.maskMaterial = material;

            shaderTagsList.Add(new ShaderTagId("UniversalForward"));
            shaderTagsList.Add(new ShaderTagId("LightweightForward"));
            shaderTagsList.Add(new ShaderTagId("SRPDefaultUnlit"));
            
            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, layerMask);
        }

        public void SetSource(RenderTargetIdentifier source) {
            this.source = source;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
            //cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor, FilterMode.Point);
            //ConfigureClear(ClearFlag.All, Color.black);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDesc.depthBufferBits = 32;

            //cmd.GetTemporaryRT(tempTexture.id, cameraTextureDesc, FilterMode.Point);
            //ConfigureTarget(tempTexture.Identifier());
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData){
            CommandBuffer commandBuffer = CommandBufferPool.Get();
            // Command buffer shouldn't contain anything, but apparently need to
            // execute so DrawRenderers call is put under profiling scope title correctly
            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();

            // Now draw the objects in filtering Settings layerMask in white
            SortingCriteria sortingCriteria = renderingData.cameraData.defaultOpaqueSortFlags;
            DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagsList, ref renderingData, sortingCriteria);
            drawingSettings.overrideMaterial = maskMaterial;
            //drawingSettings.overrideMaterialPassIndex = 2;
            context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref filteringSettings);

            // Pass our custom target to shaders as a Global Texture reference
            // In a Shader Graph, you'd obtain this as a Texture2D property with "Exposed" unticked
            commandBuffer.SetGlobalTexture("_MaskTex", source);

            // Apply material (e.g. Fullscreen Graph) to camera


            // Execute Command Buffer one last time and release it
            // (otherwise we get weird recursive list in Frame Debugger)
            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
            CommandBufferPool.Release(commandBuffer);
        }

        public override void FrameCleanup(CommandBuffer cmd) {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }

    public class DuplicatePass : ScriptableRenderPass {
        private Material duplicateMaterial;
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempTexture;
        private List<ShaderTagId> shaderTagsList = new List<ShaderTagId>();
        private FilteringSettings filteringSettings;

        public DuplicatePass(Material material) : base() {
            this.duplicateMaterial = material;

            shaderTagsList.Add(new ShaderTagId("UniversalForward"));
            shaderTagsList.Add(new ShaderTagId("LightweightForward"));
            shaderTagsList.Add(new ShaderTagId("SRPDefaultUnlit"));

            filteringSettings = FilteringSettings.defaultValue;
        }

        public void SetSource(RenderTargetIdentifier source) {
            this.source = source;
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor) {
            cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor, FilterMode.Point);
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
            ConfigureTarget(tempTexture.Identifier());

        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            CommandBuffer commandBuffer = CommandBufferPool.Get(featureName);
            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();

            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDesc.depthBufferBits = 0;

            commandBuffer.GetTemporaryRT(tempTexture.id, cameraTextureDesc, FilterMode.Bilinear);
  
            Blit(commandBuffer, source, tempTexture.Identifier(), duplicateMaterial, 0);
            Blit(commandBuffer, tempTexture.Identifier(), source);

            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
            CommandBufferPool.Release(commandBuffer);
        }

        public override void FrameCleanup(CommandBuffer cmd) {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }

    private MaskPass maskPass;
    private DuplicatePass duplicatePass;

    public override void Create(){
        maskPass = new MaskPass(maskMaterial,layerMask);
        maskPass.renderPassEvent = renderPassEvent;
        
        duplicatePass = new DuplicatePass(duplicateMaterial);
        duplicatePass.renderPassEvent = renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData){
        maskPass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(maskPass);
        duplicatePass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(duplicatePass);
    }
}


