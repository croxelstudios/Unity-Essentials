Shader "Hidden/RToDepth"
{
    Properties { _MainTex ("RHalfTex", 2D) = "white" {} } 

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            ZWrite On
            ZTest Always
            ColorMask 0
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

            struct Attributes { float2 uv : TEXCOORD0; float4 pos : SV_POSITION; };
            Attributes Vert (uint id : SV_VertexID)
            {
                Attributes o;
                o.pos = float4((id==2)?3:-1, (id==1)?3:-1, 0, 1);
                o.uv  = o.pos.xy * 0.5 + 0.5;
                return o;
            }

            texture2D _MainTex;
            SamplerState sampler_MainTex;
            void Frag(Attributes IN, out float depth : SV_Depth)
            {
                float r = _MainTex.Sample(sampler_MainTex, IN.uv).r;
                #ifdef UNITY_REVERSED_Z
                    r = 1 - r;
                #endif
                depth = r;
            }
            ENDHLSL
        }
    }
}
