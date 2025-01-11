using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Lyrith
{
    public class EmptyRenderFeature : ScriptableRendererFeature
    {
        // the target of the position in the post processing stack
        [SerializeField] RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        // local info, generated material and the actual render pass
        private Material material;
        private EmptyRenderPass renderPass;

        // when the render feature gets created
        public override void Create()
        {
            // create a new material
            material = new Material(Shader.Find("SHADERHERE"));

            // creates the actual render pass
            renderPass = new(material)
            {
                renderPassEvent = renderPassEvent
            };
        }

        // every frame
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // if the render pass is not there, skip
            if (renderPass == null || material == null) return;

            // trigger the rendering of the pass, making sure to only target the game camera
            if (renderingData.cameraData.cameraType == CameraType.Game) renderer.EnqueuePass(renderPass);
        }

        // make sure to dispose the material, otherwise memory leak
        protected override void Dispose(bool disposing)
        {
            if (Application.isPlaying) Destroy(material);
            else DestroyImmediate(material);
        }
    }
}