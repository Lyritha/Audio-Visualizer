using UnityEngine;

public class AudioReactive_Transform : AudioReactiveComponent
{
    private enum TargetTransform
    {
        location,
        rotation,
        scale
    }

    [SerializeField] private TargetTransform targetTransform;
    [SerializeField] private Vector3Int targetBands = Vector3Int.zero;
    [SerializeField] private Vector3 strength = Vector3.zero;
    [SerializeField] private Vector2 rangeX = Vector2.zero;
    [SerializeField] private Vector2 rangeY = Vector2.zero;
    [SerializeField] private Vector2 rangeZ = Vector2.zero;

    //scale specific
    [SerializeField] private Vector3 defaultScale = Vector3.one;

    protected override void OnValidate()
    {
        if (audioAnalyzer == null) audioAnalyzer = FindFirstObjectByType<AudioAnalyser>();

        if (audioAnalyzer != null)
        {
            targetBands.x = Mathf.Clamp(targetBands.x, 0, audioAnalyzer.BandWidth);
            targetBands.y = Mathf.Clamp(targetBands.y, 0, audioAnalyzer.BandWidth);
            targetBands.z = Mathf.Clamp(targetBands.z, 0, audioAnalyzer.BandWidth);
        }

        base.OnValidate();
    }

    protected override void AudioReaction()
    {
        switch (targetTransform)
        {
            case TargetTransform.location:
                Location();
                break;
            case TargetTransform.rotation:
                Rotation();
                break;
            case TargetTransform.scale:
                Scale();
                break;
            default:
                break;
        }
    }

    protected void Location()
    {
        // not implemented yet
        print("location not yet implemented");
    }

    protected void Rotation()
    {
        Vector3 rotation = Vector3.zero;
        rotation.x = audioData[targetBands.x] * strength.x;
        rotation.y = audioData[targetBands.y] * strength.y;
        rotation.z = audioData[targetBands.z] * strength.z;

        transform.Rotate(rotation);
    }

    protected void Scale()
    {
        Vector3 transformedScale = Vector3.zero;

        transformedScale.x = Mathf.Clamp(audioData[targetBands.x] * strength.x + defaultScale.x, rangeX.x + defaultScale.x, rangeX.y + defaultScale.x);
        transformedScale.y = Mathf.Clamp(audioData[targetBands.y] * strength.y + defaultScale.y, rangeY.x + defaultScale.y, rangeY.y + defaultScale.y);
        transformedScale.z = Mathf.Clamp(audioData[targetBands.z] * strength.z + defaultScale.z, rangeZ.x + defaultScale.z, rangeZ.y + defaultScale.z);


        transform.localScale = transformedScale;
    }
}
