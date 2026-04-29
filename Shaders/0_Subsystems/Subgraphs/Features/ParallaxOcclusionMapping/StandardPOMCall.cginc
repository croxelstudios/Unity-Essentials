#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/PerPixelDisplacement.hlsl"

struct PerPixelHeightDisplacementParam
{
	float2 uv;
}

void ParallaxOcclusionLoop_float(
	UnityTexture2D heightmap, float2 UVs, float Amplitude, float Lod, float LodThreshold,
	float Steps, float3 ViewDirUVs,
	out float2 NewUVs, out float Depth)
{

	PerPixelHeightDisplacementParam pom;
	pom.uv = UVs;
	float height;

	NewUVs = UVs + ParallaxOcclusionMapping(Lod, LodThreshold, Steps, ViewDirUVs, pom, height);
	Depth = (Amplitude - (height * Amplitude))/max(ViewDirUVs.z, 0.0001);
}
