Shader "Custom/Scanlines" {

	Properties
	{
		_MainTex("_Color", 2D) = "white" {}
		_AmountOfLines("Amount of Lines", Float) = 4
		_Hardness("Hardness", Float) = 0.9
		_Speed("Displacement Speed", Float) = 0.1
		[MaterialToggle] _UseUnscaledTime("Use unscaled time uniform", Float) = 1
	}

	SubShader
	{
		Tags {"IgnoreProjector" = "True" "Queue" = "Transparent"}

		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			Fog{ Mode off }

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "UnityCG.cginc"
			#pragma target 3.0

			struct v2f
			{
				float4 pos      : POSITION;
				float2 uv       : TEXCOORD0;
				float4 scr_pos : TEXCOORD1;
			};

			sampler2D _MainTex;
			float _AmountOfLines;
			float _Hardness;
			float _Speed;
			bool _UseUnscaledTime;
			uniform float4 _UnscaledTime;

			v2f vert(appdata_img v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uv = MultiplyUV(UNITY_MATRIX_TEXTURE0, v.texcoord);
				o.scr_pos = ComputeScreenPos(o.pos);

				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				half4 color = tex2D(_MainTex, i.uv); //TO DO: Add _Color functionality maybe
				//Get line size in pixels, use _LineWidth directly for measuring in pixels instead of percentage
				fixed lineSize = _ScreenParams.y / _AmountOfLines;
				//Displacement in pixels per second
				float time;
				if (_UseUnscaledTime) time = _UnscaledTime.y;
				else time = _Time.y;
				float displacement = (time * _Speed) % _ScreenParams.y;
				float ps = displacement + (i.scr_pos.y * _ScreenParams.y / _ScreenParams.w); //<- This bit here was being divided by _ScreenParams.w and I dont know why

				return ((uint)(floor(ps / lineSize)) % 2 == 0) ? color : color * float4(_Hardness, _Hardness, _Hardness, 1);
			}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
