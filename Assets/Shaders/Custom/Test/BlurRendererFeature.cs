using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BlurRendererFeature : ScriptableRendererFeature
{
    // settings of the render feature
    [SerializeField] private BlurSettings settings;
    [SerializeField] private Shader shader;

    // position in the stack to apply the effect at
    [SerializeField] private RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    
    // local info, generated material and the actual render pass
    private Material material;
    private BlurRenderPass blurRenderPass;

    // when the render feature gets created
    public override void Create()
    {
        // if there is no shader, just skip
        if (shader == null) return;

        // create a new material
        material = new Material(shader);

        // creates the actual render pass
        blurRenderPass = new(material, settings)
        {
            // tell when to render
            renderPassEvent = renderPassEvent
        };
    }

    // every frame
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // if the render pass is not there, skip
        if (blurRenderPass == null || material == null) return;

        // trigger the rendering of the pass, making sure to only target the game camera
        if (renderingData.cameraData.cameraType == CameraType.Game) renderer.EnqueuePass(blurRenderPass);
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
public struct BlurSettings
{
    [Range(0, 0.4f)] public float horizontalBlur;
    [Range(0, 0.4f)] public float verticalBlur;
}