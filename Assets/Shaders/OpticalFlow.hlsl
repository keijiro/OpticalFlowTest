static const int kWindowWidth = 4;

// Gaussian weight
float OF_GaussWeight(float x, float y)
{
    float sigma = (kWindowWidth * 2 + 1) / 6.0;
    float2 v = float2(x, y);
    return exp(-dot(v, v) / (2 * sigma * sigma));
}

// Inverse matrix calculation
float2x2 OF_Inverse(float2x2 m)
{
    float a = m._m00, b = m._m01, c = m._m10, d = m._m11;
    float det = a * d - b * c;
    return abs(det) <= 0.000001 ? 0 : float2x2(d, -b, -c, a) / det;
}

// Operator for gradient calculation pass
void OF_Gradients_float
  (UnityTexture2D Source, UnityTexture2D Previous, float2 UV, out float3 Output)
{
    float3 duv = float3(Source.texelSize.xy, 0);
    float cur = Luminance(LinearToSRGB(tex2D(Source, UV).rgb));
    float pre = Luminance(LinearToSRGB(tex2D(Previous, UV).rgb));
    float x_n = Luminance(LinearToSRGB(tex2D(Source, UV - duv.xz).rgb));
    float x_p = Luminance(LinearToSRGB(tex2D(Source, UV + duv.xz).rgb));
    float y_n = Luminance(LinearToSRGB(tex2D(Source, UV - duv.zy).rgb));
    float y_p = Luminance(LinearToSRGB(tex2D(Source, UV + duv.zy).rgb));
    Output = float3((x_p - x_n) / 2, (y_p - y_n) / 2, cur - pre);
}

// Operator for Lucas Kanade estimation pass
void OF_LucasKanade_float
  (UnityTexture2D Gradients, float2 UV, out float3 Output)
{
    float2x2 ATWA = 0;
    float2 ATWb = 0;

    for (int yi = -kWindowWidth; yi <= kWindowWidth; yi++)
    {
        for (int xi = -kWindowWidth; xi <= kWindowWidth; xi++)
        {
            // Offset UV
            float2 uv = UV + Gradients.texelSize.xy * float2(xi, yi);

            // Gradients
            float3 I = tex2D(Gradients, uv).xyz;

            // Gaussian weight
            float w = OF_GaussWeight(xi, yi);

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
    float2 v = mul(OF_Inverse(ATWA), ATWb);

    Output = float3(v, 0);
}
