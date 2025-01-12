using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Lyrith
{
    public class PixelateRenderFeature : ScriptableRendererFeature
    {
        [SerializeField] RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;

        // local info, generated material and the actual render pass
        [SerializeField] private Material material;
        private PixelateRenderPass pixelateRenderPass;

        // when the render feature gets created
        public override void Create()
        {
            // creates the actual render pass
            pixelateRenderPass = new(material)
            {
                // tell when to render in the stack
                renderPassEvent = renderPassEvent
            };
        }

        // every frame
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // if the render pass is not there, skip
            if (pixelateRenderPass == null || material == null) return;

            // trigger the rendering of the pass, making sure to only target the game camera
            if (renderingData.cameraData.cameraType == CameraType.Game) renderer.EnqueuePass(pixelateRenderPass);
        }
    }
}