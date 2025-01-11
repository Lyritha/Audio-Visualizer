#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
    
int _Steps;
const float Epsilon = 1e-10;

// converts rgb colors to hcv to hsv, pure magic, but why code your own when there is a good solution already online
float3 RGBtoHCV(float3 RGB)
{
    // Based on work by Sam Hocevar and Emil Persson
    float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0 / 3.0) : float4(RGB.gb, 0.0, -1.0 / 3.0);
    float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
    float C = Q.x - min(Q.w, Q.y);
    float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
    return float3(H, C, Q.x);
}
float3 RGBtoHSV(float3 RGB)
{
    float3 HCV = RGBtoHCV(RGB);
    float S = HCV.y / (HCV.z + Epsilon);
    return float3(HCV.x, S, HCV.z);
}


// convert hsv back to rgb
float3 HUEtoRGB(float H)
{
    float R = abs(H * 6 - 3) - 1;
    float G = 2 - abs(H * 6 - 2);
    float B = 2 - abs(H * 6 - 4);
    return saturate(float3(R, G, B));
}
float3 HSVtoRGB(float3 HSV)
{
    float3 RGB = HUEtoRGB(HSV.x);
    return ((RGB - 1) * HSV.y + 1) * HSV.z;
}


// Convert RGB to HSV while preserving HDR values and vice versa
float3 RGBtoHSV_HDR(float3 RGB)
{
    float3 HCV = RGBtoHCV(RGB);
    float S = HCV.y / (HCV.z + Epsilon);
    return float3(HCV.x, S, HCV.z);
}
float3 HSVtoRGB_HDR(float3 HSV)
{
    float3 RGB = HUEtoRGB(HSV.x);
    return ((RGB - 1) * HSV.y + 1) * HSV.z;
}



// Posterize the colors while preserving HDR
float3 posterizeHSV_HDR(float3 color, int steps)
{
    float3 hsv = RGBtoHSV_HDR(color);

    // Adjust only the hue and saturation, leaving value (brightness) to represent HDR range
    float stepSizeHue = 1.0 / float(steps); // Hue and saturation are normalized between 0 and 1
    float stepSizeSat = 1.0 / float(steps);
    hsv.x = floor(hsv.x / stepSizeHue) * stepSizeHue; // Quantize hue
    hsv.y = floor(hsv.y / stepSizeSat) * stepSizeSat; // Quantize saturation

    // Convert back to RGB
    return HSVtoRGB_HDR(hsv);
}



float4 PosturizeRGB(Varyings input) : SV_Target
{
    float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
    
    float stepSize = 1.0 / float(_Steps);
    col = floor(col / stepSize) * stepSize;
    
    return float4(col.rgb, col.a);  
}

float4 PosturizeHSV(Varyings input) : SV_Target
{
    float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
    
    float stepSize = 1.0 / float(_Steps);
    float3 col2 = floor(RGBtoHSV(col.rgb) / stepSize) * stepSize;
    col2 = HSVtoRGB(col2);
    
    return float4(col2.rgb, col.a);
}   

float4 PosturizeHDR(Varyings input) : SV_Target
{
    // Sample the HDR texture
    float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);

    // Posterize while preserving HDR
    float3 posterizedColor = posterizeHSV_HDR(col.rgb, _Steps);

    return float4(posterizedColor, col.a); // Preserve alpha if applicable
}