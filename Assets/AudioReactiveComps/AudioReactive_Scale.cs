using UnityEngine;

public class AudioReactive_Scale : AudioReactiveComponent
{
    [SerializeField] private Vector3Int targetBands = Vector3Int.zero;
    [SerializeField] private Vector3 reactionStrength = Vector3.one;
    [SerializeField] private Vector2 minMaxScaling = new(1,5);

    //store scale
    private Vector3 originalScale = Vector3.one;

    private void Awake()
    {
        originalScale = transform.localScale;
        float medianScale = (originalScale.x + originalScale.y + originalScale.z) / 3f;
        minMaxScaling.x += medianScale;
        minMaxScaling.y += medianScale;
    }

    private void OnValidate()
    {
        if (audioAnalyzer != null)
        {
            targetBands.x = Mathf.Clamp(targetBands.x, 0, audioAnalyzer.BandWidth);
            targetBands.y = Mathf.Clamp(targetBands.y, 0, audioAnalyzer.BandWidth);
            targetBands.z = Mathf.Clamp(targetBands.z, 0, audioAnalyzer.BandWidth);
        }
    }

    protected override void AudioReaction()
    {
        Vector3 transformedScale = Vector3.zero;

        transformedScale.x = Mathf.Clamp(audioData[targetBands.x] * reactionStrength.x + originalScale.x, minMaxScaling.x, minMaxScaling.y);
        transformedScale.y = Mathf.Clamp(audioData[targetBands.y] * reactionStrength.y + originalScale.y, minMaxScaling.x, minMaxScaling.y);
        transformedScale.z = Mathf.Clamp(audioData[targetBands.z] * reactionStrength.z + originalScale.z, minMaxScaling.x, minMaxScaling.y);


        transform.localScale = transformedScale;
    }
}
