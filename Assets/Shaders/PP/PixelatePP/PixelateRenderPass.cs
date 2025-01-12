using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Lyrith
{
    public class PixelateRenderPass : ScriptableRenderPass
    {
        // shader properties
        private static readonly int pixelSizeID = Shader.PropertyToID("_PixelSize");

        // reference to the material with the shader
        private Material material;
        private RenderTextureDescriptor textureDescriptor;

        private bool applyPixelization = false;

        public PixelateRenderPass(Material material)
        {
            this.material = material;

            textureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height,
            RenderTextureFormat.Default, 0);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Update the settings
            UpdateSettings();

            // Skip the pass entirely if pixelization should not be applied.
            if (!applyPixelization || material == null) return;

            // get all the render textures to do with the urp pipeline
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            // grab the color texture and create a new texture to render to
            TextureHandle src = resourceData.activeColorTexture;
            TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(renderGraph, textureDescriptor, "PosturizeTex", false);

            // The following line ensures that the render pass doesn't blit from the back buffer.
            if (resourceData.isActiveTargetBackBuffer) return;

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            textureDescriptor.width = cameraData.cameraTargetDescriptor.width;
            textureDescriptor.height = cameraData.cameraTargetDescriptor.height;
            textureDescriptor.colorFormat = RenderTextureFormat.DefaultHDR;
            textureDescriptor.depthBufferBits = 0;

            // Validate textures, and render
            if (!src.IsValid() || !dst.IsValid()) return;

            // The AddBlitPass method triggers the rendering of this pass
            RenderGraphUtils.BlitMaterialParameters paraPosturize = new(src, dst, material, 0);
            renderGraph.AddBlitPass(paraPosturize, "PosturizeRenderPass");

            // Write the result back to the active color texture
            RenderGraphUtils.BlitMaterialParameters paraWriteBack = new(dst, src, material, 0);
            renderGraph.AddBlitPass(paraWriteBack, "WriteBackToCamera");
        }

        private void UpdateSettings()
        {
            if (material == null) return;

            // Use the Volume settings or the default settings if no Volume is set.
            PixelateVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<PixelateVolumeComponent>();

            // Check if the overrideState is true, and only then enable posturization.
            applyPixelization = volumeComponent != null && volumeComponent.pixelSize.overrideState;

            // set values of the material
            if (applyPixelization)
                material.SetFloat(pixelSizeID, volumeComponent.pixelSize.value);
        }
    }
}