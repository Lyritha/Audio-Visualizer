using System;
using UnityEngine.Rendering;

namespace Lyrith
{
    [Serializable, VolumeComponentMenu("Lyrith/Posturize")]
    public class PosturizeVolumeComponent : VolumeComponent
    {
        public ClampedIntParameter steps = new(128, 8, 512);
        public PosturizeTargetParameter colorSpace = new(PosturizeTarget.HSV);
    }

    [Serializable]
    public enum PosturizeTarget
    {
        HSV,
        RGB,
        HDR
    }

    [Serializable]
    public class PosturizeTargetParameter : VolumeParameter<PosturizeTarget>
    {
        public PosturizeTargetParameter(PosturizeTarget value, bool overrideState = false)
            : base(value, overrideState) { }
    }
}