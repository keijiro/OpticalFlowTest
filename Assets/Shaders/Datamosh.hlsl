void DatamoshSampler_float
  (UnityTexture2D Source,
   UnityTexture2D Flow,
   float2 UV,
   float Scale,
   out float3 Output)
{
    float2 vec = tex2D(Flow, round(UV * float2(40, 24)) / float2(40, 24)).xy;
    //float2 vec = tex2D(Flow, UV).xy;
    Output = tex2D(Source, UV + Source.texelSize.xy * vec * -Scale).rgb;
}
