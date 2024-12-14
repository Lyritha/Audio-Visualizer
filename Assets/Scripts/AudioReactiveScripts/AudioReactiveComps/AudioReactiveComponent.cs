using System;
using UnityEngine;

public abstract class AudioReactiveComponent : MonoBehaviour
{
    [Serializable]
    public enum DataTarget
    {
        rawSamples,
        rawFrequencyBands
    }

    // tells the script what part of the audio data to target
    [Header("Audio analyser info")]
    [SerializeField, HideInInspector] protected DataTarget targetData;
    [SerializeField, HideInInspector, Range(1,100)] protected int sampleResolution = 5;
    [SerializeField, HideInInspector] protected Vector2Int sampleRange;


    // holds the audio data for further processing
    [SerializeField] protected AudioAnalyser audioAnalyzer;
    [SerializeField] protected AudioAnalyser.Channel audioChannel;
    protected float[] audioData = new float[0];

    // for custom inspector
    public AudioAnalyser AudioAnalyzer => audioAnalyzer;

    protected virtual void OnValidate()
    {
    }

    // Update is called once per frame
    protected void FixedUpdate()
    {
        ProcessAudioData();
        AudioReaction();
    }

    protected abstract void AudioReaction();

    protected void ProcessAudioData()
    {
        switch (targetData)
        {
            case DataTarget.rawSamples:
                ProcessRawSamples(); 
                break;

            case DataTarget.rawFrequencyBands:
                ProcessRawFrequencyBands();
                break;
        }
    }

    protected void ProcessRawSamples()
    {
        // get the samples
        float[] samples = audioAnalyzer.GetSample(audioChannel);

        // get how many samples the new array will be
        int sampleCount = (sampleRange.y - sampleRange.x) / 100 * sampleResolution;

        // re-scale the array if it doesn't match with the incoming data
        if (audioData.Length != sampleCount)
            audioData = new float[sampleCount];

        // fill the data array with the correct sample
        for (int i = 0; i < sampleCount; i++)
            audioData[i] = samples[sampleRange.x + (i * (100 / sampleResolution))];
    }

    protected void ProcessRawFrequencyBands()
    {
        float[] bands = audioAnalyzer.GetBands(audioChannel);

        // re-scale the array if it doesn't match with the incoming data
        if (audioData.Length != bands.Length)
            audioData = new float[bands.Length];

        // assign the data
        audioData = bands;
    }
}
