using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioAnalyzer : MonoBehaviour
{
    private enum AudioChannel
    {
        left, 
        right
    }

    // audio source to target for sampling
    private AudioSource source;

    [SerializeField] private float delay = 5f;
    [SerializeField] private AudioChannel audioChannel;

    [SerializeField, Tooltip("Tells the script how many frames it should use to average the samples at.")]
    private int sampleSmoothingCount = 1;

    [SerializeField, Tooltip("Tells the script how many samples a band needs at minimum, falls back on sample count / band count."), Range(0, 500)]
    private int minimumSampleCount = 1;

    [SerializeField, Range(64, 8192)] private int sampleCount = 2048;
    [SerializeField, Range(2, 16)] private int frequencyBandCount = 8;

    public float[] Samples { get; private set; }
    public float[] FrequencyBands { get; private set; }

    //enforce values in the inspector
    private void OnValidate()
    {
        // clamp sample count to power of 2
        sampleCount = Mathf.Clamp(Mathf.ClosestPowerOfTwo(sampleCount), 64, 8192);

        // clamp frequency band to even numbers, to make sample assignment easier
        frequencyBandCount += frequencyBandCount % 2 != 0 ? 1 : 0;
        frequencyBandCount = Mathf.Clamp(frequencyBandCount, 2, 16);

        // re-create the arrays
        SetValues();
    }

    // setup the sampler
    private void Awake()
    {
        source = GetComponent<AudioSource>();
        SetValues();
        StartCoroutine(Wait());
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(delay);
        source.Play();
    }

    private void SetValues()
    {
        Samples = new float[sampleCount];
        FrequencyBands = new float[frequencyBandCount];
    }

    // Update is called once per frame
    private void Update()
    {
        source.GetSpectrumData(Samples, (int)audioChannel, FFTWindow.Blackman);
        MakeFrequencyBands();
    }

    private void MakeFrequencyBands()
    {
        // Avoid null reference errors
        if (FrequencyBands == null || Samples == null) return;

        (long[] bandSampleCount, long availableSamples) = CreateInitialValues(sampleCount);
        bandSampleCount = DistributeSamples(bandSampleCount, availableSamples);

        // Adjust the last band if there's a mismatch
        long adjustedTotal = bandSampleCount.Sum();
        if (adjustedTotal != sampleCount)
            bandSampleCount[^1] += sampleCount - adjustedTotal;

        // Calculate normalized values for each band
        int sampleIndex = 0;
        for (int i = 0; i < frequencyBandCount; i++)
        {
            float bandTotal = 0;

            for (int j = 0; j < bandSampleCount[i]; j++)
                bandTotal += Samples[sampleIndex++];

            FrequencyBands[i] = bandTotal / bandSampleCount[i];
            if (float.IsNaN(FrequencyBands[i])) FrequencyBands[i] = 0;
        }
    }

    private (long[], long) CreateInitialValues(long availableSamples)
    {
        // create the new array
        long[] output = new long[frequencyBandCount];

        // add base samples, if minimum samples is heigher then available samples, give them all equally
        for (int i = 0; i < frequencyBandCount; i++)
        {
            long amount = (availableSamples < minimumSampleCount * frequencyBandCount) ? (availableSamples / frequencyBandCount) : minimumSampleCount;

            output[i] = amount;
            availableSamples -= amount;
        }

        return (output, availableSamples);
    }

    private long[] DistributeSamples(long[] input, long availableSamples)
    {
        // don't do any of the work if there are no available samples
        if (availableSamples <= 0) return input;

        long totalSamples = 0;
        long[] BandSamples = new long[input.Length];

        // Calculate the initial sample counts per band (^2)
        for (int i = 0; i < frequencyBandCount; i++)
        {
            BandSamples[i] = (1L << i) * 2;
            totalSamples += BandSamples[i];
        }

        // remap all the values
        for (int i = 0; i < frequencyBandCount; i++)
            input[i] += Remap(BandSamples[i], 0, totalSamples, 0, availableSamples);

        return input;
    }

    private long Remap(long value, long fromMin, long fromMax, long toMin, long toMax)
    {
        if (fromMax == fromMin) return toMin; // Avoid division by zero
        return toMin + (value - fromMin) * (toMax - toMin) / (fromMax - fromMin);
    }
}
