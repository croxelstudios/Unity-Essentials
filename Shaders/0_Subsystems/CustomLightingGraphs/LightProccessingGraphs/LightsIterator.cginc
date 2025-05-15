// void AdditionalLightsLoop_half(float3 Normal, float Smoothness,
//     float LightCurveFactor, float RimLightAmount, float BackRimLightAmount,
//     UnityTexture2D RampTexture, bool UseRampTexture, float ToonSubdivisions,
//     bool BinaryShadows, float LightStepsCurveFactor, bool Invert,
//     float4 SubgraphInitialization, float3 Position, float3 WorldSpaceNormal, bool ReceiveShadows,
//     out float4 Diffuse, out float3 Specular, out float3 RimLight, out float3 BackRimLight, out float SpecularMask)
// {
//     Diffuse = float4(0, 0, 0, 1);
//     Specular = float3(0, 0, 0);
//     RimLight = float3(0, 0, 0);
//     BackRimLight = float3(0, 0, 0);
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
// #else
//         for (uint lightI = 0; lightI < min(MAX_VISIBLE_LIGHTS, lightsCount); lightI++)
//         {
//             Light light = GetAdditionalLight(lightI, Position, 1);
    
// #ifdef _LIGHT_LAYERS
//                 if (IsMatchingLightLayer(light.layerMask, meshRenderingLayers))
// #endif
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
//         }
// #endif
// #endif
// #endif
// }

// void AdditionalLight_float(float3 Normal, float Smoothness,
//     float LightCurveFactor, float RimLightAmount, float BackRimLightAmount,
//     UnityTexture2D RampTexture, bool UseRampTexture, float ToonSubdivisions,
//     bool BinaryShadows, float LightStepsCurveFactor, bool Invert,
//     float4 SubgraphInitialization, float3 Position, float3 WorldSpaceNormal,
//     float3 LightDir, float3 LightColor, float ShadowAtten, float DistanceAtten,
//     out float4 Diffuse, out float3 Specular, out float3 RimLight, out float3 BackRimLight)
// {
//     Bindings_LightProcessor_891721543fd22034bab66ea6db7b508c_half bindings;
//     bindings.WorldSpaceNormal = WorldSpaceNormal;
//     bindings.WorldSpacePosition = Position;
//     SG_LightProcessor_891721543fd22034bab66ea6db7b508c_half(Normal, 1,
// 	    Smoothness, LightCurveFactor, RimLightAmount, BackRimLightAmount, 
// 	    UseRampTexture, RampTexture, UseRampTexture,
// 	    ToonSubdivisions, LightStepsCurveFactor, BinaryShadows, Invert,
// 	    LightDir, 1, LightColor, 1, ShadowAtten, 1,
// 	    DistanceAtten, Position, 1, bindings,
//         Diffuse, Specular, RimLight, BackRimLight);
// }