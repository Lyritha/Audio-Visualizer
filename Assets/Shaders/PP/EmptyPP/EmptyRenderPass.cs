using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Lyrith
{
    public class EmptyRenderPass : ScriptableRenderPass
    {
        // shader properties, will target a parameter inside the hlsl
        private static readonly int exampleID = Shader.PropertyToID("_EXAMPLE");

        // reference to the material with the shader
        private Material material;

        // texture in which to save a render in
        private RenderTextureDescriptor textureDescriptor;

        public EmptyRenderPass(Material material)
        {
            this.material = material;

            textureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height,
            RenderTextureFormat.Default, 0);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Update the settings
            UpdateSettings();

            // get all the render textures to do with the urp pipeline
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            // grab the color texture and create a new texture to render to
            TextureHandle srcCamColor = resourceData.activeColorTexture;
            TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(renderGraph, textureDescriptor, "PosturizeTex", false);

            // The following line ensures that the render pass doesn't blit from the back buffer. (dunno what this means)
            if (resourceData.isActiveTargetBackBuffer) return;

            // Set the texture of this path to the same size as the camera
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            textureDescriptor.width = cameraData.cameraTargetDescriptor.width;
            textureDescriptor.height = cameraData.cameraTargetDescriptor.height;
            textureDescriptor.depthBufferBits = 0;


            // Validate textures
            if (!srcCamColor.IsValid() || !dst.IsValid()) return;


            // The AddBlitPass method triggers the rendering of this pass
            RenderGraphUtils.BlitMaterialParameters paraPosturize = new(srcCamColor, dst, material, 0);
            renderGraph.AddBlitPass(paraPosturize, "RenderPass");

            // Write the result back to the active color texture
            RenderGraphUtils.BlitMaterialParameters paraWriteBack = new(dst, srcCamColor, material, 0);
            renderGraph.AddBlitPass(paraWriteBack, "WriteBackToCamera");
        }

        private void UpdateSettings()
        {
            // make sure the material isn't missing, otherwise won't build
            if (material == null) return;

            // Get the volume settings
            EmptyVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<EmptyVolumeComponent>();
        }
    }
}