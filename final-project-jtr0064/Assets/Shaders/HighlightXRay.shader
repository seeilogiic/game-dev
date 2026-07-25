// Flat-color unlit shader that always passes the depth test, so it draws through walls
// and the target's own mesh. Used by HighlightRing for Ability Two (item Highlight, gold) and
// Ability Four (Dropoff Highlight, light blue) - the whole point of these abilities is to show
// the player where nearby resources/dropoffs are, so the glow shouldn't be hidden just because
// its hitbox sits inside the visible model.
Shader "Custom/HighlightXRay"
{
    Properties
    {
        _BaseColor("Color", Color) = (1, 0.82, 0.15, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "XRay"
            Tags { "LightMode"="UniversalForward" }

            ZWrite Off
            ZTest Always
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}
