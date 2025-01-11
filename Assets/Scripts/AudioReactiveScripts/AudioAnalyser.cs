using Assets.Scripts.Audio;
using System;
using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioAnalyser : MonoBehaviour
{
    public enum Channel
    {
        left,
        right,
        stereo
    }
    public enum AudioOrigin
    {
        audioSource,
        desktop
    }



    // audio to target for sampling
    [SerializeField] AudioOrigin audioOrigin;

    private RealtimeAudio realtimeAudio;
    private AudioSource sourceAudio;

    [SerializeField, Tooltip("Tells the script how many samples a band needs at minimum, falls back on sample count / band count."), Range(0, 500)]
    private int minimumSampleCount = 1;

    [SerializeField, Range(64, 8192)] private int sampleCount = 2048;
    [SerializeField, Range(2, 16)] private int bandWidth = 8;

    [SerializeField] private float delayTimer = 0;

    public int SampleCount => sampleCount; // Public getter
    public int BandWidth => bandWidth;     // Public getter


    // "output" variables, where the data is stored
    private float[] stereoBands;
    private float[] leftBands;
    private float[] rightBands;

    private float[] stereoSamples;
    private float[] leftSamples;
    private float[] rightSamples;

    // desktop samples, realtime audio outputs into this
    private float[] desktopSamples;

    //enforce values in the inspector
    private void OnValidate()
    {
        // clamp sample count to power of 2
        sampleCount = Mathf.Clamp(Mathf.ClosestPowerOfTwo(sampleCount), 64, 8192);

        // clamp frequency band to even numbers, to make sample assignment easier
        bandWidth += bandWidth % 2 != 0 ? 1 : 0;
        bandWidth = Mathf.Clamp(bandWidth, 2, 16);
    }

    private IEnumerator AudioSourceTimer()
    {
        yield return new WaitForSeconds(delayTimer);

        sourceAudio.Play();

        yield return null;
    }

    // setup the sampler
    private void Awake()
    {
        sourceAudio = GetComponent<AudioSource>();

        // start audio source with a delay only if audio source is the target
        if (audioOrigin == AudioOrigin.audioSource) StartCoroutine(AudioSourceTimer());

        stereoBands = new float[BandWidth];
        leftBands = new float[BandWidth];
        rightBands = new float[BandWidth];

        stereoSamples = new float[SampleCount];
        leftSamples = new float[SampleCount];
        rightSamples = new float[SampleCount];

        desktopSamples = new float[sampleCount];

        // Setup loopback audio and start listening
        realtimeAudio = new RealtimeAudio(sampleCount, ScalingStrategy.Sqrt, (spectrumData) =>
        { desktopSamples = spectrumData; });

        realtimeAudio.StartListen();
    }

    private void Update()
    {
        // get the data
        GetSpectrumData();

        // generate the bands
        leftBands = GenerateFrequencyBands(leftBands, leftSamples);
        rightBands = GenerateFrequencyBands(rightBands, rightSamples);

        // re-scale the frequency bands if it no longer matches up
        if (stereoBands.Length != BandWidth) stereoBands = new float[BandWidth];

        // generate stereo bands from other bands, way cheaper
        for (int i = 0; i < BandWidth; i++)
            stereoBands[i] = (leftBands[i] + rightBands[i]) * 0.5f;
    }



    // handles getting the samples

    private void GetSpectrumData()
    {
        // re-create the arrays if it no longer matches sample count
        if (stereoSamples.Length != SampleCount)
        {
            leftSamples = new float[SampleCount];
            rightSamples = new float[SampleCount];
            stereoSamples = new float[SampleCount];
        }

        if (audioOrigin == AudioOrigin.desktop)
        {
            leftSamples = desktopSamples;
            rightSamples = desktopSamples;
            stereoSamples = desktopSamples;
        }
        else
        {
            sourceAudio.GetSpectrumData(leftSamples, 0, FFTWindow.Blackman);
            sourceAudio.GetSpectrumData(rightSamples, 1, FFTWindow.Blackman);

            // generate stereo samples from other bands, way cheaper
            for (int i = 0; i < SampleCount; i++)
                stereoSamples[i] = (leftSamples[i] + rightSamples[i]) * 0.5f;
        }
    }


    // assigns the samples to the different bands

    private float[] GenerateFrequencyBands(float[] band, float[] samples)
    {
        // re-scale the frequency bands if it no longer matches up
        if (band.Length != BandWidth) band = new float[BandWidth];

        // prepare the array of sample counts with a default value each.
        (long[] bandSampleCount, long availableSamples) = CreateInitialBands(SampleCount);
        if (availableSamples > 0) bandSampleCount = DistributeSamples(bandSampleCount, availableSamples);

        // Adjust the last band if there's a mismatch
        long adjustedTotal = bandSampleCount.Sum();
        if (adjustedTotal != SampleCount)
            bandSampleCount[^1] += SampleCount - adjustedTotal;

        return SetAverageForBands(band, samples, bandSampleCount);
    }
    private (long[], long) CreateInitialBands(long availableSamples)
    {
        // Calculate the base amount to distribute
        bool canDistributeMinimum = minimumSampleCount * BandWidth < availableSamples;
        long amount = canDistributeMinimum ? minimumSampleCount : availableSamples / BandWidth;

        // Create the output array and distribute the base amount and adjust available samples
        long[] output = Enumerable.Repeat(amount, BandWidth).ToArray();
        availableSamples -= amount * BandWidth;

        return (output, availableSamples);
    }
    private long[] DistributeSamples(long[] bandSampleCount, long availableSamples)
    {
        long totalSamples = 0;
        long[] BandSamples = new long[bandSampleCount.Length];

        // Calculate the initial sample counts per band (^2)
        for (int i = 0; i < BandWidth; i++)
        {
            BandSamples[i] = (1L << i) * 2;
            totalSamples += BandSamples[i];
        }

        // remap all the values
        for (int i = 0; i < BandWidth; i++)
            bandSampleCount[i] += Remap(BandSamples[i], 0, totalSamples, 0, availableSamples);

        return bandSampleCount;
    }
    private float[] SetAverageForBands(float[] bands, float[] samples, long[] bandSampleCount)
    {
        // empty the array for the new data
        Array.Clear(bands, 0, bands.Length);

        // Calculate normalized values for each band
        int sampleIndex = 0;

        for (int i = 0; i < bands.Length; i++)
        {
            long samplesInBand = bandSampleCount[i];

            // Skip bands with no samples
            if (samplesInBand <= 0) continue;

            // Accumulate samples for the current band
            float bandTotal = 0;
            for (int j = 0; j < samplesInBand; j++)
                bandTotal += samples[sampleIndex++];

            // Calculate and store the average
            bands[i] = bandTotal / samplesInBand;
        }

        return bands;
    }

    public void OnApplicationQuit()
    {
        realtimeAudio.StopListen();
    }

    // helper functions

    public float[] GetSample(Channel channel = Channel.stereo)
    {
        return channel switch
        {
            Channel.left => leftSamples,
            Channel.right => rightSamples,
            Channel.stereo => stereoSamples,
            _ => stereoSamples,
        };
    }

    public float[] GetBands(Channel channel = Channel.stereo)
    {
        return channel switch
        {
            Channel.left => leftBands,
            Channel.right => rightBands,
            Channel.stereo => stereoBands,
            _ => stereoBands,
        };
    }

    // utility functions

    private long Remap(long value, long fromMin, long fromMax, long toMin, long toMax)
    {
        if (fromMax == fromMin) return toMin; // Avoid division by zero
        return toMin + (long)((value - fromMin) * (double)(toMax - toMin) / (fromMax - fromMin));
    }
}
