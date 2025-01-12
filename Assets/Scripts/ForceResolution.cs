using UnityEngine;

public class ForceResolution : MonoBehaviour
{
    void Awake()
    {
        Screen.SetResolution(2560, 1440, true);
        Application.runInBackground = true;

        // Set the game to start on the primary monitor
        Display.main.SetRenderingResolution(Screen.width, Screen.height);
    }
}