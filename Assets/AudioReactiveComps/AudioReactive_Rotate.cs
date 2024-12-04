using UnityEngine;

public class AudioReactive_Rotate : AudioReactiveComponent
{
    [Header("Rotate data")]
    [SerializeField] private int targetBandX = 0;
    [SerializeField] private int targetBandY = 0;
    [SerializeField] private int targetBandZ = 0;

    [SerializeField] private Vector3 rotationSpeed;
    [SerializeField] private Vector3 minRotationSpeed;
    [SerializeField] private Vector3 maxRotationSpeed;

    private void OnValidate()
    {
        if (audioAnalyzer != null)
        {
            targetBandX = Mathf.Clamp(targetBandX, 0, audioAnalyzer.BandWidth);
            targetBandY = Mathf.Clamp(targetBandY, 0, audioAnalyzer.BandWidth);
            targetBandZ = Mathf.Clamp(targetBandZ, 0, audioAnalyzer.BandWidth);
        }
    }

    // put any audio reactive effects in this, no need to worry about execution order
    protected override void AudioReaction()
    {
        Vector3 rotation = Vector3.zero;
        rotation.x = Mathf.Clamp(audioData[targetBandX] * rotationSpeed.x, minRotationSpeed.x, maxRotationSpeed.x);
        rotation.y = Mathf.Clamp(audioData[targetBandY] * rotationSpeed.y, minRotationSpeed.y, maxRotationSpeed.y);
        rotation.z = Mathf.Clamp(audioData[targetBandZ] * rotationSpeed.z, minRotationSpeed.z, maxRotationSpeed.z);

        transform.Rotate(rotation);
    }
}
