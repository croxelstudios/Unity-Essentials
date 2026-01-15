// Diffuse = float4(0, 0, 0, 1);
// Specular = float3(0, 0, 0);
// RimLight = float3(0, 0, 0);
// BackRimLight = float3(0, 0, 0);
// SpecularMask = 0;

// #if !defined(SHADERGRAPH_PREVIEW)
//     #if defined(_ADDITIONAL_LIGHTS)
//         uint lightsCount = GetAdditionalLightsCount();
//         uint meshRenderingLayers = GetMeshRenderingLayer();

//         //InitializeInputData
//         float4 positionCS = TransformWorldToHClip(Position);
//         InputData inputData = (InputData)0;
//         #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
//             inputData.positionWS = Position;
//         #endif
//         #if defined(DEBUG_DISPLAY)
//             inputData.positionCS = positionCS;
//         #endif
//         inputData.normalWS = Normal;
//         inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(Position);

//         float4 screenPos = ComputeScreenPos(positionCS);
//         #if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
//             #if defined(_MAIN_LIGHT_SHADOWS_SCREEN) && !defined(_SURFACE_TYPE_TRANSPARENT)
//                 inputData.shadowCoord = screenPos;
//             #else
//                 inputData.shadowCoord = TransformWorldToShadowCoord(Position);
//             #endif
//         #elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
//             inputData.shadowCoord = TransformWorldToShadowCoord(Position);
//         #else
//             inputData.shadowCoord = float4(0, 0, 0, 0);
//         #endif
//         inputData.fogCoord = InitializeInputDataFog(float4(Position, 1.0),
//             ComputeFogFactor(positionCS.z));

//         inputData.normalizedScreenSpaceUV = float2(GetNormalizedScreenSpaceUV(screenPos) * 0.5 + 0.5);
        
//         #if defined(DEBUG_DISPLAY)
//             #if defined(DYNAMICLIGHTMAP_ON)
//                 inputData.dynamicLightmapUV =
//                     float2(0, 0) * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
//             #endif
//             #if defined(LIGHTMAP_ON)
//                 float2 staticLightmapUV;
//                 OUTPUT_LIGHTMAP_UV(staticLightmapUV, unity_LightmapST, inputData.staticLightmapUV);
//             #endif
//             OUTPUT_SH4(Position, Normal.xyz,
//                 GetWorldSpaceNormalizeViewDir(Position),
//                 inputData.vertexSH, inputData.probeOcclusion);
//         #endif

//         #if USE_CLUSTER_LIGHT_LOOP
//             [loop] for (uint lightIndex = 0; lightIndex < min(URP_FP_DIRECTIONAL_LIGHTS_COUNT, MAX_VISIBLE_LIGHTS); lightIndex++)
//             {
//                 CLUSTER_LIGHT_LOOP_SUBTRACTIVE_LIGHT_CHECK
                
//                 Light light = GetAdditionalLight(lightIndex, inputData.positionWS);

//                 #ifdef _LIGHT_LAYERS
//                     if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
//                 #endif
//                 {
//                     float4 _Diffuse = float4(0, 0, 0, 1);
//                     float3 _Specular = float3(0, 0, 0);
//                     float3 _RimLight = float3(0, 0, 0);
//                     float3 _BackRimLight = float3(0, 0, 0);
//                     float _SpecularMask = 0;

//                     float shadowAtten = ReceiveShadows ? light.shadowAttenuation : 1.0;
//                     Bindings_LightProcessor_891721543fd22034bab66ea6db7b508c_half bindings;
//                     bindings.WorldSpaceNormal = WorldSpaceNormal;
//                     bindings.WorldSpacePosition = Position;
//                     SG_LightProcessor_891721543fd22034bab66ea6db7b508c_half(Normal, 1,
// 	                    Smoothness, LightCurveFactor, RimLightAmount, BackRimLightAmount, 
// 	                    UseRampTexture, RampTexture, UseRampTexture,
// 	                    ToonSubdivisions, LightStepsCurveFactor, BinaryShadows, Invert,
// 	                    light.direction, 1, light.color, 1, shadowAtten, 1,
// 	                    light.distanceAttenuation, Position, 1, bindings,
//                         _Diffuse, _Specular, _RimLight, _BackRimLight, _SpecularMask);
    
//                     Diffuse.rgb += _Diffuse.rgb;
//                     Diffuse.a *= _Diffuse.a;
// 	                Specular += _Specular;
//                     RimLight += _RimLight;
//                     BackRimLight += _BackRimLight;
//                     SpecularMask += _SpecularMask;
//                 }
//             }
//         #endif
//         LIGHT_LOOP_BEGIN(lightsCount)
//             Light light = GetAdditionalLight(lightIndex, inputData.positionWS);

//             #ifdef _LIGHT_LAYERS
//                 if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
//             #endif
//             {
//                 float4 _Diffuse = float4(0, 0, 0, 1);
//                 float3 _Specular = float3(0, 0, 0);
//                 float3 _RimLight = float3(0, 0, 0);
//                 float3 _BackRimLight = float3(0, 0, 0);
//                 float _SpecularMask = 0;

//                 float shadowAtten = ReceiveShadows ? light.shadowAttenuation : 1.0;
//                 Bindings_LightProcessor_891721543fd22034bab66ea6db7b508c_half bindings;
//                 bindings.WorldSpaceNormal = WorldSpaceNormal;
//                 bindings.WorldSpacePosition = Position;
//                 SG_LightProcessor_891721543fd22034bab66ea6db7b508c_half(Normal, 1,
// 	                Smoothness, LightCurveFactor, RimLightAmount, BackRimLightAmount, 
// 	                UseRampTexture, RampTexture, UseRampTexture,
// 	                ToonSubdivisions, LightStepsCurveFactor, BinaryShadows, Invert,
// 	                light.direction, 1, light.color, 1, shadowAtten, 1,
// 	                light.distanceAttenuation, Position, 1, bindings,
//                     _Diffuse, _Specular, _RimLight, _BackRimLight, _SpecularMask);
    
//                 Diffuse.rgb += _Diffuse.rgb;
//                 Diffuse.a *= _Diffuse.a;
// 	            Specular += _Specular;
//                 RimLight += _RimLight;
//                 BackRimLight += _BackRimLight;
//                 SpecularMask += _SpecularMask;
//             }
//         LIGHT_LOOP_END
//     #endif
// #endif

// #if defined(SHADERGRAPH_PREVIEW)
// 	Direction = half3(0.5, 0.5, 0);
// 	Color = float3(1, 0.898, 0.6);
// 	ShaddowAtten = 1;
// #else
//     	float4 shadowCoord;
// 	#ifdef _MAIN_LIGHT_SHADOWS_CASCADES
// 		shadowCoord = TransformWorldToShadowCoord(Position);
// 	#else
// 		shadowCoord =
// 			mul(_MainLightWorldToShadow[0], float4(Position, 1.0));
// 	#endif
//     	Light light = GetMainLight(shadowCoord);


	
// 	Direction = half3(0, 0, 0);
// 	Color = float3(0, 0, 0);
// 	ShaddowAtten = 1;

// #ifdef _LIGHT_LAYERS
// 	if (IsMatchingLightLayer(light.layerMask, GetMeshRenderingLayer()))
// #endif
// 	{
//     		Direction = light.direction;
		

// 		Color = light.color;
// 		ShaddowAtten = light.shadowAttenuation;
// 	}
// #endif

  //       float4 positionCS = TransformWorldToHClip(Position);
		// float4 shadowCoord = TransformWorldToShadowCoord(Position);
  //       InputData inputData = (InputData)0;
  //       inputData.positionWS = Position;
  //       inputData.positionCS = positionCS;
  //       inputData.normalWS = Normal;
  //       inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(Position); 
  //       inputData.shadowCoord = shadowCoord;
  //       inputData.normalizedScreenSpaceUV =
  //           float2((positionCS.xy * rcp(positionCS.w)) * 0.5 + 0.5);