Shader "Custom/ShadowCaster"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_CutOff("CutOff", Range(0, 1)) = 0.5
	}

	SubShader
	{
		Pass
		{
			Tags {"LightMode" = "ShadowCaster"}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f
			{
				float4 texcoord : TEXCOORD0;
				V2F_SHADOW_CASTER;
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				o.texcoord = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			sampler2D _MainTex;
			float _CutOff;

			float4 frag(v2f i) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, i.texcoord);
				clip(c.a - _CutOff);
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}
