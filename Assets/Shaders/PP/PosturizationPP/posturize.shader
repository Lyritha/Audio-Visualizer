Shader "Lyrith/Posterize"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        ZWrite Off Cull Off

        Pass
        {
            Name "PosturizeHSV"

            HLSLPROGRAM
            #include "Posturize.hlsl"

            #pragma vertex Vert
            #pragma fragment PosturizeHSV
            ENDHLSL
        }

        Pass
        {
            Name "PosturizeRGB"

            HLSLPROGRAM
            #include "Posturize.hlsl"

            #pragma vertex Vert
            #pragma fragment PosturizeRGB
            ENDHLSL
        }

        Pass
        {
            Name "PosturizeHDR"

            HLSLPROGRAM
            #include "Posturize.hlsl"

            #pragma vertex Vert
            #pragma fragment PosturizeHDR
            ENDHLSL
        }
    }

    Fallback "Diffuse"
}
