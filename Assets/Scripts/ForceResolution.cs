using UnityEngine;

public class ForceResolution : MonoBehaviour
{
    void Awake()
    {
        // Set the desired resolution
        int targetWidth = 640;
        int targetHeight = 360;
        bool isFullScreen = true;

        Screen.SetResolution(targetWidth, targetHeight, isFullScreen);
    }
}