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

float4x4 buffer_mat;
float buffer_useNormal;
float3 buffer_normal;
float buffer_flip;
float4 buffer_color;

inline float4x4 TRSMatrix(float3 position, float4 rotation, float3 scale)
{
    float4x4 m = 0.0;

    m[0][0] = (1.0 - 2.0 * (rotation.y * rotation.y + rotation.z * rotation.z)) * scale.x;
    m[1][0] = (rotation.x * rotation.y + rotation.z * rotation.w) * scale.x * 2.0;
    m[2][0] = (rotation.x * rotation.z - rotation.y * rotation.w) * scale.x * 2.0;
    m[3][0] = 0.0;

    m[0][1] = (rotation.x * rotation.y - rotation.z * rotation.w) * scale.y * 2.0;
    m[1][1] = (1.0 - 2.0 * (rotation.x * rotation.x + rotation.z * rotation.z)) * scale.y;
    m[2][1] = (rotation.y * rotation.z + rotation.x * rotation.w) * scale.y * 2.0;
    m[3][1] = 0.0;

    m[0][2] = (rotation.x * rotation.z + rotation.y * rotation.w) * scale.z * 2.0;
    m[1][2] = (rotation.y * rotation.z - rotation.x * rotation.w) * scale.z * 2.0;
    m[2][2] = (1.0 - 2.0 * (rotation.x * rotation.x + rotation.y * rotation.y)) * scale.z;
    m[3][2] = 0.0;

    m[0][3] = position.x;
    m[1][3] = position.y;
    m[2][3] = position.z;
    m[3][3] = 1.0;

    return m;
}

inline void SetUnityMatrices(uint Id, inout float4x4 mat, inout float useNormal,
    inout float3 normal, inout float flip, inout float4 color)
{
#if UNITY_ANY_INSTANCING_ENABLED
    MeshProperties prop = _PropBuffer[Id];
    if (prop.useMatrix) mat = prop.mat;
    else mat = Identity;
    flip = prop.flip;
    useNormal = prop.useNormal;
    normal = prop.normal;
    if (prop.useColor) color = prop.color;
    else color = float4(1, 1, 1, 1);
#endif
}

void passthroughVec3_half(in float3 In, out float3 Out)
{
    Out = In;
}

void passthroughVec3_float(in float3 In, out float3 Out)
{
    Out = In;
}

void setup()
{
#if UNITY_ANY_INSTANCING_ENABLED
    SetUnityMatrices(unity_InstanceID, buffer_mat, buffer_useNormal,
        buffer_normal, buffer_flip, buffer_color);
#endif
}
 
void GetBufferInfo_float(float Id, out float4x4 TrMatrix, out bool Flip, out bool UseNormal,
    out float3 OverrideNormal, out float4 Color)
{
    /*
    TrMatrix = buffer_mat;
    Flip = buffer_flip;
    UseNormal = buffer_useNormal;
    OverrideNormal = buffer_normal;
    Color = buffer_color;
*/
    
    if (_PropBuffer[Id].useMatrix)
        TrMatrix = _PropBuffer[Id].mat;
    else TrMatrix = Identity;
    Flip = _PropBuffer[Id].flip;
    UseNormal = _PropBuffer[Id].useNormal;
    OverrideNormal = _PropBuffer[Id].normal;
    if (_PropBuffer[Id].useColor)
        Color = _PropBuffer[Id].color;
    else Color = float4(1, 1, 1, 1);
}
