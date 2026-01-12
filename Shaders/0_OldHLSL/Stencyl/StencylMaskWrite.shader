Shader "Custom/StencilMaskWrite"
{
	Properties
	{
		[Enum(Off,0,Front,1,Back,2)] _Cull("Cull", Int) = 2
		[MaterialToggle] _zWrite("Z Write", Float) = 0
		[Enum(Less,0,Greater,1,LEqual,2,GEqual,3,Equal,4,NotEqual,5,Always,6)]
		_zTest("Z Test", Int) = 2
		[IntRange] _StencilRef("Stencil Reference Value", Range(0, 255)) = 1
	}

	SubShader
	{
		Stencil
		{
			Ref[_StencilRef]
			Comp Always
			Pass Replace
		}

		Lighting Off
		Cull[_Cull]
		ZWrite[_zWrite]
		ZTest[_zTest]
		Blend Zero One

		Pass
        {
            Name "DepthOnlyPass"
            Tags { "LightMode" = "DepthOnly" }
			
			ZWrite[_zWrite]
			ZTest[_zTest]
            ColorMask 0

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			int frag(v2f i) : SV_Target
			{
				return 0;
			}
			ENDCG
        }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				return o;
			}

			int frag(v2f i) : SV_Target
			{
				return 0;
			}
			ENDCG
		}
	}
}