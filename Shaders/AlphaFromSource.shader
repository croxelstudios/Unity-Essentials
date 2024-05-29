Shader "Custom/AlphaFromSource"
{
	//show values to edit in inspector
	Properties
	{
		_Color("Tint", Color) = (0, 0, 0, 1)
		_MainTex("Texture", 2D) = "white" {}

		_Source("Source Position", Vector) = (0, 1, 0, 0)
		_RadiusMin("Radius Min Value", Float) = 0.5
		_RadiusMax("Radius Max Value", Float) = 0.8
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry"}

		CGPROGRAM
		#pragma surface surf Standard alpha:fade vertex:vert
		#pragma target 3.0
		#include "Assets/Utilities - HypercubeCore/Essentials/Shaders/CGIncludes/HcCFunctionsCG.cginc"

		sampler2D _MainTex;
		float4 _MainTex_ST;

		float3 _Source;
		float _RadiusMin;
		float _RadiusMax;

		struct Input
		{
			float2 uv_MainTex;
			float3 worldNormal;
			float3 viewDir;
			float3 localPos : SV_POSITION;
			INTERNAL_DATA
		};

		void vert(inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input, o);
			o.localPos = v.vertex.xyz;
		}

		UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf(Input i, inout SurfaceOutputStandard o)
		{
			float rangeAlpha = AlphaFromRangeToPosition(i.localPos - _Source, _RadiusMin, _RadiusMax);
			fixed4 col = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
			col.a *= rangeAlpha;
			o.Emission = col;
			o.Alpha = col.a;
		}
		ENDCG
	}
	FallBack "Standard"
}
