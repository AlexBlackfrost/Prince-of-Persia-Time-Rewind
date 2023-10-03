using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DistanceFogFeature : ScriptableRendererFeature{
    [SerializeField] private static string featureName = "DistanceFog";
    [SerializeField] private Material distanceFogMaterial;
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    public class CustomRenderPass : ScriptableRenderPass{
        private Material material;
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempTexture;
        private FilteringSettings filteringSettings;
        private List<ShaderTagId> shaderTagsList = new List<ShaderTagId>();

        public CustomRenderPass(Material material) : base() {
            this.material = material;
            tempTexture.Init("_MainTex"); // Using a name different than _MainTex does not work for some reason

            shaderTagsList.Add(new ShaderTagId("UniversalForward"));
            shaderTagsList.Add(new ShaderTagId("LightweightForward"));
            shaderTagsList.Add(new ShaderTagId("SRPDefaultUnlit"));

            filteringSettings = FilteringSettings.defaultValue;
        }

        public void SetSource(RenderTargetIdentifier source) {
            this.source = source;
        }
        public override void OnCameraSetup(CommandBuffer commandBuffer, ref RenderingData renderingData) {
            //Set the background color to black.
            //ConfigureClear(ClearFlag.All, Color.red); 
            //ConfigureTarget(source);
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData){
            CommandBuffer commandBuffer = CommandBufferPool.Get(featureName);
            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();

            //Shader.SetGlobalMatrix("_InverseViewMatrix", renderingData.cameraData.camera.cameraToWorldMatrix);
            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDesc.depthBufferBits = 32;
            
            commandBuffer.GetTemporaryRT(tempTexture.id , cameraTextureDesc, FilterMode.Trilinear);

            material.SetMatrix("_InverseViewMatrix", renderingData.cameraData.camera.cameraToWorldMatrix);
            Blit(commandBuffer, source, tempTexture.Identifier(), material, 0);
            Blit(commandBuffer, tempTexture.Identifier(), source);

            context.ExecuteCommandBuffer(commandBuffer);
            commandBuffer.Clear();
            CommandBufferPool.Release(commandBuffer);
        }

        public override void FrameCleanup(CommandBuffer cmd) {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }

    private CustomRenderPass renderPass;

    public override void Create(){
        renderPass = new CustomRenderPass(distanceFogMaterial);
        renderPass.renderPassEvent = renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData){
        renderPass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(renderPass);
    }
}


