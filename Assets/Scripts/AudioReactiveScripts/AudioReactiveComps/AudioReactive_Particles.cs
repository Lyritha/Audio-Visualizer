using System;
using UnityEngine;
using UnityEngine.VFX;

public class AudioReactive_Particles : AudioReactiveComponent
{
    [Serializable]
    private struct EffectData
    {
        public string name;
        public int targetBand;
        public float strength;

        public EffectData(string name, int targetBand, float strength)
        {
            this.name = name;
            this.targetBand = targetBand;
            this.strength = strength;
        }
    }

    [SerializeField] VisualEffect effect;
    [SerializeField] private EffectData[] parameterDrivers;


    protected override void OnValidate()
    {
        if (audioAnalyzer == null) audioAnalyzer = FindFirstObjectByType<AudioAnalyser>();

        for (int i = 0; i < parameterDrivers.Length; i++) 
        {
            parameterDrivers[i].targetBand = Math.Clamp(parameterDrivers[i].targetBand, 0, audioAnalyzer.BandWidth);
        }
    }

    protected override void AudioReaction()
    {
        foreach (EffectData data in parameterDrivers)
        {
            effect.SetFloat(data.name, audioData[data.targetBand] * data.strength);
        }
    }
}
