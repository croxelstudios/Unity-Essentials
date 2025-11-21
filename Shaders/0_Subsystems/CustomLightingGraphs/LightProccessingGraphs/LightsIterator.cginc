// Diffuse = float4(0, 0, 0, 1);
// Specular = float3(0, 0, 0);
// RimLight = float3(0, 0, 0);
// BackRimLight = float3(0, 0, 0);
// SpecularMask = 0;

// #if !defined(SHADERGRAPH_PREVIEW)
//     #if defined(_ADDITIONAL_LIGHTS)
//         uint lightsCount = GetAdditionalLightsCount();
//         uint meshRenderingLayers = GetMeshRenderingLayer();

//         float4 positionCS = TransformWorldToHClip(Position);
//         #if SHADOWS_SCREEN
//             float4 shadowCoord = ComputeScreenPos(positionCS);
//         #else
//             float4 shadowCoord = TransformWorldToShadowCoord(Position);
//         #endif

//         InputData inputData = (InputData)0;
//         #if defined(REQUIRES_WORLD_SPACE_POS_INTERPOLATOR)
//             inputData.positionWS = Position;
//         #endif
//         #if defined(DEBUG_DISPLAY)
//             inputData.positionCS = positionCS;
//         #endif
//         inputData.normalWS = Normal;
//         inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(Position); 
//         inputData.shadowCoord = shadowCoord;
//         inputData.normalizedScreenSpaceUV =
//             float2((positionCS.xy * rcp(positionCS.w)) * 0.5 + 0.5);

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
