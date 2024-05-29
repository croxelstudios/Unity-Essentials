Shader "Custom/ScreenspaceTexture/Unlit"
{
    //show values to edit in inspector
    Properties
    {
        _Color("Tint", Color) = (1, 1, 1, 1)
        _MainTex("Texture", 2D) = "white" {}
        _Rotation("Rotation", Range(-180, 180)) = 0
        _Aspect("Aspect", Float) = 1

        [Enum(Off,0,Front,1,Back,2)] _Cull("Cull", Int) = 2
        _CutOff("CutOff", Range(0, 1)) = 0.1
        [MaterialToggle] _CutOffIgnoresColor("Cutoff ignores material color", Float) = 0
        [MaterialToggle] _zWrite("Z Write", Float) = 1
        [Enum(Less,0,Greater,1,LEqual,2,GEqual,3,Equal,4,NotEqual,5,Always,6)]
        _zTest("Z Test", Int) = 2
        [MaterialToggle] _ColorAlphaMultiply("ColorAlphaMultiply", Float) = 0
    }

    SubShader
    {
        //the material is completely non-transparent and is rendered at the same time as the other opaque geometry
        Tags { "Queue" = "Transparent"}
        Cull[_Cull]
        ZWrite[_zWrite] //TO DO: Point lights stop working when zWrite is On and zTest is LEqual
        ZTest[_zTest]
        Lighting Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM

            //include useful shader functions
            #include "UnityCG.cginc"

            //define vertex and fragment shader
            #pragma vertex vert
            #pragma fragment frag

            //texture and transforms of the texture
            sampler2D _MainTex;
            float4 _MainTex_ST;

            //tint of the texture
            fixed4 _Color;
            bool _ColorAlphaMultiply;
            bool _CutOffIgnoresColor;
            float _CutOff;

            //the object data that's put into the vertex shader
            struct appdata
            {
                float4 vertex : POSITION;
            };

            //the data that's used to generate fragments and can be read by the fragment shader
            struct v2f
            {
                float4 position : SV_POSITION;
                float4 screenPosition : TEXCOORD0;
            };

            //the vertex shader
            v2f vert(appdata v)
            {
                v2f o;
                //convert the vertex positions from object space to clip space so they can be rendered
                o.position = UnityObjectToClipPos(v.vertex);
                o.screenPosition = ComputeScreenPos(o.position);
                return o;
            }

            //Rotation-related data
            UNITY_INSTANCING_BUFFER_START(MirrorReflection)
                UNITY_DEFINE_INSTANCED_PROP(float, _Rotation)
                #define _Rotation_arr MirrorReflection
                UNITY_DEFINE_INSTANCED_PROP(float, _Aspect)
                #define _Aspect_arr MirrorReflection
            UNITY_INSTANCING_BUFFER_END(MirrorReflection)
            //

            //the fragment shader
            fixed4 frag(v2f i) : SV_TARGET
            {
                float2 textureCoordinate = i.screenPosition.xy / i.screenPosition.w;
                float aspect = _ScreenParams.x / _ScreenParams.y;
                textureCoordinate.x = textureCoordinate.x * aspect;
                float2 uv = TRANSFORM_TEX(textureCoordinate, _MainTex);

                //Rotation
                float2 cuv = float2(0.5 * aspect + _MainTex_ST.z, 0.5 + _MainTex_ST.w);

                float rotation = UNITY_ACCESS_INSTANCED_PROP(_Rotation_arr, _Rotation);
                float aspectt = UNITY_ACCESS_INSTANCED_PROP(_Aspect_arr, _Aspect);

                float2 v = uv - cuv;
                v.x *= aspectt;
                float r = radians(rotation);
                float cs = cos(r);
                float sn = sin(r);
                uv = float2((v.x * cs - v.y * sn) / aspectt, v.x * sn + v.y * cs) + cuv.xy;
                //

                fixed4 col = tex2D(_MainTex, uv);

                if (_CutOffIgnoresColor) clip(col.a - _CutOff);
                col *= _Color;
                if (!_CutOffIgnoresColor) clip(col.a - _CutOff);
                if (_ColorAlphaMultiply) col.rgb *= col.a;

                return col;
            }

            ENDCG
        }
    }
}
