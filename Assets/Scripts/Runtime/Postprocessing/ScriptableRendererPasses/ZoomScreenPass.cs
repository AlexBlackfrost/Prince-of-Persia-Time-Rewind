using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

public class ZoomScreenPass : ScriptableRenderPass {
    private RenderTargetHandle tempTexture;
    private Material zoomScreenMaterial;
    private ProfilingSampler profilingSampler;

    public ZoomScreenPass(Material zoomScreenMaterial) : base() {
        this.zoomScreenMaterial = zoomScreenMaterial;
        profilingSampler = new ProfilingSampler("ZoomScreen");
        tempTexture.Init("_ZoomedColorTexture");
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
            Blit(commandBuffer, renderingData.cameraData.renderer.cameraColorTarget, tempTexture.Identifier(), zoomScreenMaterial);

        }
        context.ExecuteCommandBuffer(commandBuffer);
        commandBuffer.Clear();
        CommandBufferPool.Release(commandBuffer);
    }

    public override void OnCameraCleanup(CommandBuffer cmd) {
        cmd.ReleaseTemporaryRT(tempTexture.id);
    }
}