Shader "Hidden/Hsinpa/Mask Outline/Composite"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest Always

        HLSLINCLUDE
        #pragma target 3.0
        #pragma vertex Vert
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        #define MAX_OUTLINE_WIDTH 12

        float _OutlineWidth;
        half4 _OutlineColor;

        half4 ExpandHorizontal(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 uv = input.texcoord;
            float centerMask = SAMPLE_TEXTURE2D_X_LOD(
                _BlitTexture, sampler_PointClamp, uv, _BlitMipLevel).r;
            float expandedMask = centerMask;
            int width = clamp((int)round(_OutlineWidth), 1, MAX_OUTLINE_WIDTH);

            [unroll]
            for (int offset = -MAX_OUTLINE_WIDTH; offset <= MAX_OUTLINE_WIDTH; offset++)
            {
                if (abs(offset) <= width)
                {
                    float2 sampleUV = uv + float2(offset * _BlitTexture_TexelSize.x, 0.0);
                    expandedMask = max(expandedMask, SAMPLE_TEXTURE2D_X_LOD(
                        _BlitTexture, sampler_PointClamp, sampleUV, _BlitMipLevel).r);
                }
            }

            // Red stores horizontal dilation. Green preserves the original mask so
            // the next pass can remove the object's interior without another texture.
            return half4(expandedMask, centerMask, 0.0, 1.0);
        }

        half4 ExpandVerticalAndColor(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 uv = input.texcoord;
            half2 center = SAMPLE_TEXTURE2D_X_LOD(
                _BlitTexture, sampler_PointClamp, uv, _BlitMipLevel).rg;
            float expandedMask = center.r;
            int width = clamp((int)round(_OutlineWidth), 1, MAX_OUTLINE_WIDTH);

            [unroll]
            for (int offset = -MAX_OUTLINE_WIDTH; offset <= MAX_OUTLINE_WIDTH; offset++)
            {
                if (abs(offset) <= width)
                {
                    float2 sampleUV = uv + float2(0.0, offset * _BlitTexture_TexelSize.y);
                    expandedMask = max(expandedMask, SAMPLE_TEXTURE2D_X_LOD(
                        _BlitTexture, sampler_PointClamp, sampleUV, _BlitMipLevel).r);
                }
            }

            float edge = saturate(expandedMask - center.g);
            return half4(_OutlineColor.rgb, _OutlineColor.a * edge);
        }
        ENDHLSL

        Pass
        {
            Name "Expand Horizontal"
            Blend Off

            HLSLPROGRAM
            #pragma fragment ExpandHorizontal
            ENDHLSL
        }

        Pass
        {
            Name "Expand Vertical and Composite"
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma fragment ExpandVerticalAndColor
            ENDHLSL
        }
    }
}
