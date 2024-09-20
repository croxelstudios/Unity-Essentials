void AdditionalLightsLoop_float(float4 Albedo, float3 Normal, float Smoothness,
    float LightCurveFactor, float RimLightAmount, float BackRimLightAmount,
    UnityTexture2D RampTexture, bool UseRampTexture, float ToonSubdivisions,
    float LightStepsCurveFactor, bool Invert, float4 SubgraphInitialization,
    float3 Position, float3 WorldSpaceNormal, float3 ViewDirection,
    out float4 LightingResult, out float SpecularMask)
{
    LightingResult = float4(0, 0, 0, 1);
    SpecularMask = 0;
    #if !defined(SHADERGRAPH_PREVIEW)
        #ifdef _ADDITIONAL_LIGHTS
            float4 positionCS = TransformWorldToHClip(Position);
            #if SHADOWS_SCREEN
                float4 shadowCoord = ComputeScreenPos(positionCS);
            #else
                float4 shadowCoord = TransformWorldToShadowCoord(Position);
            #endif
            uint lightsCount = GetAdditionalLightsCount();
            #ifdef _FORWARD_PLUS
                InputData inputData = (InputData)0;
                inputData.positionWS = Position;
                inputData.normalWS = Normal;
                inputData.viewDirectionWS = ViewDirection;
                inputData.shadowCoord = shadowCoord;
                inputData.normalizedScreenSpaceUV =
                    float2((positionCS.xy * rcp(positionCS.w)) * 0.5 + 0.5);
                
                LIGHT_LOOP_BEGIN(lightsCount)
                    Light light = GetAdditionalLight(lightIndex, Position, 1);
    
                    float4 OutVector4_1;
	                float sm;
                    
                    AdditionalLight_float(Albedo, Normal, Smoothness, LightCurveFactor,
	                    RimLightAmount, BackRimLightAmount, RampTexture, UseRampTexture,
		                ToonSubdivisions, LightStepsCurveFactor, Invert, 0, Position, 
		                WorldSpaceNormal, ViewDirection, light.direction, light.color, 
		                light.shadowAttenuation, light.distanceAttenuation,
                        OutVector4_1, sm);
    
                    LightingResult += OutVector4_1;
	                SpecularMask += sm;
                LIGHT_LOOP_END
            #else //Forward mode crashes game randomly
                for (uint lightI = 0; lightI < lightsCount; lightI++)
                {
                    Light light = GetAdditionalLight(lightI, Position, 1);
    
                    float4 OutVector4_1;
	                float sm;
                    AdditionalLight_float(Albedo, Normal, Smoothness, LightCurveFactor,
	                    RimLightAmount, BackRimLightAmount, RampTexture, UseRampTexture,
		                ToonSubdivisions, LightStepsCurveFactor, Invert, 0, Position, 
		                WorldSpaceNormal, ViewDirection, light.direction, light.color, 
		                light.shadowAttenuation, light.distanceAttenuation,
                        OutVector4_1, sm);
    
                    LightingResult += OutVector4_1;
	                SpecularMask += sm;
                }
            #endif
        #endif
    #endif
}