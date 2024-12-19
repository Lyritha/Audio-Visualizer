using System;
using UnityEngine;

public class AudioReactive_Transform : AudioReactiveComponent
{
    [Serializable]
    public enum TargetTransform
    {
        location,
        rotation,
        scale
    }

    // all layers
    [SerializeField] protected Vector3Int targets = Vector3Int.zero;
    [SerializeField] protected Vector3 strength = Vector3.zero;

    // global
    [Header("Transform values")]
    [SerializeField, HideInInspector] protected TargetTransform targetTransform;

    // scale only
    [SerializeField, HideInInspector] protected Vector3 defaultScale = Vector3.one;
    [SerializeField, HideInInspector] protected Vector2 rangeX = Vector2.zero;
    [SerializeField, HideInInspector] protected Vector2 rangeY = Vector2.zero;
    [SerializeField, HideInInspector] protected Vector2 rangeZ = Vector2.zero;

    protected override void OnValidate()
    {
        if (audioAnalyzer == null) audioAnalyzer = FindFirstObjectByType<AudioAnalyser>();

        if (audioAnalyzer != null)
        {
            switch (targetData)
            {
                case DataTarget.rawSamples:
                    targets.x = Mathf.Clamp(targets.x, sampleRange.x, sampleRange.y);
                    targets.y = Mathf.Clamp(targets.y, sampleRange.x, sampleRange.y);
                    targets.z = Mathf.Clamp(targets.z, sampleRange.x, sampleRange.y);
                    break;

                case DataTarget.rawFrequencyBands:
                    targets.x = Mathf.Clamp(targets.x, 0, audioAnalyzer.BandWidth);
                    targets.y = Mathf.Clamp(targets.y, 0, audioAnalyzer.BandWidth);
                    targets.z = Mathf.Clamp(targets.z, 0, audioAnalyzer.BandWidth);
                    break;
            }
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
        rotation.x = audioData[targets.x] * strength.x;
        rotation.y = audioData[targets.y] * strength.y;
        rotation.z = audioData[targets.z] * strength.z;

        transform.Rotate(rotation);
    }

    protected void Scale()
    {
        Vector3 transformedScale = Vector3.zero;

        transformedScale.x = Mathf.Clamp(audioData[targets.x] * strength.x + defaultScale.x, rangeX.x + defaultScale.x, rangeX.y + defaultScale.x);
        transformedScale.y = Mathf.Clamp(audioData[targets.y] * strength.y + defaultScale.y, rangeY.x + defaultScale.y, rangeY.y + defaultScale.y);
        transformedScale.z = Mathf.Clamp(audioData[targets.z] * strength.z + defaultScale.z, rangeZ.x + defaultScale.z, rangeZ.y + defaultScale.z);


        transform.localScale = transformedScale;
    }
}
