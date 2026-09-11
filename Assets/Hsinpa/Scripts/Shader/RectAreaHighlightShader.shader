Shader "Hsinpa/RectAreaHighlightShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        _Strength("Strength", Range(0, 2)) = 1
        _HighLightHeight("Highlight Height", Range(0, 1)) = 0
        _FadeRange("Fade Range", Range(0, 1)) = 0.2
        _HeightEpsilon("Height Epsilon", Float) = 0.0001

        // Object-space mesh bounds
        _BoundMinY("Bound Min Y", Float) = -0.5
        _BoundMaxY("Bound Max Y", Float) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float posY : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _Strength;
                float _HighLightHeight;
                float _FadeRange;
                float _HeightEpsilon;
                float _BoundMinY;
                float _BoundMaxY;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.posY = IN.positionOS.y;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                // Normalize object-space Y to 0..1 within the mesh bounds
                float normalizedY = saturate((IN.posY - _BoundMinY) / max(_BoundMaxY - _BoundMinY, 0.0001));

                // Opaque up to the fade start, then fades linearly to fully
                // transparent at _HighLightHeight; nothing shows above it
                float fadeStart = _HighLightHeight - _FadeRange;
                float gradient = 1.0 - saturate((normalizedY - fadeStart) / max(_FadeRange, _HeightEpsilon));
                color.a *= gradient * _Strength;

                return color;
            }
            ENDHLSL
        }
    }
}
