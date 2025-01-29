void DatamoshSampler_float
  (UnityTexture2D Source, UnityTexture2D Flow, float2 UV, out float3 Output)
{
    float2 vec = tex2D(Flow, round(UV * float2(40, 22)) / float2(40, 22)).xy;
    Output = tex2D(Source, UV + Source.texelSize.xy * vec * -2.0).rgb;
}
