using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PosturizeRendererFeature : ScriptableRendererFeature
{
    // settings of the render feature
    [SerializeField] private PosturizeSettings settings;
    [SerializeField] private Shader shader;

    // position in the stack to apply the effect at
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    
    // local info, generated material and the actual render pass
    private Material material;
    private PosturizeRenderPass posturizeRenderPass;

    // when the render feature gets created
    public override void Create()
    {
        // if there is no shader, just skip
        if (shader == null) return;

        // create a new material
        material = new Material(shader);

        if (material == null) return;

        // creates the actual render pass
        posturizeRenderPass = new(material, settings)
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

    // make sure to dispose the material, otherwise memory leak
    protected override void Dispose(bool disposing)
    {
        if (Application.isPlaying) Destroy(material);
        else DestroyImmediate(material);
    }
}


// container for the data, for easy transfer
[Serializable]
public struct PosturizeSettings
{
    [Range(2, 128)] public int Steps;
}