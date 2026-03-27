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

//         float2 uv = (positionCS.xy / positionCS.w) * 0.5 + 0.5;
//         #if defined(UNITY_UV_STARTS_AT_TOP)
//             uv.y = 1.0 - uv.y;
//         #endif
//         inputData.normalizedScreenSpaceUV = uv;
        
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
                
//                 Light light = GetAdditionalLight(lightIndex, inputData.positionWS, 1);

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
//                     bindings.NDCPosition = ScreenPosition;
//                     SG_LightProcessor_891721543fd22034bab66ea6db7b508c_half(
//                         Position, 1, Normal, 1, 
//                         light.direction, 1, light.color, 1, light.distanceAttenuation,
//                         shadowAtten, 1,
//                         LightCurveFactor, Smoothness, RimLightAmount, BackRimLightAmount, 
// 	                    Invert,
//                         ToonSubdivisions, LightStepsCurveFactor, UseRampTexture, 
// 	                    RampTexture, UseRampTexture, BinaryShadows, SmoothStep, DitherStep,
	                    
// 	                    bindings,
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
//             Light light = GetAdditionalLight(lightIndex, inputData.positionWS, 1);

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
//                 bindings.NDCPosition = ScreenPosition;
//                 SG_LightProcessor_891721543fd22034bab66ea6db7b508c_half(
//                         Position, 1, Normal, 1, 
//                         light.direction, 1, light.color, 1, light.distanceAttenuation,
//                         shadowAtten, 1,
//                         LightCurveFactor, Smoothness, RimLightAmount, BackRimLightAmount, 
// 	                    Invert,
//                         ToonSubdivisions, LightStepsCurveFactor, UseRampTexture, 
// 	                    RampTexture, UseRampTexture, BinaryShadows, SmoothStep, DitherStep,
	                    
// 	                    bindings,
//                         _Diffuse, _Specular, _RimLight, _BackRimLight, _SpecularMask);
    
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
