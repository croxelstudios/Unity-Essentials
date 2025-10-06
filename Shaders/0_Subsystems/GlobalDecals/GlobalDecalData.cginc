#pragma dynamic_branch USE_GLOBAL_DECAL_SYSTEM
half4 _GlobalDecalColor_0;
Texture2D _GlobalDecalTex_0;
SamplerState sampler_GlobalDecalTex_0;
float4x4 _GlobalDecalMatrix_0;
half _GlobalDecalIsAlpha_0;
half _GlobalDecalRepaint_0;
half4 _GlobalDecalRepaintColor_0;
int _GlobalDecalRenderingLayers_0;

float4 Remap(float4 In, float2 InMinMax, float2 OutMinMax)
{
    return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x)
        / (InMinMax.y - InMinMax.x);
}

bool IsInRange(float2 uv)
{
    return (uv.x >= 0.0) && (uv.x <= 1.0) && (uv.y >= 0.0) && (uv.y <= 1.0);
}

void GetDecalColor_half(float3 WorldPosition, out float4 DecalColor)
{
    if (!USE_GLOBAL_DECAL_SYSTEM)
    {
        DecalColor = float4(0, 0, 0, 0);
        return;
    }

    uint meshRenderingLayers = GetMeshRenderingLayer();
    if (!IsMatchingLightLayer(_GlobalDecalRenderingLayers_0, meshRenderingLayers))
    {
        DecalColor = float4(0, 0, 0, 0);
        return;
    }

    float4 UV = mul(_GlobalDecalMatrix_0, float4(WorldPosition, 1.0));
    UV = Remap(UV, float2(-0.5, 0.5), float2(0.0, 1.0));

    bool isRepaint = (_GlobalDecalRepaint_0 > 0.5) && !IsInRange(UV.xy);
    if (isRepaint)
    {
        DecalColor = _GlobalDecalRepaintColor_0;
        return;
    }

    float4 texResult =
        SAMPLE_TEXTURE2D_LOD(_GlobalDecalTex_0, sampler_GlobalDecalTex_0, UV, 0);
    if (_GlobalDecalIsAlpha_0 > 0.5)
    {
        DecalColor = _GlobalDecalColor_0;
        DecalColor.a = texResult.r * _GlobalDecalColor_0.a;
    }
    else
    {
        DecalColor = texResult * _GlobalDecalColor_0;
    }
}
