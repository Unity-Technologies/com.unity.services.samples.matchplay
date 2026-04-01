Shader "Custom/HyperrealisticPlatformShader"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (0.5, 0.5, 0.5, 1)
        _Metallic("Metallic", Range(0, 1)) = 0.2
        _Roughness("Roughness", Range(0, 1)) = 0.8
        _DetailAlbedoMap("Detail Albedo Map", 2D) = "white" {}
        _DetailNormalMap("Detail Normal Map", 2D) = "bump" {}
        _DetailSmoothnessMask("Detail Smoothness Mask", 2D) = "white" {}
        _DetailScale("Detail Scale", Float) = 5
        _NormalMap("Normal Map", 2D) = "bump" {}
        _ParallaxMap("Parallax Map", 2D) = "black" {}
        _ParallaxHeight("Parallax Height", Float) = 0.05
        _EmissiveColor("Emissive Color", Color) = (0, 0.1, 0.2, 1)
        _EmissiveIntensity("Emissive Intensity", Float) = 0.1

        [Header(Void Glitch)]
        _VoidResonance("Void Resonance", Range(0, 1)) = 0.5
        _GlitchFrequency("Glitch Frequency", Float) = 1.0
        _GlitchScale("Glitch Scale", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float2 detailUV : TEXCOORD3;
                float3 tangentWS : TEXCOORD4;
                float3 bitangentWS : TEXCOORD5;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                half _Metallic;
                half _Roughness;
                float _DetailScale;
                float _ParallaxHeight;
                float4 _EmissiveColor;
                float _EmissiveIntensity;
                float _VoidResonance;
                float _GlitchFrequency;
                float _GlitchScale;
            CBUFFER_END

            sampler2D _DetailAlbedoMap;
            sampler2D _DetailNormalMap;
            sampler2D _DetailSmoothnessMask;
            sampler2D _NormalMap;
            sampler2D _ParallaxMap;

            Varyings vert(Attributes input)
            {
                Varyings output;

                // --- VOID GLITCH LOGIC ---
                float time = _Time.y * _GlitchFrequency;
                float glitchNoiseX = sin(input.positionOS.y * _GlitchScale + time) * cos(input.positionOS.z * _GlitchScale - time);
                float glitchNoiseY = cos(input.positionOS.x * _GlitchScale - time) * sin(input.positionOS.z * _GlitchScale + time);
                float glitchNoiseZ = sin(input.positionOS.x * _GlitchScale + time) * cos(input.positionOS.y * _GlitchScale - time);

                float3 glitchDisplacement = input.normalOS * float3(glitchNoiseX, glitchNoiseY, glitchNoiseZ) * _VoidResonance * 0.2;
                input.positionOS.xyz += glitchDisplacement;

                // --- STANDARD TRANSFORMS ---
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;
                output.detailUV = input.uv * _DetailScale;
                output.tangentWS = TransformObjectToWorldDir(input.tangentOS.xyz);
                output.bitangentWS = cross(output.normalWS, output.tangentWS) * (input.tangentOS.w * GetOddNegativeScale());

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 color = _BaseColor;
                half3 detailAlbedo = tex2D(_DetailAlbedoMap, input.detailUV).rgb;
                color.rgb *= detailAlbedo;

                half3 emissive = _EmissiveColor.rgb * _EmissiveIntensity;
                color.rgb += emissive;

                return color;
            }
            ENDHLSL
        }
    }
}
