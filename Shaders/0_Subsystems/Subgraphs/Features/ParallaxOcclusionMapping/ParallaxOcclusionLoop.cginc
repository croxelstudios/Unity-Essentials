void ParallaxOcclusionLoop_float(
	Texture2D Heightmap, float Amplitude, SamplerState _SamplerState, float Channel, 
	float2 UV, float3 ViewDirUVs, int Steps, float Lod, float LodThreshold,
	out float2 NewUVs, out float Depth)
{
	float stepSize = 1.0 / Steps;
	float2 maxOffset = ViewDirUVs.xy / -ViewDirUVs.z;
	float2 stepOffset = maxOffset * stepSize;
	float2 uvOffset = float2(0.0, 0.0);

	float prevHeight = SAMPLE_TEXTURE2D_LOD(Heightmap, _SamplerState, UV + uvOffset, Lod)[Channel];
	uvOffset += stepOffset;
	float currHeight = SAMPLE_TEXTURE2D_LOD(Heightmap, _SamplerState, UV + uvOffset, Lod)[Channel];

	float rayHeight = 1.0 - stepSize;

	[loop]
	for (int i = 0; i < Steps; i++)
	{
		if (currHeight > rayHeight)
			break;

		prevHeight = currHeight;
		rayHeight -= stepSize;
		uvOffset += stepOffset;
		currHeight = SAMPLE_TEXTURE2D_LOD(Heightmap, _SamplerState, UV + uvOffset, Lod)[Channel];
	}

	float h0 = rayHeight + stepSize;
    float h1 = rayHeight;

    float d0 = h0 - prevHeight;
    float d1 = h1 - currHeight;

    float2 finalOffset = uvOffset;
    
	[loop]
    for (int j = 0; j < 3; j++)
    {
        float denom = (d1 - d0);
        if (abs(denom) < 1e-5)
            break;

        float hitHeight = (h0 * d1 - h1 * d0) / denom;
        finalOffset = (1.0 - hitHeight) * stepOffset * Steps;

        currHeight = SAMPLE_TEXTURE2D_LOD(Heightmap, _SamplerState, UV + finalOffset, Lod)[Channel];

        float delta = hitHeight - currHeight;
        if (abs(delta) <= 0.01)
            break;

        if (delta < 0.0)
        {
            d1 = delta;
            h1 = hitHeight;
        }
        else
        {
            d0 = delta;
            h0 = hitHeight;
        }
    }

    finalOffset *= (1.0 - saturate(Lod - LodThreshold));

    NewUVs = UV + finalOffset;
    Depth = (Amplitude - (currHeight * Amplitude))/max(ViewDirUVs.z, 0.0001);
}
