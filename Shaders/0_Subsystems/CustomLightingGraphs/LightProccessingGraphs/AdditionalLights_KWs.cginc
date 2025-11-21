#pragma shader_feature _ADDITIONAL_LIGHT_SHADOWS
#pragma shader_feature _ADDITIONAL_LIGHTS
#pragma shader_feature ADDITIONAL_LIGHT_CALCULATE_SHADOWS
#pragma shader_feature LIGHTMAP
#pragma shader_feature _CLUSTER_LIGHT_LOOP

void PassthroughAdditionalLightsKW_float(float3 Vector, out float3 Out)
{
	Out = Vector;
}

void PassthroughAdditionalLightsKW_half(half3 Vector, out half3 Out)
{
	Out = Vector;
}
