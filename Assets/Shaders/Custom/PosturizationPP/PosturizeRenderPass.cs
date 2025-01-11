using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace Lyrith
{
    public class PosturizeRenderPass : ScriptableRenderPass
    {
        // shader properties
        private static readonly int stepsId = Shader.PropertyToID("_Steps");

        // reference to the material with the shader
        private Material material;


        private RenderTextureDescriptor textureDescriptor;

        private bool applyPosturization = false;
        private int targetPass = 0;

        public PosturizeRenderPass(Material material)
        {
            this.material = material;

            textureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height,
            RenderTextureFormat.Default, 0);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            // Update the settings
            UpdateSettings();

            // Skip the pass entirely if posturization should not be applied.
            if (!applyPosturization) return;

            // get all the render textures to do with the urp pipeline
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            // grab the color texture and create a new texture to render to
            TextureHandle src = resourceData.activeColorTexture;
            TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(renderGraph, textureDescriptor, "PosturizeTex", false);

            // The following line ensures that the render pass doesn't blit from the back buffer.
            if (resourceData.isActiveTargetBackBuffer) return;

            // Set the texture of this path to the same size as the camera
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            textureDescriptor.width = cameraData.cameraTargetDescriptor.width;
            textureDescriptor.height = cameraData.cameraTargetDescriptor.height;
            textureDescriptor.depthBufferBits = 0;


            // Set the texture of this path to the same size as the camera
            PrepareTextureDescriptor(frameData);

            // Validate textures, and render
            if (src.IsValid() && dst.IsValid())
                Render(renderGraph, src, dst);
        }

        private void PrepareTextureDescriptor(ContextContainer frameData)
        {

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
            textureDescriptor.width = cameraData.cameraTargetDescriptor.width;
            textureDescriptor.height = cameraData.cameraTargetDescriptor.height;
            textureDescriptor.colorFormat = RenderTextureFormat.DefaultHDR;
            textureDescriptor.depthBufferBits = 0;
        }

        private void Render(RenderGraph renderGraph, TextureHandle src, TextureHandle dst)
        {
            // The AddBlitPass method triggers the rendering of this pass
            RenderGraphUtils.BlitMaterialParameters paraPosturize = new(src, dst, material, targetPass);
            renderGraph.AddBlitPass(paraPosturize, "PosturizeRenderPass");

            // Write the result back to the active color texture
            RenderGraphUtils.BlitMaterialParameters paraWriteBack = new(dst, src, material, 0);
            renderGraph.AddBlitPass(paraWriteBack, "WriteBackToCamera");
        }

        private void UpdateSettings()
        {
            if (material == null) return;

            // Use the Volume settings or the default settings if no Volume is set.
            PosturizeVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<PosturizeVolumeComponent>();

            // Check if the overrideState is true, and only then enable posturization.
            applyPosturization = volumeComponent != null && volumeComponent.steps.overrideState;

            targetPass = volumeComponent.colorSpace.overrideState ? (int)volumeComponent.colorSpace.value : 0;

            // set values of the material
            if (applyPosturization)
                material.SetInteger(stepsId, volumeComponent.steps.value);
        }
    }
}