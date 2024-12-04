public class AudioReactive_Rotate : AudioReactiveComponent
{
    // put any audio reactive effects in this, no need to worry about execution order
    protected override void AudioReaction()
    {
        transform.Rotate(0, audioData[0] * 90, 0);
    }
}
