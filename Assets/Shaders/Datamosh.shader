Shader "Hidden/OpticalFlowTest/Datamosh"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

Texture2D _MainTex;
Texture2D _SourceTex;
Texture2D _FlowTex;
Texture2D _BlockTex;
float _FlowAmp;

float4 SamplePoint(Texture2D tex, float2 uv, int2 offset)
{
    uint w, h;
    tex.GetDimensions(w, h);
    int2 tc = (int2)(uv * float2(w, h)) + offset;
    tc = min(max(tc, 0), int2(w, h) - 1);
    return tex[tc];
}

float3 SamplePointSRGB(Texture2D tex, float2 uv, int2 offset)
{
    return LinearToSRGB(SamplePoint(tex, uv, offset).rgb);
}

void Vertex(uint vertexID : VERTEXID_SEMANTIC,
            out float4 outPosition : SV_Position,
            out float2 outTexCoord : TEXCOORD0)
{
    outPosition = GetFullScreenTriangleVertexPosition(vertexID);
    outTexCoord = GetFullScreenTriangleTexCoord(vertexID);
}

float4 Fragment(float4 position : SV_Position,
                float2 texCoord : TEXCOORD) : SV_Target
{
    // Optical flow vector
    float2 flow = SamplePoint(_FlowTex, texCoord, 0).xy * -_FlowAmp;

    // Color samples
    float3 source = SamplePointSRGB(_SourceTex, texCoord, 0);
    float3 feedback = SamplePointSRGB(_MainTex, texCoord, flow);

    // Block data
    uint block = SamplePoint(_BlockTex, texCoord, 0).x * 255;

    // Output composition
    float3 output = (block & 4u) ? source : feedback;
    float3 glitch1 = float3(output.zx, 1 - output.y);
    float3 glitch2 = float3(1 - output.y, 0.5, 0.5);
    output = (block & 128u) ? glitch2 : ((block & 64u) ? glitch1 : output);

    return float4(SRGBToLinear(output), 1);
}

ENDHLSL

    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off Blend Off
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
    }

    Fallback Off
}
