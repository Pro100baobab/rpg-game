Shader "MagicalFX/Rim_URP"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0)
        _RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // --- Структуры данных ---
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uvMain      : TEXCOORD0;
                float2 uvBump      : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float3 tangentWS   : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float3 viewDirWS   : TEXCOORD5;
            };

            // --- Текстуры и параметры ---
            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BumpMap_ST;
                float4 _RimColor;
                float  _RimPower;
            CBUFFER_END

            // --- Вершинный шейдер ---
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);

                // UV с тайлингом/оффсетом
                OUT.uvMain = IN.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                OUT.uvBump = IN.uv * _BumpMap_ST.xy + _BumpMap_ST.zw;

                // Мировые нормаль, тангент, битангент
                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS    = normalInput.normalWS;
                OUT.tangentWS   = normalInput.tangentWS;
                OUT.bitangentWS = normalInput.bitangentWS;

                // Направление взгляда в мировом пространстве
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.viewDirWS = GetWorldSpaceViewDir(positionWS);

                return OUT;
            }

            // --- Фрагментный шейдер ---
            half4 frag(Varyings IN) : SV_Target
            {
                // Альбедо
                half4 baseMap = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uvMain);
                half3 albedo = baseMap.rgb;

                // Карта нормалей (UnpackNormalScale корректно распаковывает DXT5nm и обычные normal map)
                half3 normalTS = UnpackNormalScale(
                    SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, IN.uvBump), 1.0
                );

                // Переход из касательного пространства в мировое
                half3x3 TBN = half3x3(IN.tangentWS, IN.bitangentWS, IN.normalWS);
                half3 normalWS = normalize(mul(normalTS, TBN));

                // Диффузное освещение (Ламберт) от главного источника света
                Light mainLight = GetMainLight();
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = albedo * mainLight.color * NdotL;

                // Фоновое освещение (Light Probes / Skybox)
                half3 ambient = SampleSH(normalWS) * albedo;

                // Rim-свечение (эмиссия, как в оригинале)
                half rim = 1.0 - saturate(dot(normalize(IN.viewDirWS), normalWS));
                half3 rimEmission = _RimColor.rgb * pow(rim, _RimPower);

                half3 finalColor = diffuse + ambient + rimEmission;
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}