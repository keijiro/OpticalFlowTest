Shader "Hidden/OpticalFlowTest/Estimator"
{
    HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
#include "OpticalFlow.hlsl"

TEXTURE2D(_MainTex);
TEXTURE2D(_PrevTex);
TEXTURE2D(_DiffMask);
SAMPLER(sampler_MainTex);
SAMPLER(sampler_PrevTex);
float4 _MainTex_TexelSize;
float4 _PrevTex_TexelSize;

void Vertex(uint vertexID : VERTEXID_SEMANTIC,
            out float4 outPosition : SV_Position,
            out float2 outTexCoord : TEXCOORD0)
{
    outPosition = GetFullScreenTriangleVertexPosition(vertexID);
    outTexCoord = GetFullScreenTriangleTexCoord(vertexID);
}

float4 FragmentGrad(float4 position : SV_Position,
                    float2 texCoord : TEXCOORD) : SV_Target
{
    UnityTexture2D mainTex = UnityBuildTexture2DStructNoScale(_MainTex);
    UnityTexture2D prevTex = UnityBuildTexture2DStructNoScale(_PrevTex);
    float3 output;
    OF_Gradients_float(mainTex, prevTex, texCoord, output);
    return float4(output, 1);
}

float4 FragmentFlow(float4 position : SV_Position,
                    float2 texCoord : TEXCOORD) : SV_Target
{
    UnityTexture2D mainTex = UnityBuildTexture2DStructNoScale(_MainTex);
    float3 output;
    OF_LucasKanade_float(mainTex, texCoord, output);
    float mask = _DiffMask[uint2(0, 0)].r;
    return float4(output, mask);
}

    ENDHLSL

    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentGrad
            ENDHLSL
        }
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment FragmentFlow
            ENDHLSL
        }
    }

    Fallback Off
}
