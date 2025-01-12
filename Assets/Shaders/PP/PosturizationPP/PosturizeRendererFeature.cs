using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Lyrith
{
    public class PosturizeRendererFeature : ScriptableRendererFeature
    {
        [SerializeField] RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        // local info, generated material and the actual render pass
        [SerializeField] private Material material;
        private PosturizeRenderPass posturizeRenderPass;

        // when the render feature gets created
        public override void Create()
        {
            // creates the actual render pass
            posturizeRenderPass = new(material)
            {
                // tell when to render in the stack
                renderPassEvent = renderPassEvent
            };
        }

        // every frame
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // if the render pass is not there, skip
            if (posturizeRenderPass == null || material == null) return;

            // trigger the rendering of the pass, making sure to only target the game camera
            if (renderingData.cameraData.cameraType == CameraType.Game) renderer.EnqueuePass(posturizeRenderPass);
        }
    }
}