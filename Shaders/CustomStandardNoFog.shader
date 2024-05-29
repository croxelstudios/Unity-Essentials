Shader "Custom/StandardNoFog"
{
    Properties
    {
        _Color("Main Color", Color) = (1,1,1,1)
        _MainTex("Base (RGB)", 2D) = "white" {}
        [MaterialToggle] _Emissive("Emissive", Float) = 0
        [MaterialToggle] PixelSnap("Pixel snap", Float) = 0
        [MaterialToggle] _NormalsDirectionUp("Normals Direction Up", Float) = 0
        [Enum(Off,0,Front,1,Back,2)] _Cull("Cull", Int) = 2
        _CutOff("CutOff", Range(0, 1)) = 0.1
        [MaterialToggle] _CutOffIgnoresColor("Cutoff ignores material color", Float) = 0
        [MaterialToggle] _zWrite("Z Write", Float) = 1
        [Enum(Less,0,Greater,1,LEqual,2,GEqual,3,Equal,4,NotEqual,5,Always,6)]
        _zTest("Z Test", Int) = 2
        [MaterialToggle] _ColorAlphaMultiply("ColorAlphaMultiply", Float) = 0

        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Cull[_Cull]
        ZWrite[_zWrite] //TO DO: Point lights stop working when zWrite is On and zTest is LEqual
        ZTest[_zTest]
        Lighting Off
        Blend One OneMinusSrcAlpha

        CGPROGRAM
        #pragma target 2.0
        #pragma multi_compile_instancing
        #pragma multi_compile_local _ PIXELSNAP_ON
        #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
        #pragma surface surf Lambert vertex:vert keepalpha addshadow nofog
            // TO DO: Should probably use custom lighting function instead of btwo passes
        #include "Assets/Utilities - HypercubeCore/Essentials/Shaders/CGIncludes/HcCBaseCG.cginc" //_MainTex, _Color, _AlphaTex

        // Global Shader values
        uniform float2 _BendAmount;
        uniform float3 _BendOrigin;
        uniform float _BendFalloff;

        struct c_appdata_full
        {
            float4 vertex    : POSITION;  // The vertex position in model space.
            float3 normal    : NORMAL;    // The vertex normal in model space.
            float4 texcoord  : TEXCOORD0; // The first UV coordinate.
            float4 texcoord1 : TEXCOORD1; // The second UV coordinate.
            float4 texcoord2 : TEXCOORD2; // The second UV coordinate.
            float4 tangent   : TANGENT;   // The tangent vector in Model Space (used for normal mapping).
            float4 color     : COLOR;     // Per-vertex color
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        struct Input
        {
            float2 uv_MainTex;
            float4 color : COLOR;
        };

        bool _NormalsDirectionUp;

        void vert(inout c_appdata_full v)
        {
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(v);

            v.vertex = UnityFlipSprite(v.vertex, _Flip);
            v.texcoord = v.texcoord;
            v.color = v.color * _Color *_RendererColor;
            if (_NormalsDirectionUp) v.normal = float4(0, 1, 0, 0);

            #ifdef PIXELSNAP_ON
            v.vertex = UnityPixelSnap(v.vertex);
            #endif
        }

        bool _ColorAlphaMultiply;
        bool _CutOffIgnoresColor;
        bool _Emissive;
        float _CutOff;

        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = SampleSpriteTexture(IN.uv_MainTex);
            if (_CutOffIgnoresColor) clip(c.a - _CutOff);
            c *= IN.color;
            if (!_CutOffIgnoresColor) clip(c.a - _CutOff);

            if (_Emissive) c = IN.color;
            if (_ColorAlphaMultiply) c.rgb *= c.a;
            o.Albedo = c;
            o.Alpha = c.a;
        }

        ENDCG

        Pass
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }
            Blend One One
            Fog { Color (0,0,0,0) } // in additive pass fog should be black
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 3.0

            // -------------------------------------

            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #pragma shader_feature_local _METALLICGLOSSMAP
            #pragma shader_feature_local _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            #pragma shader_feature_local _SPECULARHIGHLIGHTS_OFF
            #pragma shader_feature_local _DETAIL_MULX2
            #pragma shader_feature_local _PARALLAXMAP

            #pragma multi_compile_fwdadd_fullshadows
            //#pragma multi_compile_fog
            // Uncomment the following line to enable dithering LOD crossfade. Note: there are more in the file to uncomment for other passes.
            //#pragma multi_compile _ LOD_FADE_CROSSFADE

            #pragma vertex vertAdd
            #pragma fragment fragAdd

            #ifndef UNITY_STANDARD_CORE_FORWARD_INCLUDED
            #define UNITY_STANDARD_CORE_FORWARD_INCLUDED

            #if defined(UNITY_NO_FULL_STANDARD_SHADER)
            #   define UNITY_STANDARD_SIMPLE 1
            #endif

            #include "UnityStandardConfig.cginc"

            bool _NormalsDirectionUp;
            float _CutOff;
            bool _zWrite;
            int _zTest;

            #if UNITY_STANDARD_SIMPLE //TO DO: Transparency and cutout don't work correctly with lights
                #include "UnityStandardCoreForwardSimple.cginc"
                VertexOutputForwardAddSimple vert(VertexInput v)
                {
                    if (_NormalsDirectionUp) v.normal = float4(0, 1, 0, 0);
                    return vertForwardAddSimple(v);
                }
                half4 fragAdd (VertexOutputForwardAddSimple i) : SV_Target
                {
                    fixed alpha = tex2D(_MainTex, i.tex).a;
                    clip(alpha - _CutOff);
                    half4 light = fragForwardAddSimpleInternal(i);
                    if ((!_zWrite) && (_zTest == 2)) return 0; //light * 0.25; //TO DO: This is a bit of a dirty patch
                    return light;
                }
            #else
                #include "UnityStandardCore.cginc"
                VertexOutputForwardAdd vertAdd (VertexInput v)
                {
                    if (_NormalsDirectionUp) v.normal = float4(0, 1, 0, 0);
                    return vertForwardAdd(v);
                }
                half4 fragAdd(VertexOutputForwardAdd i) : SV_Target
                {
                    fixed alpha = tex2D(_MainTex, i.tex).a;
                    clip(alpha - _CutOff);
                    half4 light = fragForwardAddInternal(i);
                    if ((!_zWrite) && (_zTest == 2)) return 0; //light * 0.25; //TO DO: This is a bit of a dirty patch
                    return light;
                }
            #endif

            #endif

            ENDCG
        }
    }

    Fallback "Transparent/VertexLit"
}
