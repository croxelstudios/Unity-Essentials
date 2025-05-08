// void AdditionalLightsLoop_float(float4 Albedo, float3 Normal, float Smoothness,
//     float LightCurveFactor, float RimLightAmount, float BackRimLightAmount,
//     UnityTexture2D RampTexture, bool UseRampTexture, float ToonSubdivisions,
//     bool BinaryShadows, float LightStepsCurveFactor, bool Invert,
//     float4 SubgraphInitialization, float3 Position, float3 WorldSpaceNormal,
//     bool ReceiveShadows, out float4 LightingResult, out float SpecularMask)
// {
//     LightingResult = float4(0, 0, 0, 1);
//     SpecularMask = 0;
// #if !defined(SHADERGRAPH_PREVIEW)
// #ifdef _ADDITIONAL_LIGHTS
//         uint lightsCount = GetAdditionalLightsCount();
//         uint meshRenderingLayers = GetMeshRenderingLayer();

// #ifdef _FORWARD_PLUS
// #if SHADOWS_SCREEN
//             float4 shadowCoord = ComputeScreenPos(positionCS);
// #else
//             float4 shadowCoord = TransformWorldToShadowCoord(Position);
// #endif
//         InputData inputData = (InputData)0;
//         inputData.positionWS = Position;
//         inputData.normalWS = Normal;
//         //inputData.viewDirectionWS = ViewDirection;
//         inputData.shadowCoord = shadowCoord;
//         float4 positionCS = TransformWorldToHClip(Position);
//         inputData.normalizedScreenSpaceUV =
//             float2((positionCS.xy * rcp(positionCS.w)) * 0.5 + 0.5);
                
//         LIGHT_LOOP_BEGIN(lightsCount)
//             Light light = GetAdditionalLight(lightIndex, Position, 1);
    
// #ifdef _LIGHT_LAYERS
//                 if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
// #endif
//             {
//                 float4 OutVector4_1 = float4(0, 0, 0, 1);
// 	            float sm = 0;
                    	
// 		        float shadowAtten = ReceiveShadows ? light.shadowAttenuation : 1.0;
//                 AdditionalLight_float(Albedo, Normal, Smoothness, LightCurveFactor,
// 	                RimLightAmount, BackRimLightAmount, RampTexture, UseRampTexture,
// 		            ToonSubdivisions, BinaryShadows, LightStepsCurveFactor, Invert, 0, Position, 
// 		            WorldSpaceNormal, light.direction, light.color, 
// 		            shadowAtten, light.distanceAttenuation,
//                     OutVector4_1, sm);
    
//                 LightingResult.rgb += OutVector4_1.rgb;
//                 LightingResult.a *= OutVector4_1.a;
// 	            SpecularMask += sm;
//             }
//         LIGHT_LOOP_END
// #else
//         for (uint lightI = 0; lightI < min(MAX_VISIBLE_LIGHTS, lightsCount); lightI++)
//         {
//             Light light = GetAdditionalLight(lightI, Position, 1);
    
// #ifdef _LIGHT_LAYERS
//                 if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
// #endif
//             {
//                 float4 OutVector4_1 = float4(0, 0, 0, 1);
// 	            float sm = 0;
		    
// 		        float shadowAtten = ReceiveShadows ? light.shadowAttenuation : 1.0;
//                 AdditionalLight_float(Albedo, Normal, Smoothness, LightCurveFactor,
// 	                RimLightAmount, BackRimLightAmount, RampTexture, UseRampTexture,
// 		            ToonSubdivisions, BinaryShadows, LightStepsCurveFactor, Invert, 0, Position, 
// 		            WorldSpaceNormal, light.direction, light.color, 
// 		            shadowAtten, light.distanceAttenuation,
//                     OutVector4_1, sm);
    
//                 LightingResult.rgb += OutVector4_1.rgb;
//                 LightingResult.a *= OutVector4_1.a;
// 	            SpecularMask += sm;
//             }
//         }
// #endif
// #endif
// #endif
// }

// void AdditionalLight_float(float4 Albedo, float3 Normal, float Smoothness,
//     float LightCurveFactor, float RimLightAmount, float BackRimLightAmount,
//     UnityTexture2D RampTexture, bool UseRampTexture, float ToonSubdivisions,
//     bool BinaryShadows, float LightStepsCurveFactor, bool Invert,
//     float4 SubgraphInitialization, float3 Position, float3 WorldSpaceNormal,
//     bool ReceiveShadows, out float4 LightingResult, out float SpecularMask)
// {
//     Bindings_LightProcessing_e2dcdf5b1a4895c4b980fd3aca5d0224_float bindings;
//     bindings.WorldSpaceNormal = WorldSpaceNormal;
//     bindings.WorldSpacePosition = Position;
//     SG_LightProcessing_e2dcdf5b1a4895c4b980fd3aca5d0224_float(Albedo, Normal, 1,
// 	    Smoothness, LightCurveFactor, RimLightAmount, BackRimLightAmount, 
// 	    UseRampTexture, RampTexture, UseRampTexture,
// 	    ToonSubdivisions, BinaryShadows, LightStepsCurveFactor, Invert,
// 	    LightDir, 1, LightColor, ShadowAtten,
// 	    DistanceAtten, Position, bindings, OutVector4_1, SpecularMask);
// }