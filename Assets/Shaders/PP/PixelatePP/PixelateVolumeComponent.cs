using System;
using UnityEngine.Rendering;

namespace Lyrith
{
    [Serializable, VolumeComponentMenu("Lyrith/Pixelate")]
    public class PixelateVolumeComponent : VolumeComponent
    {
        public ClampedFloatParameter pixelSize = new(5, 1, 100);
    }
}