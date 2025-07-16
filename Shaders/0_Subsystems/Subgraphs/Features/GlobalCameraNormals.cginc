Texture2D _CameraNormalsTexture;
SamplerState sampler_CameraNormalsTexture;

void GetCameraWorldNormals_half(float2 UV, out float4 WorldNormal)
{
    WorldNormal = SAMPLE_TEXTURE2D_LOD(_CameraNormalsTexture, sampler_CameraNormalsTexture, UV, 0);  
}
