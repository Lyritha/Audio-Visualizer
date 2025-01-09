using UnityEngine;

public class ForceResolution : MonoBehaviour
{
    void Start()
    {
        // Set resolution to 1920x1080, Fullscreen mode
        Screen.SetResolution(1920, 1080, true);
    }
}