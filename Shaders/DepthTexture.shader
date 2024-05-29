Shader "Custom/DepthTexture"
{
	Properties
	{
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Lighting Off
		ZWrite Off

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "UnityCG.cginc"
			
			struct v2f
			{
				float4 pos : POSITION;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			v2f vert(v2f i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_TRANSFER_INSTANCE_ID(i, o);
				o.pos = UnityObjectToClipPos(i.pos);
				o.texcoord = i.texcoord;
				return o;
			}

			UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 c = tex2D(_CameraDepthTexture, i.texcoord);
				c.g = c.b = c.r;
				if (c.r <= 0) c.a = 0;
				return c;
			}
		ENDCG
		}
	}
}