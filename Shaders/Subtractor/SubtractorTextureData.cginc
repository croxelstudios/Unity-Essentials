Texture2D _3DSubtractor;
#ifndef SHARED_SAMPLER_POINT_CLAMP
#define SHARED_SAMPLER_POINT_CLAMP
SamplerState SharedSampler_Point_Clamp
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};
#endif

void GetSubtractorsData_float(float2 UV, out float Back, out float Front)
{
    float2 data = SAMPLE_TEXTURE2D_LOD(_3DSubtractor, SharedSampler_Point_Clamp, UV, 0).xy;
    Back = data.x;
    Front = data.y;
}

void GetSubtractorsData_half(half2 UV, out half Back, out half Front)
{
    half2 data = SAMPLE_TEXTURE2D_LOD(_3DSubtractor, SharedSampler_Point_Clamp, UV, 0).xy;
    Back = data.x;
    Front = data.y;
}
