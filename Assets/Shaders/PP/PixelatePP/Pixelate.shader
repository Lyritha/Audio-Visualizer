Shader "Lyrith/Pixelate"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "Pixelate"

            HLSLPROGRAM
            #include "Pixelate.hlsl"  // External HLSL file with posterization logic

            #pragma vertex Vert
            #pragma fragment Pixelate
            ENDHLSL
        }
    }
    Fallback "Diffuse"
}