Shader "Hidden/OpticalFlowTest/Estimator"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

Texture2D _MainTex;
Texture2D _PrevTex;
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
    uint w, h;
    _MainTex.GetDimensions(w, h);

    // Offset with boundaries
    uint2 tc = texCoord.xy * float2(w, h);
    uint2 tc_n = max(tc, 1) - 1;
    uint2 tc_p = min(tc + 1, uint2(w, h) - 1);

    // Sample points
    float cur = Luminance(_MainTex[tc].rgb);
    float pre = Luminance(_PrevTex[tc].rgb);
    float x_n = Luminance(_MainTex[uint2(tc_n.x, tc.y)].rgb);
    float x_p = Luminance(_MainTex[uint2(tc_p.x, tc.y)].rgb);
    float y_n = Luminance(_MainTex[uint2(tc.x, tc_n.y)].rgb);
    float y_p = Luminance(_MainTex[uint2(tc.x, tc_p.y)].rgb);

    return float4((x_p - x_n) / 2, (y_p - y_n) / 2, cur - pre, 1);
}

float4 FragmentFlow(float4 position : SV_Position,
                    float2 texCoord : TEXCOORD) : SV_Target
{
    uint w, h;
    _MainTex.GetDimensions(w, h);

    int2 org = texCoord.xy * float2(w, h);

    float2x2 ATWA = 0;
    float2 ATWb = 0;

    for (int yi = -kWindowWidth; yi <= kWindowWidth; yi++)
    {
        for (int xi = -kWindowWidth; xi <= kWindowWidth; xi++)
        {
            // Offset with boundaries
            int2 tc = min(max(org + int2(xi, yi), 0), int2(w, h) - 1);

            // Gradients
            float3 I = _MainTex[tc].xyz;

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
