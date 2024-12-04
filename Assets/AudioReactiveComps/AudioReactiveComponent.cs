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
    [SerializeField, HideInInspector] protected DataTarget targetData;
    [SerializeField, HideInInspector, Range(0,100)] protected int dataResolution = 5;


    // holds the audio data for further processing
    [SerializeField] protected AudioAnalyzer audioAnalyzer;
    protected float[] audioData = new float[0];



    // Update is called once per frame
    protected void Update()
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
        float[] samples = audioAnalyzer.GetSample(AudioAnalyzer.Channel.left);

        int scaledLength = (int)(samples.Length / 100 * dataResolution);

        // re-scale the array if it doesn't match with the incoming data
        if (audioData.Length != scaledLength)
            audioData = new float[scaledLength];

        // Calculate the step size for sampling
        int stepSize = samples.Length / scaledLength;

        for (int i = 0; i < scaledLength; i++)
        {
            // Grab every nth sample (without averaging)
            int sampleIndex = i * stepSize;
            audioData[i] = sampleIndex < samples.Length ? samples[sampleIndex] : samples[^1];
        }
    }

    protected void ProcessRawFrequencyBands()
    {
        float[] bands = audioAnalyzer.GetBands(AudioAnalyzer.Channel.left);

        // re-scale the array if it doesn't match with the incoming data
        if (audioData.Length != bands.Length)
            audioData = new float[bands.Length];

        // assign the data
        audioData = bands;
    }
}
