Shader "Custom/URP/ScreenSpaceOutline"
{
    Properties
    {
        [MainTexture] _BaseMap(
            "Base Texture",
            2D
        ) = "white" {}

        [MainColor] _BaseColor(
            "Base Color",
            Color
        ) = (1, 1, 1, 1)

        _OutlineColor(
            "Outline Color",
            Color
        ) = (0, 0, 0, 1)

        _OutlineWidth(
            "Outline Width (Pixels)",
            Range(0, 20)
        ) = 3
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"     = "Opaque"
            "Queue"          = "Geometry"
        }

        HLSLINCLUDE

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            float4 _BaseMap_ST;
            half4  _BaseColor;
            half4  _OutlineColor;
            float  _OutlineWidth;
        CBUFFER_END

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);

        ENDHLSL

        // ------------------------------------------------------------
        // Regular object surface
        // ------------------------------------------------------------

        Pass
        {
            Name "ForwardUnlit"

            Tags
            {
                "LightMode" = "UniversalForwardOnly"
            }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #pragma target 2.0
            #pragma vertex SurfaceVertex
            #pragma fragment SurfaceFragment
            #pragma multi_compile_instancing

            struct SurfaceAttributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct SurfaceVaryings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            SurfaceVaryings SurfaceVertex(SurfaceAttributes input)
            {
                SurfaceVaryings output = (SurfaceVaryings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS =
                    TransformObjectToHClip(input.positionOS.xyz);

                output.uv =
                    input.uv * _BaseMap_ST.xy + _BaseMap_ST.zw;

                return output;
            }

            half4 SurfaceFragment(SurfaceVaryings input) : SV_Target
            {
                half4 textureColor = SAMPLE_TEXTURE2D(
                    _BaseMap,
                    sampler_BaseMap,
                    input.uv
                );

                return textureColor * _BaseColor;
            }

            ENDHLSL
        }

        // ------------------------------------------------------------
        // Expanded back-face shell
        // ------------------------------------------------------------

        Pass
        {
            Name "Outline"

            // URP uses this tag for additional unlit passes.
            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }

            // Render only the back faces of the expanded mesh.
            Cull Front

            ZWrite On
            ZTest LEqual

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM

            #pragma target 2.0
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            #pragma multi_compile_instancing

            struct OutlineAttributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct OutlineVaryings
            {
                float4 positionCS : SV_POSITION;

                UNITY_VERTEX_OUTPUT_STEREO
            };

            OutlineVaryings OutlineVertex(OutlineAttributes input)
            {
                OutlineVaryings output = (OutlineVaryings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(input.normalOS);

                float4 positionCS = positionInputs.positionCS;

                // Convert the normal to view space so its XY components
                // indicate the outline expansion direction on screen.
                float3 normalVS = TransformWorldToViewDir(
                    normalInputs.normalWS,
                    true
                );

                float2 screenDirection = normalVS.xy;

                // Avoid NaN values when a normal points directly toward
                // or away from the camera.
                float directionLengthSquared =
                    dot(screenDirection, screenDirection);

                screenDirection *= rsqrt(
                    max(directionLengthSquared, 0.000001)
                );

                // Convert the requested pixel width to clip-space units.
                float2 clipUnitsPerPixel = float2(
                    2.0 / _ScreenParams.x,
                    2.0 / _ScreenParams.y
                );

                positionCS.xy +=
                    screenDirection
                    * clipUnitsPerPixel
                    * _OutlineWidth
                    * positionCS.w;

                output.positionCS = positionCS;

                return output;
            }

            half4 OutlineFragment(OutlineVaryings input) : SV_Target
            {
                return _OutlineColor;
            }

            ENDHLSL
        }
    }

    Fallback "Hidden/Universal Render Pipeline/FallbackError"
}