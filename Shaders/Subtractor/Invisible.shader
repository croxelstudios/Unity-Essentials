Shader "Custom/Invisible"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            ZWrite Off
            ZTest Never

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            void vert ()
            {
            }

            void frag ()
            {
            }
            ENDCG
        }
    }
}
