#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

float _PixelSize;

float4 Pixelate(Varyings input) : SV_Target
{
    float2 uv = input.texcoord;
    
    // Access screen resolution via _ScreenParams.xy
    float2 resolution = _ScreenParams.xy;

    // Calculate pixelation based on resolution and pixel size
    float2 pixelatedUV = floor(uv * resolution / _PixelSize) * _PixelSize / resolution;

    // Sample the texture using pixelated UVs
    float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, pixelatedUV);
    return float4(col.rgb, col.a);
}
