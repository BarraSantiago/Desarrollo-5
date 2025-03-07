Shader "Custom/OutlineShaderURP" {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineThickness ("Outline Thickness", Float) = 0.03
    }
    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        // First pass: render the outline by extruding vertices along normals.
        Pass {
            Name "Outline"
            Tags { "LightMode"="UniversalForward" }
            // Cull front faces so that the expanded back faces are visible.
            Cull Front
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 position : POSITION;
                float3 normal   : NORMAL;
            };

            struct Varyings {
                float4 position : SV_POSITION;
            };

            float _OutlineThickness;
            float4 _OutlineColor;

            Varyings vertOutline(Attributes input) {
                Varyings output;
                // Extrude the vertex along its normal by _OutlineThickness
                float3 offset = input.normal * _OutlineThickness;
                // Transform to clip space after offsetting
                output.position = TransformObjectToHClip(input.position + float4(offset, 0));
                return output;
            }

            half4 fragOutline(Varyings input) : SV_Target {
                return _OutlineColor;
            }
            ENDHLSL
        }

        // Second pass: render the original object normally.
        Pass {
            Name "Base"
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vertBase
            #pragma fragment fragBase
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes {
                float4 position : POSITION;
                float2 uv       : TEXCOORD0;
            };

            struct Varyings {
                float4 position : SV_POSITION;
                float2 uv       : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            Varyings vertBase(Attributes input) {
                Varyings output;
                output.position = TransformObjectToHClip(input.position);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            half4 fragBase(Varyings input) : SV_Target {
                return SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
            }
            ENDHLSL
        }
    }
    FallBack "Universal Forward"
}