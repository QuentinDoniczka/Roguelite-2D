Shader "Custom/SpriteOutline2D"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineEnabled ("Outline Enabled", Float) = 0
        _OutlineColor ("Outline Color", Color) = (0,1,0,1)
        _OutlineWidth ("Outline Width", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex SpriteOutlineVertex
            #pragma fragment SpriteOutlineFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                half4 _Color;
                half _OutlineEnabled;
                half4 _OutlineColor;
                half _OutlineWidth;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings SpriteOutlineVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            half4 SpriteOutlineFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 tinted = baseColor * input.color;

                if (_OutlineEnabled > 0.5)
                {
                    float2 texelStep = _OutlineWidth * _MainTex_TexelSize.xy;

                    static const float2 OFFSETS[8] = {
                        float2( 1,  0),
                        float2(-1,  0),
                        float2( 0,  1),
                        float2( 0, -1),
                        float2( 1,  1),
                        float2( 1, -1),
                        float2(-1,  1),
                        float2(-1, -1)
                    };

                    if (baseColor.a < 0.01)
                    {
                        half neighborAlphaMax = 0;

                        for (int i = 0; i < 8; i++)
                        {
                            float2 neighborUV = input.uv + OFFSETS[i] * texelStep;
                            half neighborAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, neighborUV).a;
                            neighborAlphaMax = max(neighborAlphaMax, neighborAlpha);
                        }

                        if (neighborAlphaMax > 0.01)
                        {
                            return half4(_OutlineColor.rgb, _OutlineColor.a * input.color.a);
                        }
                    }
                }

                return tinted;
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex SpriteOutlineVertexForward
            #pragma fragment SpriteOutlineFragmentForward
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _MainTex_TexelSize;
                half4 _Color;
                half _OutlineEnabled;
                half4 _OutlineColor;
                half _OutlineWidth;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings SpriteOutlineVertexForward(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color * _Color;
                return output;
            }

            half4 SpriteOutlineFragmentForward(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half4 baseColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 tinted = baseColor * input.color;

                if (_OutlineEnabled > 0.5)
                {
                    float2 texelStep = _OutlineWidth * _MainTex_TexelSize.xy;

                    static const float2 OFFSETS[8] = {
                        float2( 1,  0),
                        float2(-1,  0),
                        float2( 0,  1),
                        float2( 0, -1),
                        float2( 1,  1),
                        float2( 1, -1),
                        float2(-1,  1),
                        float2(-1, -1)
                    };

                    if (baseColor.a < 0.01)
                    {
                        half neighborAlphaMax = 0;

                        for (int i = 0; i < 8; i++)
                        {
                            float2 neighborUV = input.uv + OFFSETS[i] * texelStep;
                            half neighborAlpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, neighborUV).a;
                            neighborAlphaMax = max(neighborAlphaMax, neighborAlpha);
                        }

                        if (neighborAlphaMax > 0.01)
                        {
                            return half4(_OutlineColor.rgb, _OutlineColor.a * input.color.a);
                        }
                    }
                }

                return tinted;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
