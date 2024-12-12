using UnityEngine;
using UnityEngine.VFX;

public class AudioReactive_Particles : AudioReactiveComponent
{
    [SerializeField] VisualEffect effect;

    protected override void AudioReaction()
    {
        effect.SetFloat("ParticleSpeed", audioData[0] * 100);
    }
}
