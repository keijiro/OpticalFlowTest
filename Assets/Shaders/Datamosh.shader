Shader "Hidden/OpticalFlowTest/Datamosh"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

sampler2D _MainTex;
float4 _MainTex_TexelSize;

sampler2D _FlowTex;
float _FlowAmp;

void Vertex(uint vertexID : VERTEXID_SEMANTIC,
            out float4 outPosition : SV_Position,
            out float2 outTexCoord : TEXCOORD0)
{
    outPosition = GetFullScreenTriangleVertexPosition(vertexID);
    outTexCoord = GetFullScreenTriangleTexCoord(vertexID);
}

float4 FragmentUpdate(float4 position : SV_Position,
                      float2 texCoord : TEXCOORD) : SV_Target
{
    float2 low_uv = round(texCoord * float2(40, 24)) / float2(40, 24);
    float2 vec = tex2D(_FlowTex, low_uv).xy;
    return tex2D(_MainTex, texCoord + _MainTex_TexelSize.xy * vec * -_FlowAmp);
}

ENDHLSL

    SubShader
    {
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
