using UnityEngine;

public class ForceResolution : MonoBehaviour
{
    void Awake()
    {
        // Set the desired resolution
        int targetWidth = 1280;
        int targetHeight = 720;
        bool isFullScreen = true;

        Screen.SetResolution(targetWidth, targetHeight, isFullScreen);
    }
}