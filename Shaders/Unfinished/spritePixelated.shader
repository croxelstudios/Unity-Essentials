Shader "Unlit/spritePixelated"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ResolutionX("Resolution X", Int) = 480
        _CameraSize("Camera Size", Float) = 5
       
        [Header(Sprite MetaData)]
        _UVCenter ("_UVCenter", Vector) = (0,0,0,0)
       
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"
        "DisableBatching"="True"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
 
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
 
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
 
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
 
            sampler2D _MainTex;
            float4 _MainTex_ST;
       
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }
       
            float2 quant(float2 q, float2 v)
            {
                return floor(q*v)/v;
            }
           
            float2 quantToWorld(float2 value, float2 pixelate)
            {
                float2 wp = mul(unity_ObjectToWorld, float4(value, 0, 0));
                wp = quant(wp, pixelate);
                return mul(unity_WorldToObject, float4(wp, 0, 0));
            }

            int _ResolutionX;
            half4 _UVCenter;
            float _CameraSize;
           
            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                int2 pixelate = int2(_ResolutionX, _ResolutionX * _ScreenParams.y / _ScreenParams.x);
                float2 cameraSize = float2(_CameraSize * _ScreenParams.x / _ScreenParams.y, _CameraSize) * 2;
                uv = quantToWorld(uv-_UVCenter.xy, pixelate / cameraSize) + _UVCenter.xy;
               
                fixed4 col = tex2D(_MainTex, uv);
                clip(col.a-.001);
                return col;
            }
            ENDCG
        }
    }
}