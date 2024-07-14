#ifndef VOXEL_MESH_INFO
#define VOXEL_MESH_INFO

matrix Identity =
{
    { 1, 0, 0, 0 },
    { 0, 1, 0, 0 },
    { 0, 0, 1, 0 },
    { 0, 0, 0, 1 }
};

struct MeshProperties
{
    float useMatrix;
    float useNormal;
    float4x4 mat;
    float flip;
    float3 normal;
    float useColor;
    float4 color;
};

StructuredBuffer<MeshProperties> _PropBuffer;
 
void GetBufferInfo_float(float Id, out float4x4 TrMatrix, out bool Flip, out bool UseNormal,
    out float3 OverrideNormal, out float4 Color)
{
    MeshProperties prop = _PropBuffer[Id];
    if (prop.useMatrix) TrMatrix = prop.mat;
    else TrMatrix = Identity;
    Flip = prop.flip;
    UseNormal = prop.useNormal;
    OverrideNormal = prop.normal;
    if (prop.useColor) Color = prop.color;
    else Color = float4(1, 1, 1, 1);
}

#endif
