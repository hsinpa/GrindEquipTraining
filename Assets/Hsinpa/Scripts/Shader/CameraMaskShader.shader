Shader "Hsinpa/CameraMaskShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseNoise("Base Noise", 2D) = "white" {}

        _AlphaStrength("Alpha Strength", Range(0, 1)) = 0
    }

    SubShader
    {
        // Geometry-100 => drawn before all normal opaque objects
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry-100" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Cull Off      // double-sided, visible from inside the sphere
            ZWrite On
            ZTest Always  // the mask itself always draws over whatever is there

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
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_BaseNoise);
            SAMPLER(sampler_BaseNoise);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseNoise_ST;
                float _AlphaStrength;
                
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // Force depth to the near clip plane (with a tiny epsilon so it
                // isn't clipped away). Everything rendered afterwards fails the
                // depth test, so nothing can appear in front of this mask.
                #if UNITY_REVERSED_Z
                    OUT.positionHCS.z = OUT.positionHCS.w * 0.999999;
                #else
                    OUT.positionHCS.z = -OUT.positionHCS.w * 0.999999;
                #endif

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseNoise);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(
                    _BaseNoise,
                    sampler_BaseNoise,
                    IN.uv
                );

                // clip(0) is not discarded, so explicitly handle zero strength.
                if (_AlphaStrength <= 0.0)
                    discard;

                clip(_AlphaStrength - color.r);

                return color * _BaseColor;
            }
            ENDHLSL
        }
    }
}
