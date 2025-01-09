using System;
using UnityEngine.Rendering;

[Serializable, VolumeComponentMenu("Custom/Posturize")]
public class PosturizeVolumeComponent : VolumeComponent
{
    public ClampedIntParameter steps = new(64, 2, 128);
}