Texture2D _CameraNormalsTexture;
#ifndef SHARED_SAMPLER_POINT_CLAMP
#define SHARED_SAMPLER_POINT_CLAMP
SamplerState SharedSampler_Point_Clamp
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};
#endif

void GetCameraWorldNormals_float(float2 UV, out float4 WorldNormal)
{
    WorldNormal = SAMPLE_TEXTURE2D_LOD(_CameraNormalsTexture, SharedSampler_Point_Clamp, UV, 0);
}

void GetCameraWorldNormals_half(half2 UV, out half4 WorldNormal)
{
    WorldNormal = SAMPLE_TEXTURE2D_LOD(_CameraNormalsTexture, SharedSampler_Point_Clamp, UV, 0);
}
