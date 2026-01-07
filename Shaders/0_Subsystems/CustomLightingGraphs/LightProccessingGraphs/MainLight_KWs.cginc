#pragma shader_feature _MAIN_LIGHT_CALCULATE_SHADOWS
#pragma shader_feature _MAIN_LIGHT_SHADOWS
#pragma shader_feature _MAIN_LIGHT_SHADOWS_SCREEN
#pragma shader_feature _MAIN_LIGHT_SHADOWS_CASCADES
#pragma shader_feature _SHADOWS_SOFT
#pragma shader_feature _LIGHT_COOKIES
#pragma shader_feature _LIGHT_LAYERS

void PassthroughMainLightKW_float(float3 Vector, out float3 Out)
{
	Out = Vector;
}

void PassthroughMainLightKW_half(half3 Vector, out half3 Out)
{
	Out = Vector;
}
