using UnityEngine;

public class AudioReactive_Rotate : AudioReactiveComponent
{
    [Header("Rotate data")]
    [SerializeField] private Vector3Int targetBands = Vector3Int.zero;

    [SerializeField] private Vector3 rotationSpeed;
    [SerializeField] private Vector3 minRotationSpeed;
    [SerializeField] private Vector3 maxRotationSpeed;

    private void OnValidate()
    {
        if (audioAnalyzer != null)
        {
            targetBands.x = Mathf.Clamp(targetBands.x, 0, audioAnalyzer.BandWidth);
            targetBands.y = Mathf.Clamp(targetBands.y, 0, audioAnalyzer.BandWidth);
            targetBands.z = Mathf.Clamp(targetBands.z, 0, audioAnalyzer.BandWidth);
        }
    }

    // put any audio reactive effects in this, no need to worry about execution order
    protected override void AudioReaction()
    {
        Vector3 rotation = Vector3.zero;
        rotation.x = Mathf.Clamp(audioData[targetBands.x] * rotationSpeed.x, minRotationSpeed.x, maxRotationSpeed.x);
        rotation.y = Mathf.Clamp(audioData[targetBands.y] * rotationSpeed.y, minRotationSpeed.y, maxRotationSpeed.y);
        rotation.z = Mathf.Clamp(audioData[targetBands.z] * rotationSpeed.z, minRotationSpeed.z, maxRotationSpeed.z);

        transform.Rotate(rotation);
    }
}
