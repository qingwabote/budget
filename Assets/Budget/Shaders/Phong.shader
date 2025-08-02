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

            #pragma shader_feature _USE_SKINNING

            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #if defined(_USE_SKINNING)
            #include "Assets/Budget/Shaders/Includes/Skinning.hlsl"
            #endif

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                // float2 texcoord : TEXCOORD0;

                #if defined(_USE_SKINNING)
                uint4 indices : BLENDINDICES;
                float4 weights : BLENDWEIGHTS;
                #endif

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

            #if defined(_USE_SKINNING)
            UNITY_INSTANCING_BUFFER_START(PerInstance)
                UNITY_DEFINE_INSTANCED_PROP(float, _JointOffset)
            UNITY_INSTANCING_BUFFER_END(PerInstance)

            TEXTURE2D(_JointMap);
            float4 _JointMap_TexelSize;
            #endif

            Varyings vert (Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float3 positionOS = input.positionOS.xyz;
                #if defined(_USE_SKINNING)
                SkinningDeform(positionOS, input.indices, input.weights, _JointMap, _JointMap_TexelSize.z, UNITY_ACCESS_INSTANCED_PROP(PerInstance, _JointOffset));
                #endif
                float3 positionWS = TransformObjectToWorld(positionOS);

                Varyings output = (Varyings)0;
                output.positionWS = positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.positionHCS = TransformWorldToHClip(positionWS);
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
