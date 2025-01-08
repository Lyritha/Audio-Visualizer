using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PosterizeFeature : ScriptableRendererFeature
{
    class PosterizePass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle tempTexture;
        private string profilerTag;

        public PosterizePass(Material material, string tag)
        {
            this.material = material;
            this.profilerTag = tag;
        }

        [System.Obsolete]
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
            RenderingUtils.ReAllocateHandleIfNeeded(ref tempTexture, descriptor, name: "_TemporaryColorTexture");
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null) return;

            CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;

            Blit(cmd, source, tempTexture, material);
            Blit(cmd, tempTexture, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            tempTexture?.Release();
        }
    }

    [SerializeField] private Material material;
    [SerializeField] private string profilerTag = "Posterize Effect";

    private PosterizePass posterizePass;

    public override void Create()
    {
        posterizePass = new PosterizePass(material, profilerTag);
        posterizePass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (material != null)
        {
            renderer.EnqueuePass(posterizePass);
        }
    }
}
