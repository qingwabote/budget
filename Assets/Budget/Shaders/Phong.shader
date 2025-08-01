Shader "Budget/Phong"
{
    Properties
    {
        _BaseColor("Base Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                // float2 texcoord : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                // float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                half3 normalWS : TEXCOORD2;
                float4 positionHCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            Varyings vert (Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float4 positionWS = mul(GetObjectToWorldMatrix(), input.positionOS);
                
                Varyings output = (Varyings)0;
                output.positionWS = positionWS.xyz;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionHCS = mul(GetWorldToHClipMatrix(), positionWS);
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                Light light = GetMainLight();

                half3 diffuse = LightingLambert(light.color, light.direction, input.normalWS);

                half3 viewDir = GetWorldSpaceNormalizeViewDir(input.positionWS);
                half3 specular = LightingSpecular(light.color, light.direction, input.normalWS, viewDir, half4(0.5, 0.5, 0.5, 1.0), 16.0);

                return LinearToSRGB(_BaseColor * float4(diffuse + specular + 0.3, 1.0));
            }
            ENDHLSL
        }
    }
}
