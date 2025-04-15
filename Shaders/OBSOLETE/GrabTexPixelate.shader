Shader "Custom/GrabTexPixelate"
{
	Properties
	{
		_ResolutionX("Resolution X", Int) = 480
		_ResolutionY("Resolution Y", Int) = 270
		[Enum(RectangularPixels, 0, SquareFromX, 1, SquareFromY, 2)]
		_PixelsExpandMode("Pixels expand mode", Int) = 2
		_Precission("Precission", Int) = 1 //TO DO: Support for separate axes?
		//[HideInInspector][MaterialToggle] _InterpolateColors("Interpolate Colors", Float) = 1

		[Enum(Off,0,Front,1,Back,2)] _Cull("Cull", Int) = 2
		[MaterialToggle] _zWrite("Z Write", Float) = 0
		[Enum(Less,0,Greater,1,LEqual,2,GEqual,3,Equal,4,NotEqual,5,Always,6)]
		_zTest("Z Test", Int) = 2
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"DisableBatching" = "True"
		}

		Cull[_Cull]
		ZWrite[_zWrite]
		ZTest[_zTest]

		GrabPass { "_BackgroundTexture" }

		Pass
		{
			Name "Pixelate"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile _ PIXELSNAP_ON
			#include "UnityCG.cginc"
			#include "Assets/Utilities - AstrophelMoore/Essentials/Shaders/CGIncludes/HcCFunctionsCG.cginc"

			struct v2f
			{
				float4 pos : POSITION;
				float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			sampler2D _BackgroundTexture;
			float4 _BackgroundTexture_ST;

			v2f vert(v2f i)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_TRANSFER_INSTANCE_ID(i, o);
				o.pos = UnityObjectToClipPos(i.pos);
				o.uv = ComputeGrabScreenPos(o.pos / o.pos.w);
				return o;
			}

			int _ResolutionX;
			int _ResolutionY;
			int _PixelsExpandMode;
			int _Precission;

			fixed4 frag(v2f i) : SV_Target
			{
				float4 screenTexelSize = float4(1 / _ScreenParams.x, 1 / _ScreenParams.y, _ScreenParams.x, _ScreenParams.y);

				if (_PixelsExpandMode == 1)
				{ _ResolutionY = _ResolutionX * screenTexelSize.z / screenTexelSize.w; }
				else if (_PixelsExpandMode == 2)
				{ _ResolutionX = _ResolutionY * screenTexelSize.z / screenTexelSize.w; }
				int2 pixelate = int2(_ResolutionX, _ResolutionY);
				return Pixelate(_BackgroundTexture, screenTexelSize, i.uv, pixelate, _Precission);
			}
		ENDCG
		}
	}
}
