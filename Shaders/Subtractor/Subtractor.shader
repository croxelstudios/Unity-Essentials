Shader "Unlit/Subtractor"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Name "InternalFaces"
            Tags { "LightMode" = "UniversalForward" }
            Cull Front
            ZWrite On
            ZTest Greater
            ColorMask R
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float3 vertex : POSITION;
            };

            struct v2f
            {
                float4 clipPos : SV_POSITION;
                float3 pos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = mul(unity_ObjectToWorld, float4(v.vertex, 1.0)).xyz;
                o.clipPos = mul(UNITY_MATRIX_VP, float4(o.pos, 1.0));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 clipPos = mul(UNITY_MATRIX_VP, float4(i.pos, 1.0));
                fixed depthClip = clipPos.z / clipPos.w;
                #if defined(UNITY_REVERSED_Z)
                #else
	                depthClip = 1.0 - depthClip;
                #endif
                fixed depthLinear = 1.0 / (_ZBufferParams.x * depthClip + _ZBufferParams.y);

                return fixed4(depthLinear, 0.0, 0.0, 1.0);
            }
            ENDCG
        }

        Pass
        {
            Name "ExternalFaces"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            ZTest LEqual
            ColorMask G
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct appdata
            {
                float3 vertex : POSITION;
            };

            struct v2f
            {
                float4 clipPos : SV_POSITION;
                float3 pos : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = mul(unity_ObjectToWorld, float4(v.vertex, 1.0)).xyz;
                o.clipPos = mul(UNITY_MATRIX_VP, float4(o.pos, 1.0));
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 clipPos = mul(UNITY_MATRIX_VP, float4(i.pos, 1.0));
                fixed depthClip = clipPos.z / clipPos.w;
                #if defined(UNITY_REVERSED_Z)
                #else
	                depthClip = 1.0 - depthClip;
                #endif
                fixed depthLinear = 1.0 / (_ZBufferParams.x * depthClip + _ZBufferParams.y);

                return fixed4(0.0, depthLinear, 0.0, 1.0);
            }
            ENDCG
        }
    }
}
