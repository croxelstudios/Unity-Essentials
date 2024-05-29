Shader "Custom/Projector" {
	Properties
	{
		_ProjectorTex("Texture", 2D) = "white" {}
		_FalloffTex ("FallOff", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		[MaterialToggle] _Emissive("Emissive", Float) = 0
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_CutOff("CutOff", Range(0, 1)) = 0
		[MaterialToggle] _CutOffIgnoresColor("Cutoff ignores material color", Float) = 0
		[MaterialToggle] _ColorAlphaMultiply("ColorAlphaMultiply", Float) = 0

		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
		[PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
		[PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
	}
	Subshader
	{
		Tags {"Queue"="Transparent"}
		Pass
		{
			ZWrite Off
			ColorMask RGB
			Blend DstColor Zero
			Offset -1, -1

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "Assets/Utilities - HypercubeCore/Essentials/Shaders/CGIncludes/HcCBaseCG.cginc"

			struct v2fProj
			{
				float4 vertex   : SV_POSITION;
				fixed4 color : COLOR;
				float4 texcoord : TEXCOORD0;
				float4 uvFalloff : TEXCOORD1;
				UNITY_FOG_COORDS(2)
				UNITY_VERTEX_OUTPUT_STEREO
			};
			
			float4x4 unity_Projector;
			float4x4 unity_ProjectorClip;
			
			v2fProj vert (appdata_t IN)
			{
				v2f i = SpriteVert(IN);
				v2fProj o;
				o.vertex = i.vertex;
				o.color = i.color;
				o.texcoord.xy = i.texcoord;

				o.texcoord = mul (unity_Projector, IN.vertex);
				o.uvFalloff = mul (unity_ProjectorClip, IN.vertex);
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			sampler2D _ProjectorTex;
			sampler2D _FalloffTex;

			float _CutOff;
			bool _ColorAlphaMultiply;
			bool _CutOffIgnoresColor;

			bool _Emissive;

			fixed4 SampleSpriteTextureProj(float4 uv)
			{
				fixed4 color = tex2Dproj(_ProjectorTex, UNITY_PROJ_COORD(uv));

#if ETC1_EXTERNAL_ALPHA
				fixed4 alpha = tex2Dproj(_AlphaTex, UNITY_PROJ_COORD(uv));
				color.a = lerp(color.a, alpha.r, _EnableExternalAlpha);
#endif

#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
				if (_AlphaSplitEnabled) color.a = tex2Dproj(_AlphaTex, UNITY_PROJ_COORD(uv)).r;
#endif

				return color;
			}
			
			fixed4 frag(v2fProj IN) : SV_Target
			{
				fixed4 c = SampleSpriteTextureProj(IN.texcoord);
				if (_CutOffIgnoresColor) clip(c.a - _CutOff);
				c *= IN.color;
				if (!_CutOffIgnoresColor) clip(c.a - _CutOff);

				if (_Emissive) c = IN.color;
				if (_ColorAlphaMultiply) c.rgb *= c.a;
				//c.a = 1.0-c.a;

				fixed4 texF = tex2Dproj (_FalloffTex, UNITY_PROJ_COORD(IN.uvFalloff));
				fixed4 res = lerp(fixed4(1,1,1,0), c, texF.a * c.a);

				UNITY_APPLY_FOG_COLOR(IN.fogCoord, res, fixed4(1,1,1,1));
				return res;
			}
			ENDCG
		}
	}
}
