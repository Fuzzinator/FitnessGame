void InverseFresnelEffect_float(float3 Normal, float3 ViewDir, float Power, out float Out)
{
    Out = pow(saturate(dot(normalize(Normal), normalize(ViewDir))), Power);
}
void InverseFresnelEffect_half(half3 Normal, half3 ViewDir, half Power, out half Out)
{
    Out = pow(saturate(dot(normalize(Normal), normalize(ViewDir))), Power);
}
