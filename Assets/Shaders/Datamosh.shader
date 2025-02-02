Shader "Hidden/OpticalFlowTest/Datamosh"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

Texture2D _MainTex;
Texture2D _SourceTex;
Texture2D _FlowTex;
Texture2D _NoiseTex;
float _FlowAmp;

float4 SamplePoint(Texture2D tex, float2 uv, int2 offset)
{
    uint w, h;
    tex.GetDimensions(w, h);
    int2 tc = (int2)(uv * float2(w, h)) + offset;
    tc = min(max(tc, 0), int2(w, h) - 1);
    return tex[tc];
}

void Vertex(uint vertexID : VERTEXID_SEMANTIC,
            out float4 outPosition : SV_Position,
            out float2 outTexCoord : TEXCOORD0)
{
    outPosition = GetFullScreenTriangleVertexPosition(vertexID);
    outTexCoord = GetFullScreenTriangleTexCoord(vertexID);
}

float4 FragmentCopy(float4 position : SV_Position,
                    float2 texCoord : TEXCOORD) : SV_Target
{
    return 0;
}

float4 FragmentUpdate(float4 position : SV_Position,
                      float2 texCoord : TEXCOORD) : SV_Target
{
    float2 vec = SamplePoint(_FlowTex, texCoord, 0).xy;
    float mask = SamplePoint(_NoiseTex, texCoord, 0).x;
    float3 src = SamplePoint(_SourceTex, texCoord, 0).rgb;
    float3 flow = SamplePoint(_MainTex, texCoord, vec * -_FlowAmp).rgb;
    return float4(mask > 0 ? src : flow, 1);
}

ENDHLSL

    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off Blend Off
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentCopy
            ENDHLSL
        }
        Pass
        {
            ZTest Always ZWrite Off Cull Off Blend Off
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentUpdate
            ENDHLSL
        }
    }

    Fallback Off
}
