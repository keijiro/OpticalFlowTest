Shader "Hidden/OpticalFlowTest/Estimator"
{
    HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"

TEXTURE2D(_MainTex);
TEXTURE2D(_PrevTex);
SAMPLER(sampler_MainTex);
SAMPLER(sampler_PrevTex);
float4 _MainTex_TexelSize;
float4 _PrevTex_TexelSize;

StructuredBuffer<float4> _DiffMask;

static const int kWindowWidth = 5;

// Gaussian weight
float GaussWeight(float x, float y)
{
    float sigma = (kWindowWidth * 2 + 1) / 3.0;
    float2 v = float2(x, y);
    return exp(-dot(v, v) / (2 * sigma * sigma));
}

// Inverse matrix calculation
float2x2 Inverse(float2x2 m)
{
    float a = m._m00, b = m._m01, c = m._m10, d = m._m11;
    float det = a * d - b * c;
    return abs(det) <= 0.000001 ? 0 : float2x2(d, -b, -c, a) / det;
}

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
    float3 duv = float3(_MainTex_TexelSize.xy, 0);
    float cur = Luminance(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texCoord).rgb);
    float pre = Luminance(SAMPLE_TEXTURE2D(_PrevTex, sampler_PrevTex, texCoord).rgb);
    float x_n = Luminance(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texCoord - duv.xz).rgb);
    float x_p = Luminance(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texCoord + duv.xz).rgb);
    float y_n = Luminance(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texCoord - duv.zy).rgb);
    float y_p = Luminance(SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, texCoord + duv.zy).rgb);
    return float4((x_p - x_n) / 2, (y_p - y_n) / 2, cur - pre, 1);
}

float4 FragmentFlow(float4 position : SV_Position,
                    float2 texCoord : TEXCOORD) : SV_Target
{
    float2x2 ATWA = 0;
    float2 ATWb = 0;

    for (int yi = -kWindowWidth; yi <= kWindowWidth; yi++)
    {
        for (int xi = -kWindowWidth; xi <= kWindowWidth; xi++)
        {
            // Offset UV
            float2 uv = texCoord + _MainTex_TexelSize.xy * float2(xi, yi);

            // Gradients
            float3 I = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).xyz;

            // Gaussian weight
            float w = GaussWeight(xi, yi);

            // AT*W*A matrix
            ATWA._m00 += w * I.x * I.x;
            ATWA._m01 += w * I.x * I.y;
            ATWA._m10 += w * I.x * I.y;
            ATWA._m11 += w * I.y * I.y;

            // AT*W*b vector
            ATWb.x -= w * I.x * I.z;
            ATWb.y -= w * I.y * I.z;
        }
    }

    // Solve
    float2 v = mul(Inverse(ATWA), ATWb);

    return float4(v, 0, _DiffMask[0].x);
}

    ENDHLSL

    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off
            Blend Off
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
