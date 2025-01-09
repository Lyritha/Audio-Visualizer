Shader "Custom/Posterize"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off
        Pass
        {
            Name "Posturize"

            HLSLPROGRAM
            #include "Posturization.hlsl"  // External HLSL file with posterization logic

            #pragma vertex Vert
            #pragma fragment Posturize
            ENDHLSL
        }
    }
    Fallback "Diffuse"
}