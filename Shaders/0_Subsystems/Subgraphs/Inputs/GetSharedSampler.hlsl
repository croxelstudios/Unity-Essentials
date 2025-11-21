#ifndef SHARED_SAMPLER_POINT_CLAMP
#define SHARED_SAMPLER_POINT_CLAMP
SamplerState SharedSampler_Point_Clamp
{
    Filter = MIN_MAG_MIP_POINT;
    AddressU = Clamp;
    AddressV = Clamp;
};
#endif

void GetSharedSampler_float(out UnitySamplerState Sampler)
{
    Sampler = UnityBuildSamplerStateStruct(SharedSampler_Point_Clamp);
}

void GetSharedSampler_half(out UnitySamplerState Sampler)
{
    Sampler = UnityBuildSamplerStateStruct(SharedSampler_Point_Clamp);
}
