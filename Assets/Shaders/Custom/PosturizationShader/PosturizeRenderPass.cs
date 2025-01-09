using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class PosturizeRenderPass : ScriptableRenderPass
{
    // shader properties
    private static readonly int stepsId = Shader.PropertyToID("_Steps");
    private const string k_PassName = "PosturizeRenderPass";

    private PosturizeSettings defaultSettings;
    private Material material;

    private RenderTextureDescriptor textureDescriptor;

    public PosturizeRenderPass(Material material, PosturizeSettings defaultSettings)
    {
        this.material = material;
        this.defaultSettings = defaultSettings;

        textureDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height,
        RenderTextureFormat.Default, 0);
    }

    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        // get all the render textures to do with the urp pipeline
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        // grab the color texture
        TextureHandle srcCamColor = resourceData.activeColorTexture;
        TextureHandle dst = UniversalRenderer.CreateRenderGraphTexture(renderGraph, textureDescriptor, "PosturizeTex", false);

        // get the info from the active camera
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        // The following line ensures that the render pass doesn't blit from the back buffer.
        if (resourceData.isActiveTargetBackBuffer) return;

        // Set the texture of this path to the same size as the camera
        textureDescriptor.width = cameraData.cameraTargetDescriptor.width;
        textureDescriptor.height = cameraData.cameraTargetDescriptor.height;
        textureDescriptor.depthBufferBits = 0;

        // Update the settings
        UpdateSettings();

        // Validate textures
        if (!srcCamColor.IsValid() || !dst.IsValid()) return;


        // The AddBlitPass method triggers the rendering of this pass
        RenderGraphUtils.BlitMaterialParameters paraPosturize = new(srcCamColor, dst, material, 0);
        renderGraph.AddBlitPass(paraPosturize, k_PassName);

        // Write the result back to the active color texture
        RenderGraphUtils.BlitMaterialParameters paraWriteBack = new(dst, srcCamColor, material, 0);
        renderGraph.AddBlitPass(paraWriteBack, "WriteBackToCamera");
    }

    private void UpdateSettings()
    {
        if (material == null) return;

        // Use the Volume settings or the default settings if no Volume is set.
        PosturizeVolumeComponent volumeComponent = VolumeManager.instance.stack.GetComponent<PosturizeVolumeComponent>();

        // if volume is overriding, use volume value, if not used render feature value
        int steps = volumeComponent.steps.overrideState ? volumeComponent.steps.value : defaultSettings.Steps;

        // set values of the material
        material.SetInteger(stepsId, steps);

    }
}