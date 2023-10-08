using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;
/// <summary>
/// Save color in global texture "_MainTex" so that it can be accessed from 
/// other screen space shaders in the next renderer passes
/// </summary>
public class SaveColorPass : ScriptableRenderPass {
    private RenderTargetHandle tempTexture;
    private ProfilingSampler profilingSampler;

    public SaveColorPass() : base() {
        // This render target will render to the global shader texture _ColorTexture, which can be accessed from a shader
        tempTexture.Init("_ColorTexture");
        profilingSampler = new ProfilingSampler("SaveColor");
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