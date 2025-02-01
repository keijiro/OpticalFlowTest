Shader "Hidden/OpticalFlowTest/Datamosh"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

Texture2D _MainTex;
Texture2D _FlowTex;
float _FlowAmp;

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
    float2 vec;

    // Flow map sampling
    {
        uint w, h;
        _FlowTex.GetDimensions(w, h);

        int2 tc = texCoord.xy * float2(w, h);
        vec = _FlowTex[tc].xy;
    }

    // Source color sampling
    {
        uint w, h;
        _MainTex.GetDimensions(w, h);

        int2 tc = texCoord * float2(w, h) - vec * _FlowAmp;
        tc = min(max(tc, 0), int2(w, h) - 1);
        return _MainTex[tc];
    }
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
