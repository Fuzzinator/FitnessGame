void Soft_Rectangle_float(float2 UV, float Width, float Height, out float Out)
{
    float2 d = abs(UV * 2 - 1) - float2(Width, Height);
#if defined(SHADER_STAGE_RAY_TRACING)
    d = saturate((1 - saturate(d * FLT_MAX)));
#else
    d = //smoothstep(1 - d / fwidth(d), 0 ,1);
#endif
    Out = min(d.x, d.y);
}

void Soft_Square_float(float2 uv, float size, out float Out)
{
    float2 r = length(max(abs(uv*2-1)- size,0));
    Out = r.x;
}

float3 GetTriplanarWeights (float3 normals) {
    float3 triW = abs(normals);
    return triW / (triW.x + triW.y + triW.z);
}

void Triplaner_UVs_float(float3 position, float3 normals, out float2 UVs)
{
    float2 xy = position.xy;
    float2 xz = position.xz;
    float2 yz = position.yz;
    float3 weights = GetTriplanarWeights(normals);
    UVs = ((xy*weights.x+xz*weights.y+yz*weights.z)/3);
}
