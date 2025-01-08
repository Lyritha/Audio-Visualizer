using UnityEngine;

public class LowResolutionRenderer : MonoBehaviour
{
    #if !UNITY_EDITOR

    public int targetWidth = 160; // Low resolution width
    public int targetHeight = 90; // Low resolution height

    private RenderTexture lowResRenderTexture;

    void Start()
    {
        // Create a low-resolution render texture
        lowResRenderTexture = new RenderTexture(targetWidth, targetHeight, 16);
        lowResRenderTexture.filterMode = FilterMode.Point; // Ensures sharp edges when upscaled
        lowResRenderTexture.useMipMap = false;

        // Set the render texture as the active target for rendering
        Camera.main.targetTexture = lowResRenderTexture;
    }

    void OnGUI()
    {
        // Draw the low-resolution render texture on the screen
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), lowResRenderTexture);
    }

    void OnDestroy()
    {
        // Clean up the render texture when the game stops
        if (lowResRenderTexture != null)
        {
            lowResRenderTexture.Release();
        }
    }

    #endif
}
