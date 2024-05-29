Shader "Custom/Sprites"
{
	Properties
	{
		_MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Saturation("Saturation", Range(0, 1)) = 1
		[MaterialToggle] _ColorAlphaMultiply("ColorAlphaMultiply", Float) = 0
		_BlackValue("BlackValue", Range(0, 1)) = 0

		[Space]
		[Space]
		[Space]
		[MaterialToggle] _Emissive("Silhouette", Float) = 0
		[MaterialToggle] _Opaque("Opaque", Float) = 0
		[Enum(Off,0,Front,1,Back,2)] _Cull("Cull", Int) = 2
		_CutOff("CutOff", Range(0, 1)) = 0.1
		[MaterialToggle] _CutOffIgnoresColor("Cutoff ignores material color", Float) = 0

		[Space]
		[Space]
		[Space]
		_LightColorOverride("Light Color Override", Color) = (1,1,1,1)
		_LightColorReplaceAmount("Light Color Replace Amount", Range(0, 1)) = 0
		_LightThreshold("Light Threshold", Range(0, 1)) = 0.25
		_LightColorIntensity("Light Intensity", Range(0, 1)) = 0.5
		_ShadowColorOverride("Shadow Color Override", Color) = (1,1,1,1)
		_ShadowColorReplaceAmount("Shadow Color Replace Amount", Range(0, 1)) = 0
		_ShadowThreshold("Shadow Threshold", Range(0, 1)) = 0.5
		_ShadowColorIntensity("Shadow Intensity", Range(0, 1)) = 0.25

		[Space]
		[Space]
		[Space]
		[MaterialToggle] _zWrite("Z Write", Float) = 0
		[Enum(Less,0,Greater,1,LEqual,2,GEqual,3,Equal,4,NotEqual,5,Always,6)]
		_zTest("Z Test", Int) = 2

		[Space]
		[Space]
		[Space]
		[Enum(Never,1,Less,2,Equal,3,LEqual,4,Greater,5,NotEqual,6,GEqual,7)]
		_CompareOp("CompareOp", Int) = 4
		[IntRange] _StencilRef("Stencil", Range(0, 255)) = 0
		[HideInInspector] _ReplaceColor("RendererColor", Float) = 0
		[HideInInspector] _ReplaceAlpha("RendererColor", Float) = 0

		[Space]
		[Space]
		[Space]
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
		_AlphaTex("External Alpha", 2D) = "white" {}
		[MaterialToggle] _EnableExternalAlpha("Enable External Alpha", Float) = 0
		[HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
		[HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
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
		
		Stencil
		{
			Ref[_StencilRef]
			Comp[_CompareOp]
		}

		Cull [_Cull]
		ZWrite [_zWrite]
		ZTest [_zTest]
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			Name "CustomSprites"
			CGPROGRAM
			#pragma vertex SpriteVert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "Assets/Utilities - HypercubeCore/Essentials/Shaders/CGIncludes/HcCBaseCG.cginc" //_MainTex, _Color, _AlphaTex

			float _CutOff;
			bool _ColorAlphaMultiply;
			bool _CutOffIgnoresColor;
			float _BlackValue;
			float4 _MainTex_ST;
			float _ReplaceColor;
			float _ReplaceAlpha;
			float _Saturation;

			bool _Emissive;
			bool _Opaque;

			float4 _LightColorOverride;
			float _LightColorReplaceAmount;
			float _LightThreshold;
			float _LightColorIntensity;
			float4 _ShadowColorOverride;
			float _ShadowColorReplaceAmount;
			float _ShadowThreshold;
			float _ShadowColorIntensity;

			fixed4 RGBToGrayscale(fixed4 color)
			{
				fixed grayValue = (color.r * 0.299f) + (color.g * 0.587) + (color.b * 0.114);
				return fixed4(grayValue, grayValue, grayValue, color.a);
			}

			float InvLerp(float from, float to, float value)
			{
				return clamp((value - from) / (to - from), 0, 1);
			}

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = SampleSpriteTexture(IN.texcoord * _MainTex_ST.xy + _MainTex_ST.zw);
				fixed4 grayscale = RGBToGrayscale(c);
				c = lerp(grayscale, c, _Saturation);

				if (_CutOffIgnoresColor) clip(c.a - _CutOff);

				fixed4 targetColor = lerp(c * _LightColorOverride, _LightColorOverride, _LightColorReplaceAmount);
				float amount = InvLerp(_LightThreshold, lerp(1, _LightThreshold, _LightColorIntensity), grayscale);
				c = lerp(c, targetColor, amount);

				targetColor = lerp(c * _ShadowColorOverride, _ShadowColorOverride, _ShadowColorReplaceAmount);
				amount = InvLerp(_ShadowThreshold, lerp(1, _ShadowThreshold, _ShadowColorIntensity), 1-grayscale);
				c = lerp(c, targetColor, amount);

				c *= IN.color;
				if (!_CutOffIgnoresColor) clip(c.a - _CutOff);

				if (_Opaque) c.a = 1;
				if (_Emissive) c.rgb = IN.color.rgb;
				if (_ColorAlphaMultiply) c.rgb *= c.a;
				if (c.r < _BlackValue) c.r = _BlackValue;
				if (c.g < _BlackValue) c.g = _BlackValue;
				if (c.b < _BlackValue) c.b = _BlackValue;

				if (_ReplaceColor) c.rgb = _Color.rgb;
				if (_ReplaceAlpha) c.a = _Color.a;

				return c;
			}
			ENDCG
		}
	}
}
