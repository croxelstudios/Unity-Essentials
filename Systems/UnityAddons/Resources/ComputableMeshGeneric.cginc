RWByteAddressBuffer vertices;
uint vertexStride;
uint vertexCount;
RWByteAddressBuffer indices;
uint indexStride;
uint indexCount;
uint indexStart;

struct VertexData
{
    float3 position;
    float3 normal;
    float4 tangent;
    float4 color;
    float2 uv;
};

VertexData AverageVertexData(VertexData v1, VertexData v2)
{
    VertexData vd;
    vd.position = (v1.position + v2.position) * 0.5;
    vd.normal = (v1.normal + v2.normal) * 0.5;
    vd.tangent = (v1.tangent + v2.tangent) * 0.5;
    vd.color = (v1.color + v2.color) * 0.5;
    vd.uv = (v1.uv + v2.uv) * 0.5;
    return vd;
}

float3 ExtractVertexPosition(uint i)
{
    uint v = i * vertexStride;
    return asfloat(vertices.Load3(v));
}

float3 ExtractVertexNormal(uint i)
{
    uint v = (i * vertexStride) + 12;
    return asfloat(vertices.Load3(v));
}

float4 ExtractVertexTangent(uint i)
{
    uint v = (i * vertexStride) + 24;
    return asfloat(vertices.Load4(v));
}

float4 ExtractVertexColor(uint i)
{
    uint v = (i * vertexStride) + 40;
    return asfloat(vertices.Load4(v));
}

float2 ExtractVertexUV(uint i)
{
    uint v = (i * vertexStride) + 56;
    return asfloat(vertices.Load2(v));
}

VertexData ExtractVertexData(uint i)
{
    VertexData vd;
    vd.position = ExtractVertexPosition(i);
    vd.normal = ExtractVertexNormal(i);
    vd.tangent = ExtractVertexTangent(i);
    vd.color = ExtractVertexColor(i);
    vd.uv = ExtractVertexUV(i);
    return vd;
}

void SetVertexPosition(uint i, float3 value)
{
    uint v = i * vertexStride;
    vertices.Store3(v, asuint(value));
}

void SetVertexNormal(uint i, float3 value)
{
    uint v = (i * vertexStride) + 12;
    vertices.Store3(v, asuint(value));
}

void SetVertexTangent(uint i, float4 value)
{
    uint v = (i * vertexStride) + 24;
    vertices.Store4(v, asuint(value));
}

void SetVertexColor(uint i, float4 value)
{
    uint v = (i * vertexStride) + 40;
    vertices.Store4(v, asuint(value));
}

void SetVertexUV(uint i, float2 value)
{
    uint v = (i * vertexStride) + 56;
    vertices.Store2(v, asuint(value));
}

void SetVertexData(uint i, VertexData data)
{
    SetVertexPosition(i, data.position);
    SetVertexNormal(i, data.normal);
    SetVertexTangent(i, data.tangent);
    SetVertexColor(i, data.color);
    SetVertexUV(i, data.uv);
}

uint ExtractIndex(int i)
{
    uint v = i * indexStride;
    return indices.Load(v);
}

void SetIndex(int i, int value)
{
    uint v = i * indexStride;
    indices.Store(v, asuint(value));
}
