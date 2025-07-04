//#include "UnitySprites.cginc"

#ifndef UNITY_SPRITES_INCLUDED
#define UNITY_SPRITES_INCLUDED

#ifdef UNITY_INSTANCING_ENABLED
    // Inicio del cbuffer de instancing para SpriteRenderer
    UNITY_INSTANCING_BUFFER_START(PerDrawSprite)
        // Array de colores por instancia
        UNITY_DEFINE_INSTANCED_PROP(float4, unity_SpriteRendererColorArray)
        // Array de flips (X/Y) por instancia
        UNITY_DEFINE_INSTANCED_PROP(float2, unity_SpriteFlipArray)
    UNITY_INSTANCING_BUFFER_END(PerDrawSprite)

    // Macro que accede al elemento correspondiente de unity_SpriteRendererColorArray
    #define _RendererColor UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteRendererColorArray)
    #define _Flip          UNITY_ACCESS_INSTANCED_PROP(PerDrawSprite, unity_SpriteFlipArray)
#endif // UNITY_INSTANCING_ENABLED

// Búfer para cuando NO hay instancing activo
CBUFFER_START(UnityPerDrawSprite)
#ifndef UNITY_INSTANCING_ENABLED
    float4 _RendererColor;
    float2 _Flip;
#endif
    float _EnableExternalAlpha;
CBUFFER_END

// … resto del include …
#endif // UNITY_SPRITES_INCLUDED

void GetSpriteRendererColor_float(float InstanceID, out float4 Out)
{
    Out = float4(1,1,1,1);
    
    #if defined(_RendererColor)
         Out *= SRGBToLinear(_RendererColor);
    #endif

    #if defined(unity_SpriteColor)
        Out *= unity_SpriteColor;
    #endif
}