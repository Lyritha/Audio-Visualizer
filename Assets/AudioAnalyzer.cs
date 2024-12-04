using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class AudioAnalyzer : MonoBehaviour
{
    public enum Channel
    {
        left,
        right,
        stereo
    }

    // audio source to target for sampling
    [SerializeField] private AudioSource source;


    [SerializeField, Tooltip("Tells the script how many samples a band needs at minimum, falls back on sample count / band count."), Range(0, 500)]
    private int minimumSampleCount = 1;

    [SerializeField, Range(64, 8192)] private int sampleCount = 2048;
    [SerializeField, Range(2, 16)] private int bandWidth = 8;



    // "output" variables, where the data is stored
    private float[] stereoBands;
    private float[] leftBands;
    private float[] rightBands;

    private float[] stereoSamples;
    private float[] leftSamples;
    private float[] rightSamples;

    //enforce values in the inspector
    private void OnValidate()
    {
        // clamp sample count to power of 2
        sampleCount = Mathf.Clamp(Mathf.ClosestPowerOfTwo(sampleCount), 64, 8192);

        // clamp frequency band to even numbers, to make sample assignment easier
        bandWidth += bandWidth % 2 != 0 ? 1 : 0;
        bandWidth = Mathf.Clamp(bandWidth, 2, 16);
    }



    // setup the sampler
    private void Awake()
    {
        stereoBands = new float[bandWidth];
        leftBands = new float[bandWidth];
        rightBands = new float[bandWidth];

        stereoSamples = new float[sampleCount];
        leftSamples = new float[sampleCount];
        rightSamples = new float[sampleCount];
    }
    private void Update()
    {
        GetSpectrumData();
        stereoBands = GenerateFrequencyBands(stereoBands, stereoSamples);
        leftBands = GenerateFrequencyBands(leftBands, leftSamples);
        rightBands = GenerateFrequencyBands(rightBands, rightBands);
    }



    // handles getting the samples

    private void GetSpectrumData()
    {
        // re-create the arrays if it no longer matches sample count
        if (stereoSamples.Length != sampleCount)
        {
            leftSamples = new float[sampleCount];
            rightSamples = new float[sampleCount];
            stereoSamples = new float[sampleCount];
        }

        // get spectrum data for both channels
        source.GetSpectrumData(leftSamples, 0, FFTWindow.Blackman);
        source.GetSpectrumData(rightSamples, 1, FFTWindow.Blackman);

        for (int i = 0; i < sampleCount; i++)
        {
            stereoSamples[i] = (leftSamples[i] + rightSamples[i]) * 0.5f;
        }
    }


    // assigns the samples to the different bands

    private float[] GenerateFrequencyBands(float[] band, float[] samples)
    {
        // re-scale the frequency bands if it no longer matches up
        if (band.Length != bandWidth) band = new float[bandWidth];

        // prepare the array of sample counts with a default value each.
        (long[] bandSampleCount, long availableSamples) = CreateInitialBands(sampleCount);
        if (availableSamples > 0) bandSampleCount = DistributeSamples(bandSampleCount, availableSamples);

        // Adjust the last band if there's a mismatch
        long adjustedTotal = bandSampleCount.Sum();
        if (adjustedTotal != sampleCount)
            bandSampleCount[^1] += sampleCount - adjustedTotal;

        band = SetAverageForBands(band, samples, bandSampleCount);

        return band;
    }
    private (long[], long) CreateInitialBands(long availableSamples)
    {
        // Calculate the base amount to distribute
        bool canDistributeMinimum = minimumSampleCount * bandWidth < availableSamples;
        long amount = canDistributeMinimum ? minimumSampleCount : availableSamples / bandWidth;

        // Create the output array and distribute the base amount and adjust available samples
        long[] output = Enumerable.Repeat(amount, bandWidth).ToArray();
        availableSamples -= amount * bandWidth;

        return (output, availableSamples);
    }
    private long[] DistributeSamples(long[] input, long availableSamples)
    {
        long totalSamples = 0;
        long[] BandSamples = new long[input.Length];

        // Calculate the initial sample counts per band (^2)
        for (int i = 0; i < bandWidth; i++)
        {
            BandSamples[i] = (1L << i) * 2;
            totalSamples += BandSamples[i];
        }

        // remap all the values
        for (int i = 0; i < bandWidth; i++)
            input[i] += Remap(BandSamples[i], 0, totalSamples, 0, availableSamples);

        return input;
    }
    private float[] SetAverageForBands(float[] band, float[] samples, long[] bandSampleCount)
    {
        // empty the array for the new data
        Array.Clear(band, 0, band.Length);

        // Calculate normalized values for each band
        int sampleIndex = 0;

        for (int i = 0; i < bandWidth; i++)
        {
            long samplesInBand = bandSampleCount[i];

            // Skip bands with no samples
            if (samplesInBand <= 0) continue;

            // Accumulate samples for the current band
            float bandTotal = 0;
            for (int j = 0; j < samplesInBand; j++)
                bandTotal += samples[sampleIndex++];

            // Calculate and store the average
            band[i] = bandTotal / samplesInBand;
        }

        return band;
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



    public struct AverageBandsJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> inputSamples;
        [ReadOnly] public NativeArray<long> bandSampleCount;
        public NativeArray<float> result;

        public void Execute(int index)
        {
            long samplesInBand = bandSampleCount[index];
            if (samplesInBand <= 0) return;

            float bandTotal = 0;
            for (int j = 0; j < samplesInBand; j++)
            {
                bandTotal += inputSamples[(int)(index * samplesInBand + j)];
            }

            result[index] = bandTotal / samplesInBand;
        }
    }
}
