Shader "Custom/SkillTreeDarkness"
{
    Properties
    {
        _Color ("Darkness Color", Color) = (0, 0, 0, 1.0)
        _CenterUV ("Center UV", Vector) = (0.5, 0.5, 0, 0)
        _LightRadius ("Light Radius", Float) = 0.05
        _LightSoftness ("Light Softness", Float) = 0.45
        _LightIntensity ("Light Intensity", Float) = 1.0
        _LightColor ("Light Color", Color) = (1, 0.92, 0.75, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent+1"
            "RenderType" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex SkillTreeDarknessVertex
            #pragma fragment SkillTreeDarknessFragment
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _CenterUV;
                float _LightRadius;
                float _LightSoftness;
                float _LightIntensity;
                half4 _LightColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings SkillTreeDarknessVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }

            half4 SkillTreeDarknessFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float2 uv = input.uv;
                float2 centerUV = _CenterUV.xy;
                float dist = distance(uv, centerUV);

                // Gaussian-like falloff for smooth natural light decay
                float normalizedDist = max(dist - _LightRadius, 0.0) / max(_LightSoftness, 0.001);
                float light = exp(-normalizedDist * normalizedDist * 2.0);
                light = saturate(light * _LightIntensity);

                float darknessAlpha = _Color.a * input.color.a;
                float finalAlpha = darknessAlpha * (1.0 - light);

                // Subtle warm tint near the light edge
                half3 finalColor = lerp(_Color.rgb, _LightColor.rgb, light * light * 0.4);
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }

        Pass
        {
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex SkillTreeDarknessVertexForward
            #pragma fragment SkillTreeDarknessFragmentForward
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
                float4 _CenterUV;
                float _LightRadius;
                float _LightSoftness;
                float _LightIntensity;
                half4 _LightColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings SkillTreeDarknessVertexForward(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.color = input.color;
                output.uv = input.uv;
                return output;
            }

            half4 SkillTreeDarknessFragmentForward(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                float2 uv = input.uv;
                float2 centerUV = _CenterUV.xy;
                float dist = distance(uv, centerUV);

                // Gaussian-like falloff for smooth natural light decay
                float normalizedDist = max(dist - _LightRadius, 0.0) / max(_LightSoftness, 0.001);
                float light = exp(-normalizedDist * normalizedDist * 2.0);
                light = saturate(light * _LightIntensity);

                float darknessAlpha = _Color.a * input.color.a;
                float finalAlpha = darknessAlpha * (1.0 - light);

                // Subtle warm tint near the light edge
                half3 finalColor = lerp(_Color.rgb, _LightColor.rgb, light * light * 0.4);
                return half4(finalColor, finalAlpha);
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
