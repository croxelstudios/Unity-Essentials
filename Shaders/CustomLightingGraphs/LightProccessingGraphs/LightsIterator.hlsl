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
            uint lightsCount = GetAdditionalLightsCount();
            #ifdef _FORWARD_PLUS
                #if SHADOWS_SCREEN
                    float4 shadowCoord = ComputeScreenPos(positionCS);
                #else
                    float4 shadowCoord = TransformWorldToShadowCoord(Position);
                #endif
                InputData inputData = (InputData)0;
                inputData.positionWS = Position;
                inputData.normalWS = Normal;
                inputData.viewDirectionWS = ViewDirection;
                inputData.shadowCoord = shadowCoord;
                float4 positionCS = TransformWorldToHClip(Position);
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
    
                    LightingResult.rgb += OutVector4_1.rgb;
                    LightingResult.a *= OutVector4_1.a;
	                SpecularMask += sm;
                LIGHT_LOOP_END
            #else
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
    
                    LightingResult.rgb += OutVector4_1.rgb;
                    LightingResult.a *= OutVector4_1.a;
	                SpecularMask += sm;
                }
            #endif
        #endif
    #endif
}

void AdditionalLightsLoopV2_float(float4 Albedo, float3 Normal, float Smoothness,
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
            InputData inputData = (InputData)0;
            inputData.positionWS = Position;
            inputData.normalWS = Normal;
            inputData.viewDirectionWS = ViewDirection;
            inputData.shadowCoord = shadowCoord;
            inputData.normalizedScreenSpaceUV =
                float2((positionCS.xy * rcp(positionCS.w)) * 0.5 + 0.5);
            half4 shadowMask = CalculateShadowMask(inputData);

            SurfaceData surfData = (SurfaceData)0;
            surfData.albedo = Albedo.rgb;
            surfData.specular = half3(0, 0, 0);
            surfData.metallic = 0;
            surfData.smoothness = Smoothness;
            surfData.normalTS = half3(0, 1, 0);
            surfData.emission = half3(0, 0, 0);
            surfData.occlusion = 0;
            surfData.alpha = Albedo.a;
            surfData.clearCoatMask = 0;
            surfData.clearCoatSmoothness = 0;
            AmbientOcclusionFactor aoFactor = CreateAmbientOcclusionFactor(inputData, surfData);
            
            uint meshRenderingLayers = GetMeshRenderingLayer();

            #if USE_FORWARD_PLUS
            [loop] for (uint lightIndex = 0; lightIndex <
                min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
            {
                FORWARD_PLUS_SUBTRACTIVE_LIGHT_CHECK

                Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

                #ifdef _LIGHT_LAYERS
                    if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
                #endif
                {
                    float4 OutVector4_1;
	                float sm;
                    AdditionalLight_float(Albedo, Normal, Smoothness, LightCurveFactor,
	                    RimLightAmount, BackRimLightAmount, RampTexture, UseRampTexture,
		                ToonSubdivisions, LightStepsCurveFactor, Invert, 0, Position, 
		                WorldSpaceNormal, ViewDirection, light.direction, light.color, 
		                light.shadowAttenuation, light.distanceAttenuation,
                        OutVector4_1, sm);
    
                    LightingResult.rgb += OutVector4_1.rgb;
                    LightingResult.a *= OutVector4_1.a;
	                SpecularMask += sm;
                }
            }
            #endif

            LIGHT_LOOP_BEGIN(lightsCount)
            Light light = GetAdditionalLight(lightIndex, inputData, shadowMask, aoFactor);

            #ifdef _LIGHT_LAYERS
                if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
            #endif
            {
                    float4 OutVector4_1;
	                float sm;
                    AdditionalLight_float(Albedo, Normal, Smoothness, LightCurveFactor,
	                    RimLightAmount, BackRimLightAmount, RampTexture, UseRampTexture,
		                ToonSubdivisions, LightStepsCurveFactor, Invert, 0, Position, 
		                WorldSpaceNormal, ViewDirection, light.direction, light.color, 
		                light.shadowAttenuation, light.distanceAttenuation,
                        OutVector4_1, sm);
    
                    LightingResult.rgb += OutVector4_1.rgb;
                    LightingResult.a *= OutVector4_1.a;
	                SpecularMask += sm;
            }
            LIGHT_LOOP_END
        #endif
    #endif
}